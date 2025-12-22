using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<BaseResponse<PasswordChangeResponse>>
    {
        public Guid? UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
