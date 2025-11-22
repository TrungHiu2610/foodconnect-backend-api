using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.RejectComplaint;

public class RejectComplaintCommand : IRequest<BaseResponse<ComplaintDetailDto>>
{
    public Guid ComplaintId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}
