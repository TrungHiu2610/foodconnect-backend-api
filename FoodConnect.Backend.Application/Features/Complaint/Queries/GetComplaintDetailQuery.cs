using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Queries
{
    public class GetComplaintDetailQuery : IRequest<BaseResponse<ComplaintDetailDto>>
    {
        public Guid ComplaintId { get; set; }
    }
}
