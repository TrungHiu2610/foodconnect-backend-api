using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class UserStatusAuditLogRepository : BaseRepository<UserStatusAuditLog>, IUserStatusAuditLogRepository
    {
        public UserStatusAuditLogRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UserStatusAuditLog>> GetAuditLogsByUserIdAsync(Guid userId)
        {
            return await _context.UserStatusAuditLogs
                .Include(a => a.ChangedByUser)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ChangedAtUtc)
                .ToListAsync();
        }
    }
}
