using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Features.Auth.Commands.RefreshToken;
using FoodConnect.Backend.Application.Features.Auth.Commands.Register;
using FoodConnect.Backend.Application.Features.Auth.Queries.Login;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiBaseController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserQuery query)
        {
            var result = await Mediator.Send(query);

            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);

            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command)
        {
            command.RefreshToken = Request.Cookies["refreshToken"];

            var result = await Mediator.Send(command);

            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);

            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var response = new
            {
                Id = userId,
                Email = email,
                FullName = fullName
            };
            return Ok(response);
        }

        #region Private methods
        private void SetRefreshTokenCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
        #endregion

    }
}
