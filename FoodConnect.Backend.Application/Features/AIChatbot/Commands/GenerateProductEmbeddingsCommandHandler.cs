using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class GenerateProductEmbeddingsCommandHandler : IRequestHandler<GenerateProductEmbeddingsCommand, BaseResponse<GenerateEmbeddingsResponse>>
{
    private readonly IProductEmbeddingService _embeddingService;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GenerateProductEmbeddingsCommandHandler> _logger;

    public GenerateProductEmbeddingsCommandHandler(
        IProductEmbeddingService embeddingService,
        IProductRepository productRepository,
        ILogger<GenerateProductEmbeddingsCommandHandler> logger)
    {
        _embeddingService = embeddingService;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<GenerateEmbeddingsResponse>> Handle(GenerateProductEmbeddingsCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<GenerateEmbeddingsResponse>();
        var response = new GenerateEmbeddingsResponse();

        try
        {
            // Get all active products
            var products = await _productRepository.GetAllAsync();
            var activeProducts = products
                .Where(p => p.Status == ProductStatusEnum.Active && p.IsAvailable)
                .ToList();

            response.TotalProcessed = activeProducts.Count;
            _logger.LogInformation("Starting embedding generation for {Count} products", activeProducts.Count);

            foreach (var product in activeProducts)
            {
                try
                {
                    if (request.ForceRegenerate)
                    {
                        await _embeddingService.UpdateProductEmbeddingAsync(product.Id);
                    }
                    else
                    {
                        await _embeddingService.GenerateProductEmbeddingAsync(product.Id);
                    }
                    response.SuccessCount++;
                    
                    // Rate limiting: Gemini free tier = 15 req/min for embeddings
                    await Task.Delay(4000, cancellationToken); // 4s delay = ~15 requests/min
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Product {product.Id}: {ex.Message}");
                    _logger.LogError(ex, "Failed to generate embedding for product {ProductId}", product.Id);
                }
            }

            _logger.LogInformation("Embedding generation completed. Success: {Success}, Failures: {Failures}", 
                response.SuccessCount, response.FailureCount);

            return result.BuildSuccess(response, $"Processed {response.SuccessCount}/{response.TotalProcessed} products");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch embedding generation");
            return result.BuildFail($"Error: {ex.Message}");
        }
    }
}
