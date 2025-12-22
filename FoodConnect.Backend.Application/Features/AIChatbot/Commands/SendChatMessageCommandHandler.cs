using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.AIChatbot;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, BaseResponse<ChatResponse>>
{
    private readonly IGeminiAIService _geminiService;
    private readonly IProductRetrievalService _retrievalService;
    private readonly IAIChatConversationRepository _conversationRepository;
    private readonly IAIChatMessageRepository _messageRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<SendChatMessageCommandHandler> _logger;

    public SendChatMessageCommandHandler(
        IGeminiAIService geminiService,
        IProductRetrievalService retrievalService,
        IAIChatConversationRepository conversationRepository,
        IAIChatMessageRepository messageRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<SendChatMessageCommandHandler> logger)
    {
        _geminiService = geminiService;
        _retrievalService = retrievalService;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<ChatResponse>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<ChatResponse>();

        try
        {
            // Get or create conversation
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            
            var conversation = await _conversationRepository.GetByUserAndSessionAsync(userId, sessionId);
            if (conversation == null)
            {
                conversation = new AIChatConversation
                {
                    UserId = userId,
                    SessionId = sessionId,
                    IsActive = true,
                    LastMessageAt = DateTime.UtcNow
                };
                await _conversationRepository.AddAsync(conversation);
                await _unitOfWork.SaveChangesAsync();
            }

            // Store user message
            var userMessage = new AIChatMessage
            {
                ConversationId = conversation.Id,
                Role = AIChatMessageRoleEnum.User,
                Content = request.Message
            };
            await _messageRepository.AddAsync(userMessage);
            await _unitOfWork.SaveChangesAsync();

            // Get conversation history
            var conversationHistory = await _messageRepository.GetConversationHistoryAsync(conversation.Id, 10);
            var historyTexts = conversationHistory
                .OrderBy(m => m.CreatedAtUtc)
                .Select(m => $"{m.Role}: {m.Content}")
                .ToList();

            // Step 1: Extract Intent
            var intent = await _geminiService.ExtractIntentAsync(request.Message, historyTexts);
            _logger.LogInformation("Extracted intent: {Intent}", JsonSerializer.Serialize(intent));

            // Step 2: Hybrid Retrieval
            var retrievedProducts = await _retrievalService.HybridSearchAsync(request.Message, intent, maxResults: 50);
            _logger.LogInformation("Retrieved {Count} products", retrievedProducts.Count);

            if (!retrievedProducts.Any())
            {
                // No products found - return helpful message
                var noResultsResponse = new ChatResponse
                {
                    Message = "Xin lỗi, mình không tìm thấy sản phẩm phù hợp với yêu cầu của bạn. Bạn có thể thử:\n" +
                              "- Mô tả chi tiết hơn về sản phẩm bạn cần\n" +
                              "- Hỏi về các danh mục sản phẩm có sẵn\n" +
                              "- Thay đổi tiêu chí tìm kiếm",
                    SessionId = sessionId,
                    SuggestedQuestions = new List<string>
                    {
                        "Có những loại sản phẩm nào?",
                        "Gợi ý món ăn healthy cho tôi",
                        "Tìm đồ ăn vặt dưới 100k"
                    }
                };

                await SaveAssistantMessageAsync(conversation.Id, noResultsResponse.Message, new List<Guid>(), JsonSerializer.Serialize(intent));
                conversation.LastMessageAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return result.BuildSuccess(noResultsResponse, "Success");
            }

            // Step 3: Map to RankedProduct for reranking
            var rankedProducts = retrievedProducts.Select(p => new RankedProduct
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Ingredients = p.Ingredients,
                CategoryName = p.Category?.Name ?? "",
                ShopId = p.ShopId,
                ShopName = p.Shop?.ShopName ?? "",
                ShopRating = p.Shop?.Rating,
                ThumbnailUrl = p.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                Score = 5.0 // Initial score
            }).ToList();

            // Step 4: Rerank with AI
            var topProducts = await _geminiService.RerankProductsAsync(request.Message, intent, rankedProducts, topK: 5);
            _logger.LogInformation("Reranked to {Count} top products", topProducts.Count);

            // Step 5: Generate Response
            var responseMessage = await _geminiService.GenerateResponseAsync(request.Message, intent, topProducts, historyTexts);

            // Step 6: Build response
            var chatResponse = new ChatResponse
            {
                Message = responseMessage,
                RecommendedProducts = topProducts.Select(p => 
                {
                    var product = retrievedProducts.FirstOrDefault(rp => rp.Id == p.Id);
                    return new ProductRecommendation
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        ThumbnailUrl = p.ThumbnailUrl,
                        ProductUrl = $"/products/{p.Id}", // Frontend route to product detail
                        CategoryName = p.CategoryName,
                        ShopId = product?.ShopId ?? Guid.Empty,
                        ShopName = p.ShopName,
                        ShopUrl = product?.ShopId != null ? $"/shops/{product.ShopId}" : string.Empty, // Frontend route to shop page
                        ShopRating = p.ShopRating,
                        Reason = p.Reason,
                        RelevanceScore = p.Score,
                        IsAvailable = product?.IsAvailable ?? true,
                        StockQuantity = product?.StockQuantity
                    };
                }).ToList(),
                SuggestedQuestions = GenerateSuggestedQuestions(intent),
                SessionId = sessionId
            };

            // Store assistant message
            await SaveAssistantMessageAsync(
                conversation.Id, 
                responseMessage, 
                topProducts.Select(p => p.Id).ToList(),
                JsonSerializer.Serialize(intent));

            conversation.LastMessageAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return result.BuildSuccess(chatResponse, "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return result.BuildFail($"Error: {ex.Message}");
        }
    }

    private async Task SaveAssistantMessageAsync(Guid conversationId, string content, List<Guid> productIds, string intentJson)
    {
        var assistantMessage = new AIChatMessage
        {
            ConversationId = conversationId,
            Role = AIChatMessageRoleEnum.Assistant,
            Content = content,
            RecommendedProductIds = JsonSerializer.Serialize(productIds),
            IntentJson = intentJson
        };
        await _messageRepository.AddAsync(assistantMessage);
    }

    private List<string> GenerateSuggestedQuestions(UserIntent intent)
    {
        var suggestions = new List<string>
        {
            "Món nào ít calo nhất?",
            "Có thể kết hợp với món gì?",
            "Cách bảo quản như thế nào?",
            "Có món nào khác cùng loại không?"
        };

        if (intent.Categories.Any())
        {
            suggestions.Add($"Gợi ý thêm món {intent.Categories.First()} khác");
        }

        return suggestions.Take(3).ToList();
    }
}
