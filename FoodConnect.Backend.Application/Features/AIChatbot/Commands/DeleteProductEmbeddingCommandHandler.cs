using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class DeleteProductEmbeddingCommandHandler : IRequestHandler<DeleteProductEmbeddingCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IProductEmbeddingRepository _embeddingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductEmbeddingCommandHandler> _logger;

    public DeleteProductEmbeddingCommandHandler(
        IProductEmbeddingRepository embeddingRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductEmbeddingCommandHandler> logger)
    {
        _embeddingRepository = embeddingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(DeleteProductEmbeddingCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        try
        {
            var embedding = await _embeddingRepository.GetByProductIdAsync(request.ProductId);
            if (embedding == null)
            {
                return result.BuildNotFound("Embedding not found for this product");
            }

            _embeddingRepository.Remove(embedding);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted embedding for product {ProductId}", request.ProductId);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = embedding.Id
            }, "Embedding deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting embedding for product {ProductId}", request.ProductId);
            return result.BuildFail($"Error deleting embedding: {ex.Message}");
        }
    }
}
