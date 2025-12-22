namespace FoodConnect.Backend.Domain.Enums
{
    public enum NotificationTypeEnum
    {
        NewOrder = 0,
        OrderAccepted = 1,
        OrderRejected = 2,
        OrderPreparing = 3,
        OrderReadyForPickup = 4,
        OrderOutForDelivery = 5,
        OrderDelivered = 6,
        OrderCompleted = 7,
        OrderCancelled = 8,

        ShopApproved = 100,
        ShopRejected = 101,

        ProductOutOfStock = 200,
        ProductBackInStock = 201,

        SystemAnnouncement = 300,

        WithdrawalRequest = 400,
        WithdrawalApproved = 401,
        WithdrawalRejected = 402,
        WithdrawalResolved = 403,

        ComplaintCreated = 500,
        ComplaintUpdated = 501,
        ComplaintResolved = 502
    }
}
