using Core.Entities;

namespace Core.Interfaces.Base;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

    Task<int> CompleteAsync();
}