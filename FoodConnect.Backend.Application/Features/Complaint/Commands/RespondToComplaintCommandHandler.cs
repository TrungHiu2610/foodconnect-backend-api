using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Features.Complaint.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands
{
    public class RespondToComplaintCommandHandler : IRequestHandler<RespondToComplaintCommand, BaseResponse<ComplaintDetailDto>>
    {
        private readonly IOrderComplaintRepository _complaintRepository;
        private readonly IOrderComplaintAssetRepository _assetRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ComplaintNotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        private const int MaxEvidenceFiles = 5;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".mp4", ".mov" };
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
        private const int ResponseDeadlineHours = 48;

        public RespondToComplaintCommandHandler(
            IOrderComplaintRepository complaintRepository,
            IOrderComplaintAssetRepository assetRepository,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService,
            ComplaintNotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _complaintRepository = complaintRepository;
            _assetRepository = assetRepository;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ComplaintDetailDto>> Handle(
            RespondToComplaintCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ComplaintDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var sellerId = _currentUserService.UserId.Value;

            // Validate evidence files
            if (request.EvidenceFiles != null && request.EvidenceFiles.Count > MaxEvidenceFiles)
            {
                return result.BuildFail($"Maximum {MaxEvidenceFiles} evidence files allowed");
            }

            if (request.EvidenceFiles != null)
            {
                foreach (var file in request.EvidenceFiles)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(extension))
                    {
                        return result.BuildFail($"File type {extension} is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
                    }

                    if (file.Length > MaxFileSize)
                    {
                        return result.BuildFail($"File {file.FileName} exceeds maximum size of 50MB");
                    }
                }
            }

            // Get complaint with details
            var complaint = await _complaintRepository.GetComplaintWithDetailsAsync(request.ComplaintId);
            if (complaint == null)
            {
                return result.BuildNotFound("Complaint not found");
            }

            // Validate seller owns the complaint
            if (complaint.SellerId != sellerId)
            {
                return result.BuildForbidden("You don't have permission to respond to this complaint");
            }

            // Validate complaint status
            if (complaint.Status != OrderComplaintStatusEnum.PendingSeller)
            {
                return result.BuildFail("Only complaints with PendingSeller status can be responded to");
            }

            // Check response deadline (48 hours)
            var deadline = complaint.CreatedAtUtc.AddHours(ResponseDeadlineHours);
            if (DateTime.UtcNow > deadline)
            {
                return result.BuildFail("Response deadline has passed. Complaint has been escalated to admin.");
            }

            // Validate refund suggestion
            if (request.IsFixedAmount)
            {
                if (!request.RefundAmount.HasValue || request.RefundAmount <= 0)
                {
                    return result.BuildFail("Refund amount must be greater than 0");
                }

                if (request.RefundAmount > (decimal)complaint.Order.Total)
                {
                    return result.BuildFail($"Refund amount cannot exceed order total of {complaint.Order.Total:N0} VNĐ");
                }
            }
            else
            {
                if (!request.RefundPercentage.HasValue || request.RefundPercentage < 0 || request.RefundPercentage > 100)
                {
                    return result.BuildFail("Refund percentage must be between 0 and 100");
                }
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update complaint with seller response
                complaint.SellerResponse = request.Response;
                complaint.SellerRespondedAt = DateTime.UtcNow;
                complaint.IsSellerRefundFixedAmount = request.IsFixedAmount;
                
                if (request.IsFixedAmount)
                {
                    complaint.SellerDesiredRefundAmount = request.RefundAmount;
                    complaint.SellerDesiredRefundPercentage = null;
                }
                else
                {
                    complaint.SellerDesiredRefundPercentage = request.RefundPercentage;
                    complaint.SellerDesiredRefundAmount = null;
                }

                complaint.Status = OrderComplaintStatusEnum.PendingAdmin;
                
                _complaintRepository.Update(complaint);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Upload evidence files
                if (request.EvidenceFiles != null && request.EvidenceFiles.Count > 0)
                {
                    var assets = new List<OrderComplaintAsset>();

                    foreach (var file in request.EvidenceFiles)
                    {
                        var fileUrl = await _fileStorageService.UploadFileAsync(
                            file,
                            $"Complaints/{complaint.Id}/Seller");

                        var asset = new OrderComplaintAsset
                        {
                            OrderComplaintId = complaint.Id,
                            AssetUrl = fileUrl,
                            AssetType = OrderComplaintAssetTypeEnum.SellerEvidence
                        };

                        assets.Add(asset);
                    }

                    await _assetRepository.AddRangeAsync(assets);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                // Reload complaint with full details
                complaint = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
                var complaintDto = ComplaintMapper.MapToDetailDto(complaint!);

                // Send notification to buyer
                await _notificationService.NotifySellerRespondedAsync(complaint!, cancellationToken);

                // Send notification to admin that a complaint is pending review
                await _notificationService.NotifyAdminPendingComplaintAsync(complaint!, "seller-responded", cancellationToken);

                return result.BuildSuccess(complaintDto, "Response submitted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result.BuildFail($"Failed to submit response: {ex.Message}");
            }
        }
    }
}
