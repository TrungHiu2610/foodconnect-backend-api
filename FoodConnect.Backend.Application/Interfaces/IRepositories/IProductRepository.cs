using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetByIdAsync(Guid id);
        IQueryable<Product> GetProductsAsQueryable();
        Task<Product?> GetProductWithAssetsAsync(Guid id, bool tracking = true);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedByShopIdAsync(
            Guid shopId,
            int page,
            int pageSize,
            ProductStatusEnum? status = null,
            string? searchTerm = null);
        Task<IEnumerable<Product>> GetAllProductsWithDetailsAsync();
    }
}
