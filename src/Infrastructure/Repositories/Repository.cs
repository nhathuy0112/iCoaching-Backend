using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Specifications;
using Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public async Task<int> CountAsync(ISpecification<T> specification)
    {
        return await ApplySpecification(specification)
                .CountAsync();
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void DeleteRange(ISpecification<T> specification)
    {
        _context.Set<T>().RemoveRange(ApplySpecification(specification));
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetBySpecificationAsync(ISpecification<T> specification)
    {
        return await ApplySpecification(specification)
                .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
    {
        return await ApplySpecification(specification)
                .ToListAsync();
    }

    public void Update(T entity)
    {
        _context.Set<T>().Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), specification);
    }
}