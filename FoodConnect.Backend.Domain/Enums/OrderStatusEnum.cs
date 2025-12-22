using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum OrderStatusEnum
    {
        Pending = 0,
        Preparing = 1,
        ReadyForPickup = 2,
        DeliveryingBySeller = 3,
        DeliveryingByShipper = 4,
        Delivered = 5,  
        Completed = 6,
        Cancelled = 7,
        Returned = 8,
        AwaitingPayment = 9,
        Disputing = 10,
        Rejected = 11
    }
}
