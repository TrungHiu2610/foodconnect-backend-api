using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum UserStatusEnum
    {
        Pending = 0,
        Active = 1,
        Banned = 2,
        Locked = 3
    }
}
