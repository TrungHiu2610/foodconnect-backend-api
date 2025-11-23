using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries
{
    public class GetShopComplaintsQuery : IRequest<BaseResponse<List<ComplaintSummaryDto>>>
    {
        public Guid ShopId { get; set; }
        public OrderComplaintStatusEnum? Status { get; set; }
    }
}
