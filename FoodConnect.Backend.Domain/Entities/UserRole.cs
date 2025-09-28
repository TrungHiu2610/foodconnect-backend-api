using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public RoleEnum RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
