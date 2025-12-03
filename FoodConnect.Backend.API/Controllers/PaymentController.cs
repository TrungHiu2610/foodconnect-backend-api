using FoodConnect.Backend.API.Controllers;
using FoodConnect.Backend.Application.Features.Payment.Commands;
using FoodConnect.Backend.Application.Features.Payment.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FoodConnect.Backend.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PaymentController : ApiBaseController
{
    private readonly IConfiguration _configuration;

    public PaymentController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateVNPayPayment([FromBody] CreateVNPayPaymentCommand command)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
        command.IpAddress = ipAddress;

        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> VNPayCallback([FromQuery] Dictionary<string, string> vnpayData)
    {
        var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
        
        var command = new VNPayCallbackCommand { VnpayData = vnpayData };
        var result = await Mediator.Send(command);

        var responseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode", "");
        var transactionId = vnpayData.GetValueOrDefault("vnp_TxnRef", "");

        if (result != null && result.Success && responseCode == "00")
        {
            var orderId = result.Data?.OrderId ?? Guid.Empty;
            return Redirect($"{frontendUrl}/payment/success?orderId={orderId}");
        }
        else
        {
            var message = GetVNPayErrorMessage(responseCode);
            var redirectUrl = $"{frontendUrl}/payment/failed?message={Uri.EscapeDataString(message)}";
            
            if (result?.Data?.OrderId != null && result.Data.OrderId != Guid.Empty)
            {
                redirectUrl += $"&orderId={result.Data.OrderId}";
            }
            
            return Redirect(redirectUrl);
        }
    }
    private string GetVNPayErrorMessage(string responseCode)
    {
        return responseCode switch
        {
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Thẻ/Tài khoản chưa đăng ký InternetBanking",
            "10" => "Xác thực thông tin không đúng quá 3 lần",
            "11" => "Đã hết hạn chờ thanh toán",
            "12" => "Thẻ/Tài khoản bị khóa",
            "13" => "Nhập sai mật khẩu xác thực (OTP)",
            "24" => "Khách hàng hủy giao dịch",
            "51" => "Tài khoản không đủ số dư",
            "65" => "Đã vượt quá hạn mức giao dịch trong ngày",
            "75" => "Ngân hàng đang bảo trì",
            "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định",
            _ => "Giao dịch thất bại"
        };
    }

    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentTransaction([FromRoute] Guid orderId)
    {
        var query = new GetPaymentTransactionQuery { OrderId = orderId };
        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
