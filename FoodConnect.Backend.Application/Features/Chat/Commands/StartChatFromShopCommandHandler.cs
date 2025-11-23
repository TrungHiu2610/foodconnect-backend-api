using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Commands;

public class StartChatFromShopCommandHandler : IRequestHandler<StartChatFromShopCommand, BaseResponse<StartChatResponse>>
{
    private readonly IShopRepository _shopRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StartChatFromShopCommandHandler(
        IShopRepository shopRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _shopRepository = shopRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<StartChatResponse>> Handle(StartChatFromShopCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<StartChatResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        // Verify shop exists
        var shop = await _shopRepository.GetByIdAsync(request.ShopId);
        if (shop == null)
            return result.BuildNotFound("Shop not found");

        var buyerId = userId.Value;
        var sellerId = shop.UserId;

        // Prevent seller from chatting with themselves
        if (buyerId == sellerId)
            return result.BuildFail("Cannot start chat with yourself");

        // Check if conversation already exists
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

        var response = new StartChatResponse
        {
            ConversationId = conversation.Id,
            SystemMessage = null,
            IsNewConversation = isNewConversation
        };

        return result.BuildSuccess(response, "Conversation started successfully");
    }
}
