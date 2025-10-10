using Microsoft.EntityFrameworkCore.Storage;

namespace FoodConnect.Backend.Application.Interfaces
{
    public interface IUnitOfWork
    {
        // begin transaction
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackTransactionAsync(IDbContextTransaction transaction);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
