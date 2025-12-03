using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Commands;

public class StartChatFromProductCommandHandler : IRequestHandler<StartChatFromProductCommand, BaseResponse<StartChatResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IChatNotificationService _chatNotificationService;

    public StartChatFromProductCommandHandler(
        IProductRepository productRepository,
        IShopRepository shopRepository,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IChatNotificationService chatNotificationService)
    {
        _productRepository = productRepository;
        _shopRepository = shopRepository;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<BaseResponse<StartChatResponse>> Handle(StartChatFromProductCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<StartChatResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            return result.BuildNotFound("Product not found");

        var shop = await _shopRepository.GetByIdAsync(request.ShopId);
        if (shop == null)
            return result.BuildNotFound("Shop not found");

        var buyerId = userId.Value;
        var sellerId = shop.UserId;

        if (buyerId == sellerId)
            return result.BuildFail("Cannot start chat with yourself");

        var conversation = await _conversationRepository.GetByBuyerAndSellerAsync(buyerId, sellerId);
        var isNewConversation = false;

        if (conversation == null)
        {
            conversation = new Conversation
            {
                BuyerId = buyerId,
                SellerId = sellerId,
                LastMessageAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            isNewConversation = true;
        }

        var systemMessage = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId.Value,
            MessageType = MessageTypeEnum.System,
            Content = $"Người dùng đang hỏi về sản phẩm \"{product.Name}\""
        };

        await _messageRepository.AddAsync(systemMessage);
        conversation.LastMessageAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _chatNotificationService.NotifyNewMessageAsync(
                conversation.Id,
                systemMessage.Id,
                sellerId,
                $"Khách hàng hỏi về sản phẩm \"{product.Name}\""
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartChatFromProduct] Notification error: {ex.Message}");
        }

        var response = new StartChatResponse
        {
            ConversationId = conversation.Id,
            SystemMessage = _mapper.Map<MessageResponse>(systemMessage),
            IsNewConversation = isNewConversation
        };

        return result.BuildSuccess(response, "Conversation started successfully");
    }
}
