using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum OrderComplaintStatusEnum
    {
        PendingSeller = 0,      // Buyer vừa tạo, chờ seller phản hồi (2 ngày)
        SellerResponded = 1,    // Seller đã phản hồi, chuyển cho admin
        PendingAdmin = 2,       // Chờ admin duyệt
        Approved = 3,           // Admin chấp nhận khiếu nại
        Rejected = 4,           // Admin từ chối khiếu nại
        Completed = 5           // Đã xử lý xong tiền (refund hoàn tất)
    }
}
