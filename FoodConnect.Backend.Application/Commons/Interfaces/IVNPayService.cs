using FoodConnect.Backend.Application.Commons.DTOs.Requests.Payment;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;

namespace FoodConnect.Backend.Application.Commons.Interfaces;

public interface IVNPayService
{
    Task<VNPayPaymentUrlResponse> CreatePaymentUrlAsync(VNPayCreatePaymentRequest request);
    VNPayCallbackResponse ProcessCallback(Dictionary<string, string> vnpayData);
    bool ValidateSignature(Dictionary<string, string> vnpayData, string secureHash);
}
