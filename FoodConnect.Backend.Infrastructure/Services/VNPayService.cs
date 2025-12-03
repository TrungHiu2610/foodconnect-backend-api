using FoodConnect.Backend.Application.Commons.DTOs.Requests.Payment;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace FoodConnect.Backend.Infrastructure.Services;

public class VNPayService : IVNPayService
{
    private readonly ISystemConfigRepository _configRepository;
    private readonly IConfiguration _configuration;

    public VNPayService(
        ISystemConfigRepository configRepository,
        IConfiguration configuration)
    {
        _configRepository = configRepository;
        _configuration = configuration;
    }

    public async Task<VNPayPaymentUrlResponse> CreatePaymentUrlAsync(VNPayCreatePaymentRequest request)
    {
        var tmnCode = _configuration["VNPay:TmnCode"];
        var hashSecret = _configuration["VNPay:HashSecret"];
        var vnpUrl = _configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        var locale = _configuration["VNPay:Locale"] ?? "vn";
        var currency = _configuration["VNPay:Currency"] ?? "VND";
        var orderType = _configuration["VNPay:OrderType"] ?? "other";
        var version = _configuration["VNPay:Version"] ?? "2.1.0";
        var command = _configuration["VNPay:Command"] ?? "pay";
        var returnUrl = _configuration["VNPay:ReturnUrl"] ?? "https://glossemic-jarrett-irreverent.ngrok-free.dev/api/Payment/VNPayCallback";
        
        var createDate = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var txnRef = DateTime.Now.Ticks.ToString();
        var amount = ((long)(request.Amount * 100)).ToString();

        var vnpay = new Dictionary<string, string>
        {
            { "vnp_Version", version },
            { "vnp_Command", command },
            { "vnp_TmnCode", tmnCode },
            { "vnp_Amount", amount },
            { "vnp_CreateDate", createDate},
            { "vnp_CurrCode", currency },
            { "vnp_IpAddr", request.IpAddress },
            { "vnp_Locale", locale },
            { "vnp_OrderInfo", request.OrderInfo },
            { "vnp_OrderType", orderType },
            { "vnp_ReturnUrl", returnUrl },
            { "vnp_TxnRef", txnRef }
        };

        var rawData = VNPayHelper.BuildRawData(vnpay);
        
        var vnpSecureHash = VNPayHelper.HmacSHA512(hashSecret, rawData);

        var queryString = rawData;
        
        var paymentUrl = $"{vnpUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";

        return new VNPayPaymentUrlResponse
        {
            PaymentUrl = paymentUrl,
            TransactionId = txnRef
        };
    }

    public VNPayCallbackResponse ProcessCallback(Dictionary<string, string> vnpayData)
    {
        var response = new VNPayCallbackResponse
        {
            RawData = vnpayData,
            TransactionId = vnpayData.GetValueOrDefault("vnp_TxnRef", string.Empty),
            OrderId = vnpayData.GetValueOrDefault("vnp_OrderInfo", string.Empty),
            ResponseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode", string.Empty),
            OrderInfo = vnpayData.GetValueOrDefault("vnp_OrderInfo", string.Empty)
        };

        if (vnpayData.ContainsKey("vnp_Amount") && long.TryParse(vnpayData["vnp_Amount"], out var amount))
        {
            response.Amount = amount / 100m;
        }

        response.Success = response.ResponseCode == "00";
        response.Message = GetResponseMessage(response.ResponseCode);

        return response;
    }

    public bool ValidateSignature(Dictionary<string, string> vnpayData, string secureHash)
    {
        var hashSecret = _configuration["VNPay:HashSecret"];

        var filteredData = vnpayData
            .Where(kv => kv.Key.StartsWith("vnp_") && kv.Key != "vnp_SecureHash")
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var rawData = VNPayHelper.BuildRawData(filteredData);
        var checksum = VNPayHelper.HmacSHA512(hashSecret, rawData);

        return checksum.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
            "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch.",
            "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "70" => "Giao dịch không thành công do: Chữ ký không hợp lệ (Invalid signature).",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch",
            _ => "Giao dịch thất bại"
        };
    }
}
