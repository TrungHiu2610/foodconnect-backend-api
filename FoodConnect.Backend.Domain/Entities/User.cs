using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; } // "Male", "Female", "Other"
        public UserStatusEnum Status { get; set; } = UserStatusEnum.Pending;
        public AuthProviderEnum Provider { get; set; } = AuthProviderEnum.Local;
        public DateTime? LastPasswordChangedAt { get; set; }
        
        public virtual Shop? Shop { get; set; }
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
        public virtual Cart? Cart { get; set; }
    }
}
