using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Features.Complaint.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.RejectComplaint;

public class RejectComplaintCommandHandler : IRequestHandler<RejectComplaintCommand, BaseResponse<ComplaintDetailDto>>
{
    private readonly IOrderComplaintRepository _complaintRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ComplaintNotificationService _notificationService;

    public RejectComplaintCommandHandler(
        IOrderComplaintRepository complaintRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ComplaintNotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<ComplaintDetailDto>> Handle(RejectComplaintCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<ComplaintDetailDto>();

        // Check authentication
        if (!_currentUserService.UserId.HasValue)
        {
            return result.BuildUnauthorized("User must be logged in");
        }

        var adminId = _currentUserService.UserId.Value;

        // Check admin role
        if (_currentUserService.Role != "Admin")
        {
            return result.BuildForbidden("Only admins can reject complaints");
        }

        // Get complaint with full details
        var complaint = await _complaintRepository.GetComplaintWithDetailsAsync(request.ComplaintId);
        if (complaint == null)
        {
            return result.BuildNotFound("Complaint not found");
        }

        // Validate complaint status
        if (complaint.Status != OrderComplaintStatusEnum.PendingAdmin && 
            complaint.Status != OrderComplaintStatusEnum.SellerResponded)
        {
            return result.BuildFail("Only complaints pending admin decision can be rejected");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Update complaint
            complaint.Status = OrderComplaintStatusEnum.Rejected;
            complaint.IsApproved = false;
            complaint.ApprovedRefundAmount = 0;
            complaint.AdminDecisionReason = request.RejectionReason;
            complaint.AdminDecidedAt = DateTime.UtcNow;
            complaint.AdminId = adminId;
            complaint.CompletedAt = DateTime.UtcNow;

            _complaintRepository.Update(complaint);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Reload complaint with updated details
            complaint = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
            var complaintDto = ComplaintMapper.MapToDetailDto(complaint!);

            // Send notifications to both buyer and seller
            await _notificationService.NotifyComplaintRejectedAsync(complaint!, cancellationToken);

            return result.BuildSuccess(complaintDto, "Complaint rejected successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Failed to reject complaint: {ex.Message}");
        }
    }
}
