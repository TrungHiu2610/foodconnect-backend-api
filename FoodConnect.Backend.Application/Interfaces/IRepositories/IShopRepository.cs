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
    }
}
