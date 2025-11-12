using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IProductReviewRepository : IBaseRepository<ProductReview>
    {
        /// <summary>
        /// Get paginated reviews for a shop with filters
        /// </summary>
        Task<(List<ProductReview> Reviews, int TotalCount)> GetShopReviewsAsync(
            Guid shopId,
            int pageNumber,
            int pageSize,
            int? minRating = null,
            bool? hasSellerResponse = null);
    }
}
