using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class SellerRespondToReviewCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid ReviewId { get; set; }
        
        /// <summary>
        /// Seller's response to the buyer's review
        /// </summary>
        public string SellerResponse { get; set; } = string.Empty;
    }
}
