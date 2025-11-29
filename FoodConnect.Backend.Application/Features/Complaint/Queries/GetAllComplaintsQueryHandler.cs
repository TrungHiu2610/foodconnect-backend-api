using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries;

public class GetAllComplaintsQueryHandler : IRequestHandler<GetAllComplaintsQuery, BaseResponse<List<ComplaintSummaryDto>>>
{
    private readonly IOrderComplaintRepository _complaintRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllComplaintsQueryHandler(
        IOrderComplaintRepository complaintRepository,
        ICurrentUserService currentUserService)
    {
        _complaintRepository = complaintRepository;
        _currentUserService = currentUserService;
    }

    public async Task<BaseResponse<List<ComplaintSummaryDto>>> Handle(
        GetAllComplaintsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new BaseResponse<List<ComplaintSummaryDto>>();

        // Check authentication
        if (!_currentUserService.UserId.HasValue)
        {
            return result.BuildUnauthorized("User must be logged in");
        }

        // Check admin role
        if (_currentUserService.Role != "Admin")
        {
            return result.BuildForbidden("Only admins can view all complaints");
        }

        // Get complaints for admin review
        var complaints = await _complaintRepository.GetPendingAdminComplaintsAsync(request.Status);

        // Map to DTOs
        var complaintDtos = complaints
            .Select(ComplaintMapper.MapToSummaryDto)
            .ToList();

        return result.BuildSuccess(complaintDtos, "Complaints retrieved successfully");
    }
}
