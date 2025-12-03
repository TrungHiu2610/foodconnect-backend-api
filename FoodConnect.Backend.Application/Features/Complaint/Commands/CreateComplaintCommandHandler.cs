using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
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
    public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, BaseResponse<ComplaintDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderComplaintRepository _complaintRepository;
        private readonly IOrderComplaintAssetRepository _assetRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;
        private readonly WalletService _walletService;
        private readonly ComplaintNotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        private const int MaxEvidenceFiles = 5;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".mp4", ".mov" };
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

        public CreateComplaintCommandHandler(
            IOrderRepository orderRepository,
            IOrderComplaintRepository complaintRepository,
            IOrderComplaintAssetRepository assetRepository,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService,
            WalletService walletService,
            ComplaintNotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _complaintRepository = complaintRepository;
            _assetRepository = assetRepository;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
            _walletService = walletService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ComplaintDetailDto>> Handle(
            CreateComplaintCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ComplaintDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;

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

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            if (order.BuyerId != buyerId)
            {
                return result.BuildForbidden("You don't have permission to complain about this order");
            }

            if (order.Status != OrderStatusEnum.Delivered)
            {
                return result.BuildFail("Only delivered orders can be complained");
            }

            if (order.CompletedAt.HasValue)
            {
                return result.BuildFail("Cannot complain about completed orders");
            }

            var existingComplaint = await _complaintRepository.GetByOrderIdAsync(request.OrderId);
            if (existingComplaint != null)
            {
                return result.BuildFail("A complaint already exists for this order");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _walletService.GetOrCreateBuyerWalletAsync(buyerId, cancellationToken);

                var complaint = new OrderComplaint
                {
                    OrderId = request.OrderId,
                    BuyerId = buyerId,
                    SellerId = order.Shop.UserId,
                    BuyerReason = request.Reason,
                    Status = OrderComplaintStatusEnum.PendingSeller
                };

                await _complaintRepository.AddAsync(complaint);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (request.EvidenceFiles != null && request.EvidenceFiles.Count > 0)
                {
                    var assets = new List<OrderComplaintAsset>();

                    foreach (var file in request.EvidenceFiles)
                    {
                        var fileUrl = await _fileStorageService.UploadFileAsync(
                            file,
                            $"Complaints/{complaint.Id}/Buyer");

                        var asset = new OrderComplaintAsset
                        {
                            OrderComplaintId = complaint.Id,
                            AssetUrl = fileUrl,
                            AssetType = OrderComplaintAssetTypeEnum.BuyerEvidence
                        };

                        assets.Add(asset);
                    }

                    await _assetRepository.AddRangeAsync(assets);
                }

                order.Status = OrderStatusEnum.Disputing;
                _orderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                complaint = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
                var complaintDto = ComplaintMapper.MapToDetailDto(complaint!);

                await _notificationService.NotifyComplaintCreatedAsync(complaint!, cancellationToken);

                return result.BuildSuccess(complaintDto, "Complaint created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result.BuildFail($"Failed to create complaint: {ex.Message}");
            }
        }
    }
}
