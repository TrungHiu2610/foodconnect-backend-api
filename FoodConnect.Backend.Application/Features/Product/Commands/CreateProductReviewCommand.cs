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
        public IFormFile? ReviewImage { get; set; }
    }
}
