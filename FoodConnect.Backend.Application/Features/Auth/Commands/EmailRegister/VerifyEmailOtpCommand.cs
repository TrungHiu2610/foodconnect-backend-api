using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class VerifyEmailOtpCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
