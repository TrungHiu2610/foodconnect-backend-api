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
        Prepared = 2,
        ReadyForPickup = 3,
        DeliveryingBySeller = 4,
        DeliveryingByShipper = 5,
        OutForDelivery = 6,
        Delivered = 7,
        Completed = 8,
        Cancelled = 9,
        Returned = 10,
        AwaitingPayment = 11,
        Disputing = 12,
        Rejected = 13
    }
}
