using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FoodConnect.Backend.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackTransactionAsync(IDbContextTransaction transaction);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
