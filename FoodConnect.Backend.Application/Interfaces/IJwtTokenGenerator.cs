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
        Task<(string accessToken, RefreshToken refreshToken)> GenerateTokens(User user, List<string> roleNames, Guid? shopId);
    }
}
