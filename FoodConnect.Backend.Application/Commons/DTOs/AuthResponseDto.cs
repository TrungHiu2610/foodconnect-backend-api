using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs
{
    public record AuthResponseDto(Guid UserId, string Email, string Role, string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
}
