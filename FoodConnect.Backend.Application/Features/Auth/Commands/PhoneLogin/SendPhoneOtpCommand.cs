using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class SendPhoneOtpCommand : IRequest<BaseResponse<OtpSentResponse>>
    {
        public string PhoneNumber { get; set; }
    }
}
