using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class VerifyPhoneOtpCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
    }
}
