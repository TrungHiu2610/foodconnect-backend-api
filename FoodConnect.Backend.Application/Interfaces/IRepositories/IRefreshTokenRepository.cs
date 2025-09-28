using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(Guid userId);
        Task<RefreshToken?> GetByTokenAsync(string refreshToken);
    }
}
