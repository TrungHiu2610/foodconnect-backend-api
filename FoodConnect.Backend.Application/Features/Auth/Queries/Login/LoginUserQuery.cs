using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Auth.Queries.Login
{
    public class LoginUserQuery : IRequest<BaseResponse<AuthResponse>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
