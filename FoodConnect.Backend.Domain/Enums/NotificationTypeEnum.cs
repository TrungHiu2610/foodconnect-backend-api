namespace FoodConnect.Backend.Domain.Enums
{
    public enum NotificationTypeEnum
    {
        NewOrder = 0,
        OrderAccepted = 1,
        OrderRejected = 2,
        OrderPreparing = 3,
        OrderOutForDelivery = 4,
        OrderDelivered = 5,
        OrderCompleted = 6,
        OrderCancelled = 7,

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
