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

public class StartChatFromOrderCommandHandler : IRequestHandler<StartChatFromOrderCommand, BaseResponse<StartChatResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StartChatFromOrderCommandHandler(
        IOrderRepository orderRepository,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<StartChatResponse>> Handle(StartChatFromOrderCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<StartChatResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var order = await _orderRepository.GetByIdAsync(request.OrderId, or=>or.Shop);
        if (order == null)
            return result.BuildNotFound("Order not found");

        if (order.BuyerId != userId.Value)
            return result.BuildForbidden();

        var buyerId = order.BuyerId;
        var sellerId = order.Shop.UserId;

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
            await _unitOfWork.SaveChangesAsync();
            isNewConversation = true;
        }

        var systemMessage = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId.Value,
            MessageType = MessageTypeEnum.System,
            Content = $"Người dùng đang hỏi về đơn hàng #{order.OrderCode}"
        };

        await _messageRepository.AddAsync(systemMessage);
        conversation.LastMessageAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var response = new StartChatResponse
        {
            ConversationId = conversation.Id,
            SystemMessage = _mapper.Map<MessageResponse>(systemMessage),
            IsNewConversation = isNewConversation
        };

        return result.BuildSuccess(response, "Conversation started successfully");
    }
}
