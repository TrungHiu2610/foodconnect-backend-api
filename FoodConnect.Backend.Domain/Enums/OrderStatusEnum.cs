using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum OrderStatusEnum
    {
        Pending,
        Preparing,
        OutForDelivery,
        Delivered,
        Completed,
        Cancelled,
        Returned
    }
}
