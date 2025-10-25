using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum ShopStatusEnum
    {
        Draft = 0,              // Vừa tạo, chưa submit
        PendingApproval = 1,    // Đã submit, chờ admin duyệt
        Active = 2,             // Đã duyệt, đang hoạt động
        Rejected = 3,           // Bị từ chối bởi admin
        Suspended = 4,          // Bị tạm ngưng
        Closed = 5              // Đã đóng cửa
    }
}
