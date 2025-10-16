using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetByIdAsync(Guid id);
        IQueryable<Product> GetProductsAsQueryable();
        Task<Product?> GetProductWithAssetsAsync(Guid id, bool tracking = true);
    }
}
