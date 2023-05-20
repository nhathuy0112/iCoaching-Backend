using Core.Entities;
using Core.Specifications;

namespace Core.Interfaces.Base;

public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task<T?> GetBySpecificationAsync(ISpecification<T> specification);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification);
    Task<int> CountAsync(ISpecification<T> specification);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(ISpecification<T> specification);
}