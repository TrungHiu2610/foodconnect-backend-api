using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class SendPhoneOtpCommand : IRequest<BaseResponse<object>>
    {
        public string PhoneNumber { get; set; }
    }
}
