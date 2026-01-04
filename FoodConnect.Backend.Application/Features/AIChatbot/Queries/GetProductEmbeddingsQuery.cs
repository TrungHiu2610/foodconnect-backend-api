using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Models;
using MediatR;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Queries;

public class GetProductEmbeddingsQuery : IRequest<BaseResponse<PaginatedList<ProductEmbeddingResponse>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? HasEmbedding { get; set; }
}

public class ProductEmbeddingResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string EmbeddingContent { get; set; } = string.Empty;
    public bool HasEmbedding { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int EmbeddingDimension { get; set; }
}
