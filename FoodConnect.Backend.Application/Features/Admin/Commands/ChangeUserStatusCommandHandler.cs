using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Commands
{
    public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, BaseResponse<UserStatusChangeResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ChangeUserStatusCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<UserStatusChangeResponse>> Handle(
            ChangeUserStatusCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<UserStatusChangeResponse>();

            var adminUserId = _currentUserService.UserId;
            if (adminUserId == null)
            {
                return result.BuildUnauthorized("Admin user not found");
            }

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return result.BuildNotFound("User not found");
            }

            var oldStatus = user.Status;
            if (oldStatus == request.NewStatus)
            {
                return result.BuildFail("User already has this status");
            }

            var adminUser = await _userRepository.GetByIdAsync(adminUserId.Value);
            if (adminUser == null)
            {
                return result.BuildUnauthorized("Admin user not found");
            }

            try
            {
                user.Status = request.NewStatus;
                user.UpdatedAtUtc = DateTime.UtcNow;
                _userRepository.Update(user);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = new UserStatusChangeResponse
                {
                    UserId = user.Id,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    ChangedBy = adminUser.FullName,
                    ChangedAt = DateTime.UtcNow,
                    Reason = request.Reason
                };

                return result.BuildSuccess(response, "User status changed successfully");
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Failed to change user status: {ex.Message}");
            }
        }
    }
}
