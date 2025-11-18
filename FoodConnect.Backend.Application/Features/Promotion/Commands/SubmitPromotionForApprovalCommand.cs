using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class SubmitPromotionForApprovalCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid PromotionId { get; set; }
    }
}
