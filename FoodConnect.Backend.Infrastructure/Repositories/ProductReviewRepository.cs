using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ProductReviewRepository : BaseRepository<ProductReview>, IProductReviewRepository
    {
        public ProductReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<ProductReview> Reviews, int TotalCount)> GetShopReviewsAsync(
            Guid shopId,
            int pageNumber,
            int pageSize,
            int? minRating = null,
            bool? hasSellerResponse = null)
        {
            // Build query - CHỈ LẤY REVIEW APPROVED
            var query = _context.Set<ProductReview>()
                .Include(r => r.Product)
                .Include(r => r.Buyer)
                .Include(r => r.Assets.OrderBy(a => a.DisplayOrder))
                .Where(r => r.Product.ShopId == shopId && r.Status == Domain.Enums.ReviewStatusEnum.Approved);

            if (minRating.HasValue)
            {
                query = query.Where(r => r.Rating >= minRating.Value);
            }

            if (hasSellerResponse.HasValue)
            {
                if (hasSellerResponse.Value)
                {
                    query = query.Where(r => !string.IsNullOrEmpty(r.SellerResponse));
                }
                else
                {
                    query = query.Where(r => string.IsNullOrEmpty(r.SellerResponse));
                }
            }

            query = query.OrderByDescending(r => r.CreatedAtUtc);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<int> CountDuplicateReviewsAsync(string normalizedContent)
        {
            if (string.IsNullOrWhiteSpace(normalizedContent))
                return 0;

            // Lấy tất cả review và normalize để so sánh
            var allReviews = await _context.Set<ProductReview>()
                .Where(r => !string.IsNullOrEmpty(r.Comment))
                .Select(r => r.Comment)
                .ToListAsync();

            var count = allReviews.Count(comment => 
                NormalizeForComparison(comment!) == normalizedContent
            );

            return count;
        }

        public async Task<int> CountUserReviewsInTimeRangeAsync(Guid userId, DateTime from, DateTime to)
        {
            var count = await _context.Set<ProductReview>()
                .CountAsync(r => r.BuyerId == userId && r.CreatedAtUtc >= from && r.CreatedAtUtc <= to);

            return count;
        }

        private string NormalizeForComparison(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = RemoveDiacritics(text.ToLowerInvariant());
            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = Regex.Replace(normalized, @"[^\w\s]", "");
            return normalized.Trim();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }
    }
}
