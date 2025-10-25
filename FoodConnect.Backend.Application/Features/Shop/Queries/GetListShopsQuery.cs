using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetListShopsQuery : IRequest<BaseResponse<PaginatedList<ShopListResponse>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ShopStatusEnum? Status { get; set; }
        public string? SearchTerm { get; set; }
    }
}
