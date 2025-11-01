using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetAllCategoriesForAdminQuery : IRequest<BaseResponse<GetListCategoryResponse>>
    {
        // Search
        public string? SearchTerm { get; set; }
        
        // Filters
        public bool? IsActive { get; set; }  // null = all, true = active only, false = inactive only
        public Guid? ParentId { get; set; }  // Filter by parent category
        public string? DeliveryType { get; set; }  // "Standard", "Express"
        
        // Date range filter
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        
        // Sorting
        public string? SortBy { get; set; }  // "Name", "CreatedAt", "ProductCount"
        public bool IsDescending { get; set; } = true;  // Default: newest first
    }
}
