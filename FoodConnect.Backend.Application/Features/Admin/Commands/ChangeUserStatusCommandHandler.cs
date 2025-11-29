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
        private readonly IUserStatusAuditLogRepository _auditLogRepository;

        public ChangeUserStatusCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IUserStatusAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogRepository = auditLogRepository;
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

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                user.Status = request.NewStatus;
                user.UpdatedAtUtc = DateTime.UtcNow;
                _userRepository.Update(user);

                var auditLog = new UserStatusAuditLog
                {
                    UserId = user.Id,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    ChangedByUserId = adminUserId.Value,
                    Reason = request.Reason,
                    ChangedAtUtc = DateTime.UtcNow
                };

                await _auditLogRepository.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                var response = new UserStatusChangeResponse
                {
                    UserId = user.Id,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    ChangedBy = adminUser.FullName,
                    ChangedAt = auditLog.ChangedAtUtc,
                    Reason = request.Reason
                };

                return result.BuildSuccess(response, "User status changed successfully");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction);
                throw;
            }
        }
    }
}
