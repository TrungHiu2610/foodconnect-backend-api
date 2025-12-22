using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class SellerRespondToReviewCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid ReviewId { get; set; }
        public string SellerResponse { get; set; } = string.Empty;
    }
}
