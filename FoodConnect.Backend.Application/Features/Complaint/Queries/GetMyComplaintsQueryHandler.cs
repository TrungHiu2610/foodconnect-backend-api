using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries
{
    public class GetMyComplaintsQueryHandler : IRequestHandler<GetMyComplaintsQuery, BaseResponse<List<ComplaintSummaryDto>>>
    {
        private readonly IOrderComplaintRepository _complaintRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyComplaintsQueryHandler(
            IOrderComplaintRepository complaintRepository,
            ICurrentUserService currentUserService)
        {
            _complaintRepository = complaintRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<List<ComplaintSummaryDto>>> Handle(
            GetMyComplaintsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<ComplaintSummaryDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;

            var complaints = await _complaintRepository.GetComplaintsByBuyerIdAsync(buyerId, request.Status);
            var complaintDtos = complaints.Select(ComplaintMapper.MapToSummaryDto).ToList();

            return result.BuildSuccess(complaintDtos, "Complaints retrieved successfully");
        }
    }
}
