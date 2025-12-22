namespace FoodConnect.Backend.Domain.Enums
{
    public enum ReviewStatusEnum
    {
        Pending = 0,      // Đang chờ kiểm duyệt
        Approved = 1,     // Review hợp lệ (positive/negative đều OK)
        Toxic = 2,        // Review độc hại/chửi thề
        Spam = 3          // Review ảo/spam
    }
}
