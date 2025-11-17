using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<BaseResponse<OtpSentResponse>>
    {
        public string Email { get; set; }
    }
}
