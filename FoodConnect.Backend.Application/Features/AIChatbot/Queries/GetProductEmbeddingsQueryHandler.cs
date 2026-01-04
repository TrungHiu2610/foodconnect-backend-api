using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Queries;

public class GetProductEmbeddingsQueryHandler : IRequestHandler<GetProductEmbeddingsQuery, BaseResponse<PaginatedList<ProductEmbeddingResponse>>>
{
    private readonly IProductEmbeddingRepository _embeddingRepository;
    private readonly IProductRepository _productRepository;

    public GetProductEmbeddingsQueryHandler(
        IProductEmbeddingRepository embeddingRepository,
        IProductRepository productRepository)
    {
        _embeddingRepository = embeddingRepository;
        _productRepository = productRepository;
    }

    public async Task<BaseResponse<PaginatedList<ProductEmbeddingResponse>>> Handle(GetProductEmbeddingsQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaginatedList<ProductEmbeddingResponse>>();

        try
        {
            // Get all products
            var productsQuery = _productRepository.GetProductsAsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                productsQuery = productsQuery.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            // Get embeddings
            var embeddingsQuery = await _embeddingRepository.GetAllAsync();

            // Join products with embeddings
            var query = from product in productsQuery
                        join embedding in embeddingsQuery on product.Id equals embedding.ProductId into embeddingGroup
                        from embedding in embeddingGroup.DefaultIfEmpty()
                        select new ProductEmbeddingResponse
                        {
                            Id = embedding != null ? embedding.Id : Guid.Empty,
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ProductDescription = product.Description ?? string.Empty,
                            EmbeddingContent = embedding != null ? embedding.EmbeddingContent : string.Empty,
                            HasEmbedding = embedding != null,
                            CreatedAt = embedding != null ? embedding.CreatedAtUtc : null,
                            UpdatedAt = embedding != null ? embedding.UpdatedAtUtc : null,
                            EmbeddingDimension = embedding != null ? embedding.Embedding.ToArray().Length : 0
                        };

            // Filter by HasEmbedding if specified
            if (request.HasEmbedding.HasValue)
            {
                query = query.Where(e => e.HasEmbedding == request.HasEmbedding.Value);
            }

            // Order by updated date (newest first)
            query = query.OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt);

            // Pagination
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var paginatedList = new PaginatedList<ProductEmbeddingResponse>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return result.BuildSuccess(paginatedList);
        }
        catch (Exception ex)
        {
            return result.BuildFail($"Error retrieving embeddings: {ex.Message}");
        }
    }
}
