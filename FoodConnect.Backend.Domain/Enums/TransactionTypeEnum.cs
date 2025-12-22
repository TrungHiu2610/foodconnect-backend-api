namespace FoodConnect.Backend.Domain.Enums;

public enum TransactionTypeEnum
{
    OrderEarning = 0,
    CommissionDeduction = 1,
    Withdraw = 2,
    Refund = 3,
    Adjustment = 4,
    TopUp = 5,
    OrderPayment = 6,
    CashbackReceived = 7,
    ComplaintRefund = 8,     // Refund từ complaint (buyer nhận)
    ComplaintDeduction = 9   // Trừ tiền seller khi complaint approved
}
