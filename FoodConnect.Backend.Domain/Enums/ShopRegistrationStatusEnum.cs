using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum ShopRegistrationStatusEnum
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        CancelledByUser = 3
    }
}
