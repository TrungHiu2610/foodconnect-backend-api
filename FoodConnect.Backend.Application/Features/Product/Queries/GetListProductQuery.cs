using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQuery : IRequest<BaseResponse<PaginatedList<GetListProductItemResponse>>>
    {
        public Guid? UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ShopId { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Status { get; set; }
        
        public double? BuyerLatitude { get; set; }
        public double? BuyerLongitude { get; set; }
        public DeliveryTypeEnum? DeliveryType { get; set; } 

        public string? TextSearch { get; set; }

        public List<SortInfo>? SortInfos { get; set; }
        public bool SortOutOfStockLast { get; set; } = false; 

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
