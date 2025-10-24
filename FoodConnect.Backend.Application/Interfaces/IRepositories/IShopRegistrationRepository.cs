using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IShopRegistrationRepository : IBaseRepository<ShopRegistration>
    {
        Task<ShopRegistration?> GetPendingOrApprovedByUserIdAsync(Guid userId);
        Task<(IEnumerable<ShopRegistration> Items, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            ShopRegistrationStatusEnum? status = null,
            string? searchTerm = null);
    }
}
