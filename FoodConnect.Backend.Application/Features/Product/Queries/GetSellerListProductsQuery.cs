using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetSellerListProductsQuery : IRequest<BaseResponse<PaginatedList<GetListProductItemResponse>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProductStatusEnum? Status { get; set; }
        public string? SearchTerm { get; set; }
    }
}
