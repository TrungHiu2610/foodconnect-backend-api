using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class RegisterWithEmailCommand : IRequest<BaseResponse<OtpSentResponse>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }
}
