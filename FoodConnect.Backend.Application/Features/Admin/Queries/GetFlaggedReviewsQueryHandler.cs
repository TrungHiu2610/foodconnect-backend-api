using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetFlaggedReviewsQueryHandler : IRequestHandler<GetFlaggedReviewsQuery, BaseResponse<GetFlaggedReviewsResponse>>
    {
        private readonly IProductReviewRepository _reviewRepository;

        public GetFlaggedReviewsQueryHandler(IProductReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<BaseResponse<GetFlaggedReviewsResponse>> Handle(GetFlaggedReviewsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetFlaggedReviewsResponse>();

            var query = _reviewRepository.GetAllQueryable()
                .Include(r => r.Product)
                .Include(r => r.Buyer)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }
            else
            {
                query = query.Where(r => r.Status == ReviewStatusEnum.Toxic || r.Status == ReviewStatusEnum.Spam);
            }

            query = query.OrderByDescending(r => r.CreatedAtUtc);

            var totalCount = await query.CountAsync(cancellationToken);

            var reviews = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var reviewDtos = reviews.Select(r => new FlaggedReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Unknown",
                BuyerId = r.BuyerId,
                BuyerName = r.Buyer?.FullName ?? "Unknown",
                Rating = r.Rating,
                Comment = r.Comment,
                Status = r.Status,
                RejectionReason = r.RejectionReason,
                RejectionDetails = r.RejectionDetails,
                CreatedAtUtc = r.CreatedAtUtc,
                ModeratedAt = r.ModeratedAt
            }).ToList();

            var response = new GetFlaggedReviewsResponse
            {
                Reviews = reviewDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return result.BuildSuccess(response);
        }
    }
}
