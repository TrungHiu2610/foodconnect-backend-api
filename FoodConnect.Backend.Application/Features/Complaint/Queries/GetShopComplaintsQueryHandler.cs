using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries
{
    public class GetShopComplaintsQueryHandler : IRequestHandler<GetShopComplaintsQuery, BaseResponse<List<ComplaintSummaryDto>>>
    {
        private readonly IOrderComplaintRepository _complaintRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetShopComplaintsQueryHandler(
            IOrderComplaintRepository complaintRepository,
            IShopRepository shopRepository,
            ICurrentUserService currentUserService)
        {
            _complaintRepository = complaintRepository;
            _shopRepository = shopRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<List<ComplaintSummaryDto>>> Handle(
            GetShopComplaintsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<ComplaintSummaryDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var userId = _currentUserService.UserId.Value;

            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            if (shop.UserId != userId)
            {
                return result.BuildForbidden("You don't have permission to view complaints for this shop");
            }

            var complaints = await _complaintRepository.GetComplaintsBySellerIdAsync(userId, request.Status);
            var complaintDtos = complaints.Select(ComplaintMapper.MapToSummaryDto).ToList();

            return result.BuildSuccess(complaintDtos, "Complaints retrieved successfully");
        }
    }
}
