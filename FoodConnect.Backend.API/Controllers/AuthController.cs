using FoodConnect.Backend.API.Models;
using FoodConnect.Backend.Application.Features.Auth.Commands.RefreshToken;
using FoodConnect.Backend.Application.Features.Auth.Commands.Register;
using FoodConnect.Backend.Application.Features.Auth.Queries.Login;
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
            return Ok(BaseResponse<object>.BuildSuccess(result, "Register success"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserQuery query)
        {
            var result = await Mediator.Send(query);

            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

            return Ok(BaseResponse<object>.BuildSuccess(result, "Login success"));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command)
        {
            command.RefreshToken = Request.Cookies["refreshToken"];

            var result = await Mediator.Send(command);

            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

            return Ok(BaseResponse<object>.BuildSuccess(result, "Renew token success"));
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var result = new
            {
                Id = userId,
                Email = email,
                Roles = roles
            };
            return Ok(BaseResponse<object>.BuildSuccess(result, "User info retrieved successfully"));
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
