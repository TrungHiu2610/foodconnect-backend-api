using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string IdToken { get; set; }
    }
}
