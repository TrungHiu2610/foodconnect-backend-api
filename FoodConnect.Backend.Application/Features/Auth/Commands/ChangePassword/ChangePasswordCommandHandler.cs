using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, BaseResponse<PasswordChangeResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private const int PASSWORD_CHANGE_COOLDOWN_DAYS = 7;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<PasswordChangeResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PasswordChangeResponse>();

            var user = await _userRepository.GetByIdAsync((Guid)request.UserId);
            if (user == null)
            {
                return result.BuildNotFound("User not found");
            }

            if (user.Provider != AuthProviderEnum.Local)
            {
                return result.BuildFail($"This account uses {user.Provider} login. Password change is not available.");
            }

            if (user.Status == UserStatusEnum.Banned)
            {
                return result.BuildFail("Your account has been banned");
            }

            if (user.Status == UserStatusEnum.Locked)
            {
                return result.BuildFail("Your account has been locked");
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return result.BuildFail("Account does not have a password set");
            }

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                return result.BuildFail("Current password is incorrect");
            }

            if (user.LastPasswordChangedAt.HasValue)
            {
                var daysSinceLastChange = (DateTime.UtcNow - user.LastPasswordChangedAt.Value).TotalDays;
                if (daysSinceLastChange < PASSWORD_CHANGE_COOLDOWN_DAYS)
                {
                    var nextAllowedDate = user.LastPasswordChangedAt.Value.AddDays(PASSWORD_CHANGE_COOLDOWN_DAYS);
                    var daysRemaining = Math.Ceiling(PASSWORD_CHANGE_COOLDOWN_DAYS - daysSinceLastChange);
                    return result.BuildFail($"You can only change your password once every {PASSWORD_CHANGE_COOLDOWN_DAYS} days. Please try again in {daysRemaining} day(s) (after {nextAllowedDate:yyyy-MM-dd HH:mm} UTC)");
                }
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.PasswordHash = newPasswordHash;
            user.LastPasswordChangedAt = DateTime.UtcNow;
            
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var response = new PasswordChangeResponse
            {
                Email = user.Email ?? string.Empty,
                ChangedAt = user.LastPasswordChangedAt.Value,
                NextAllowedChangeAt = user.LastPasswordChangedAt.Value.AddDays(PASSWORD_CHANGE_COOLDOWN_DAYS),
                RequiresRelogin = true
            };

            return result.BuildSuccess(response, "Password changed successfully. Please login again with your new password.");
        }
    }
}
