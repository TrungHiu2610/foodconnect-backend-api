using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IPaymentTransactionRepository : IBaseRepository<PaymentTransaction>
{
    Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId);
    Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId);
    Task<List<PaymentTransaction>> GetByBuyerIdAsync(Guid buyerId, int pageNumber, int pageSize);
}
