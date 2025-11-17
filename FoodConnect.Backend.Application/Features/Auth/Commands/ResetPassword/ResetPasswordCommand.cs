using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<BaseResponse<PasswordResetResponse>>
    {
        public string Email { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
    }
}
