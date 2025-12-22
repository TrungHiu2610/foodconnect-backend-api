namespace FoodConnect.Backend.Application.Commons.Constants;
public static class WithdrawalNotificationMessages
{
    
    public const string ADMIN_NEW_REQUEST_TITLE = "Yêu cầu rút tiền mới";
    
    public static string AdminNewRequestMessage(string sellerName, decimal amount)
        => $"{sellerName} đã tạo yêu cầu rút {amount:N0} VND";

    
    public const string SELLER_APPROVED_TITLE = "Đã duyệt rút tiền";
    public const string SELLER_APPROVED_MESSAGE = "Yêu cầu rút tiền của bạn đã được duyệt và hoàn thành. Vui lòng kiểm tra tài khoản.";
    
    public const string SELLER_REJECTED_TITLE = "Từ chối rút tiền";
    
    public static string SellerRejectedMessage(string reason)
        => $"Yêu cầu rút tiền của bạn đã bị từ chối. Lý do: {reason}";

    
    public const string SELLER_ISSUE_RESOLVED_TITLE = "Đã giải quyết vấn đề rút tiền";
    public const string SELLER_ISSUE_RESOLVED_MESSAGE = "Vấn đề rút tiền của bạn đã được giải quyết. Vui lòng kiểm tra lại.";
}
public static class TransactionDescriptions
{
    
    public static string WithdrawalPending(string withdrawalId)
        => $"Yêu cầu rút tiền #{withdrawalId.Substring(0, 8)}";
    
    public static string WithdrawalApproved(string withdrawalId)
        => $"Rút tiền #{withdrawalId.Substring(0, 8)} - Đã duyệt bởi admin";
    
    public static string WithdrawalCompleted(string withdrawalId)
        => $"Rút tiền #{withdrawalId.Substring(0, 8)} - Hoàn thành và đã chuyển tiền";
    
    public static string WithdrawalRejected(string withdrawalId, string reason)
        => $"Rút tiền #{withdrawalId.Substring(0, 8)} - Từ chối: {reason}";
    
    public static string WithdrawalCancelled(string withdrawalId)
        => $"Rút tiền #{withdrawalId.Substring(0, 8)} - Đã hủy bởi seller";

    
    public static string OrderEarning(string orderId)
        => $"Thu nhập từ đơn hàng #{orderId.Substring(0, 8)}";
    
    public static string CommissionDeduction(string orderId, decimal rate)
        => $"Khấu trừ hoa hồng {rate:P0} từ đơn hàng #{orderId.Substring(0, 8)}";
    
    public static string OrderRefund(string orderId)
        => $"Hoàn trả từ đơn hàng #{orderId.Substring(0, 8)}";

    
    public const string BALANCE_ADJUSTMENT = "Điều chỉnh số dư ví";
    
    public static string BalanceAdjustmentWithReason(string reason)
        => $"Điều chỉnh số dư ví - Lý do: {reason}";
}
public static class WithdrawalValidationMessages
{
    public const string WITHDRAWAL_NOT_FOUND = "Không tìm thấy yêu cầu rút tiền";
    public const string UNAUTHORIZED = "Bạn không có quyền thực hiện hành động này";
    public const string INVALID_STATUS = "Trạng thái không hợp lệ để thực hiện hành động này";
    public const string INSUFFICIENT_BALANCE = "Số dư không đủ để rút tiền";
    public const string MIN_AMOUNT_NOT_MET = "Số tiền rút không đạt yêu cầu tối thiểu";
    
    public static string OnlyStatusCanBeProcessed(string currentStatus, string requiredStatus)
        => $"Chỉ có thể xử lý yêu cầu ở trạng thái {requiredStatus}. Trạng thái hiện tại: {currentStatus}";
    
    public static string ProofImageRequired()
        => "Ảnh chứng từ là bắt buộc khi duyệt yêu cầu rút tiền";
    
    public static string RejectionReasonRequired()
        => "Lý do từ chối là bắt buộc khi từ chối yêu cầu rút tiền";
}
public static class WithdrawalSuccessMessages
{
    public const string CREATE_SUCCESS = "Tạo yêu cầu rút tiền thành công";
    public const string CANCEL_SUCCESS = "Hủy yêu cầu rút tiền thành công";
    public const string APPROVE_SUCCESS = "Duyệt và hoàn thành yêu cầu rút tiền thành công";
    public const string REJECT_SUCCESS = "Từ chối yêu cầu rút tiền thành công";
    public const string REPORT_ISSUE_SUCCESS = "Gửi báo cáo vấn đề thành công";
    public const string RESOLVE_ISSUE_SUCCESS = "Giải quyết vấn đề thành công";
}
