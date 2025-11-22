using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum OrderStatusEnum
    {
        Pending = 0,           // Đơn hàng mới tạo, chờ seller xác nhận
        Preparing = 1,         // Seller đã chấp nhận và đang chuẩn bị
        ReadyForPickup = 2,    // Đã chuẩn bị xong, chờ chọn phương thức giao hàng
        DeliveryingBySeller = 3, // Seller tự giao hàng (Express only)
        DeliveryingByShipper = 4, // Shipper đang giao (Express only)
        OutForDelivery = 5,    // Đang giao hàng (Standard)
        Delivered = 6,         // Đã giao hàng (chờ buyer xác nhận)
        Completed = 7,         // Buyer đã xác nhận nhận hàng
        Cancelled = 8,         // Đã hủy
        Returned = 9,          // Đã trả hàng
        AwaitingPayment = 10   // Chờ thanh toán online (VNPay/MoMo)
    }
}
