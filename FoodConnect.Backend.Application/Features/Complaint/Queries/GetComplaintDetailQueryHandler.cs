using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries
{
    public class GetComplaintDetailQueryHandler : IRequestHandler<GetComplaintDetailQuery, BaseResponse<ComplaintDetailDto>>
    {
        private readonly IOrderComplaintRepository _complaintRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetComplaintDetailQueryHandler(
            IOrderComplaintRepository complaintRepository,
            ICurrentUserService currentUserService)
        {
            _complaintRepository = complaintRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<ComplaintDetailDto>> Handle(
            GetComplaintDetailQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ComplaintDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var userId = _currentUserService.UserId.Value;

            var complaint = await _complaintRepository.GetComplaintWithDetailsAsync(request.ComplaintId);
            if (complaint == null)
            {
                return result.BuildNotFound("Complaint not found");
            }

            // Check if user has permission to view this complaint
            if (complaint.BuyerId != userId && complaint.SellerId != userId)
            {
                if(_currentUserService.Role != "Admin")
                {
                    return result.BuildForbidden("You don't have permission to view this complaint");
                }
            }

            var complaintDto = ComplaintMapper.MapToDetailDto(complaint);
            return result.BuildSuccess(complaintDto, "Complaint retrieved successfully");
        }
    }
}
