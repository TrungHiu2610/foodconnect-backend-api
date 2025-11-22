using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries;

public class GetAllComplaintsQuery : IRequest<BaseResponse<List<ComplaintSummaryDto>>>
{
    public OrderComplaintStatusEnum? Status { get; set; }
}
