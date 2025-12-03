using Microsoft.EntityFrameworkCore.Storage;

namespace FoodConnect.Backend.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackTransactionAsync(IDbContextTransaction transaction);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
