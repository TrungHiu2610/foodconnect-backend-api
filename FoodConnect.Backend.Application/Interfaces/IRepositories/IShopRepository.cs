using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IShopRepository : IBaseRepository<Shop>
    {
        Task<Shop?> GetByUserIdAsync(Guid userId);
        Task<Shop?> GetDetailByIdAsync(Guid id);
        Task<(IEnumerable<Shop> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Domain.Enums.ShopStatusEnum? status = null,
            string? searchTerm = null);
        Task<List<Guid>> GetAllCategoryIdsForShopAsync(Guid shopId);
        IQueryable<Shop> GetShopsAsQueryable();
    }
}
