using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Models;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQuery : IRequest<BaseResponse<PaginatedList<GetListProductItemResponse>>>
    {
        // Filtering
        public Guid? CategoryId { get; set; }
        public Guid? ShopId { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Status { get; set; } 

        // Searching
        public string? TextSearch { get; set; }

        // Sorting
        public List<SortInfo>? SortInfos { get; set; }
        public bool SortOutOfStockLast { get; set; } = false; 

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
