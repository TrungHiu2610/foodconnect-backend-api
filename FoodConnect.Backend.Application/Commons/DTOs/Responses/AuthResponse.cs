using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses
{
    public record AuthResponse(Guid UserId, string Email, string FullName, List<string> Roles, string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAtUtc, Guid? ShopId);
}
