using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.ApproveComplaint;

public class ApproveComplaintCommand : IRequest<BaseResponse<ComplaintDetailDto>>
{
    public Guid ComplaintId { get; set; }
    public decimal RefundAmount { get; set; }
    public string? AdminReason { get; set; }
}
