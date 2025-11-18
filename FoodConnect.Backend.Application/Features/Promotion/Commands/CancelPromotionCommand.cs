using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class CancelPromotionCommand : IRequest<BaseResponse<DeleteResponse>>
    {
        public Guid PromotionId { get; set; }
    }
}
