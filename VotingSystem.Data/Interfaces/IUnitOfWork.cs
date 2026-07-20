using Microsoft.EntityFrameworkCore.Storage;
using SchMgr_FUTO.Data.Interfaces;

namespace VotingSystem.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        int SaveChanges();

        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    public interface IUnitofWork<TContext> : IUnitOfWork
    {
    }
}