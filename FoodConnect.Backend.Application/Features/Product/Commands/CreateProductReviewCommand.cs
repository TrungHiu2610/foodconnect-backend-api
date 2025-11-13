using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductReviewCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Rating { get; set; } // 1-5 stars
        public string? Comment { get; set; }
        
        /// <summary>
        /// Review assets (images/videos) - Max 5 files
        /// </summary>
        public List<IFormFile>? ReviewAssets { get; set; }
    }
}
