using System.Collections;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Base;
using Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private Hashtable _repositories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        if (_repositories == null) _repositories = new Hashtable();

        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repoType = typeof(Repository<>);
            var repoInstance = Activator.CreateInstance(repoType.MakeGenericType(typeof(TEntity)), _context);

            _repositories.Add(type, repoInstance);
        }

        return (IRepository<TEntity>)_repositories[type];
    }
}