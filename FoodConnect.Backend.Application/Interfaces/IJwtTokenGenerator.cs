using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        public (string accessToken, RefreshToken refreshToken) GenerateTokens(User user, string role);
    }
}
