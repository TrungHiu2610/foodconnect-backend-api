using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class UserStatusAuditLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public UserStatusEnum OldStatus { get; set; }
        public UserStatusEnum NewStatus { get; set; }
        
        public Guid ChangedByUserId { get; set; }
        public virtual User ChangedByUser { get; set; } = null!;
        
        public string? Reason { get; set; }
        public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
