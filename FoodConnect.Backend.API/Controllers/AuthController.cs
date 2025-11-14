using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister;
using FoodConnect.Backend.Application.Features.Auth.Commands.ForgotPassword;
using FoodConnect.Backend.Application.Features.Auth.Commands.GoogleLogin;
using FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin;
using FoodConnect.Backend.Application.Features.Auth.Commands.RefreshToken;
using FoodConnect.Backend.Application.Features.Auth.Commands.Register;
using FoodConnect.Backend.Application.Features.Auth.Commands.ResetPassword;
using FoodConnect.Backend.Application.Features.Auth.Queries.Login;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ApiBaseController
    {
        #region Phone + OTP Authentication

        [HttpPost]
        public async Task<IActionResult> SendPhoneOtp(SendPhoneOtpCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPhoneOtp(VerifyPhoneOtpCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Data == null)
            {
                return BadRequest(result);
            }
            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion

        #region Google OAuth Authentication

        [HttpPost]
        public async Task<IActionResult> GoogleLogin(GoogleLoginCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Data == null)
            {
                return BadRequest(result);
            }
            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion

        #region Email + Password Authentication

        [HttpPost]
        public async Task<IActionResult> RegisterWithEmail(RegisterWithEmailCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailOtp(VerifyEmailOtpCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Data == null)
            {
                return BadRequest(result);
            }
            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> LoginWithEmail(LoginUserQuery query)
        {
            var result = await Mediator.Send(query);
            if (result.Data == null)
            {
                return BadRequest(result);
            }
            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion

        #region Legacy Authentication (Deprecated - use LoginWithEmail instead)

        [HttpPost]
        [Obsolete("Use RegisterWithEmail instead")]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Obsolete("Use LoginWithEmail instead")]
        public async Task<IActionResult> Login(LoginUserQuery query)
        {
            var result = await Mediator.Send(query);
            if(result.Data == null)
            {
                return BadRequest(result);
            }
            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);

            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion

        #region Token Management

        [HttpPost]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command)
        {
            if (string.IsNullOrEmpty(command.RefreshToken))
            {
                command.RefreshToken = Request.Cookies["refreshToken"];
            }

            var result = await Mediator.Send(command);
            if (result.Data == null)
            {
                return BadRequest(result);
            }
            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiresAtUtc);

            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion

        #region User Info

        [HttpGet]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var shopId = User.FindFirstValue("shopId"); 
            
            // Get all roles from claims
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var response = new
            {
                Id = userId,
                Email = email,
                FullName = fullName,
                Roles = roles,
                ShopId = !string.IsNullOrEmpty(shopId) ? Guid.Parse(shopId) : (Guid?)null 
            };
            return Ok(response);
        }

        #endregion

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
