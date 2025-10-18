using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string ExpiredAccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
