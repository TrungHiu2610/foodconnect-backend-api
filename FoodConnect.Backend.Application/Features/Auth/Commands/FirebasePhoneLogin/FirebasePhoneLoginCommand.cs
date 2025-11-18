using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.FirebasePhoneLogin
{
    public class FirebasePhoneLoginCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string IdToken { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }
}
