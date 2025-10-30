namespace FoodConnect.Backend.Domain.Enums
{
    public enum NotificationTypeEnum
    {
        // Order related
        NewOrder = 0,
        OrderAccepted = 1,
        OrderRejected = 2,
        OrderPreparing = 3,
        OrderOutForDelivery = 4,
        OrderDelivered = 5,
        OrderCompleted = 6,
        OrderCancelled = 7,
        
        // Shop related
        ShopApproved = 100,
        ShopRejected = 101,
        
    // Product related
    ProductOutOfStock = 200,
    ProductBackInStock = 201,
        
        // System
        SystemAnnouncement = 300
    }
}
