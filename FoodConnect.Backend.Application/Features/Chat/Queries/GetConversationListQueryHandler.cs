using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Queries;

public class GetConversationListQueryHandler : IRequestHandler<GetConversationListQuery, BaseResponse<List<ConversationResponse>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetConversationListQueryHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<ConversationResponse>>> Handle(GetConversationListQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<List<ConversationResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var conversations = await _conversationRepository.GetConversationsByUserIdAsync(userId.Value);
        
        // Map to response with additional computed fields
        var response = conversations.Select(c => new ConversationResponse
        {
            Id = c.Id,
            BuyerId = c.BuyerId,
            BuyerName = c.Buyer?.FullName ?? string.Empty,
            BuyerAvatar = c.Buyer?.AvatarUrl,
            SellerId = c.SellerId,
            SellerName = c.Seller?.FullName ?? string.Empty,
            SellerAvatar = c.Seller?.AvatarUrl,
            LastMessageAt = c.LastMessageAt,
            CreatedAt = c.CreatedAtUtc,
            
            // Get last message content
            LastMessage = c.Messages?.OrderByDescending(m => m.CreatedAtUtc).FirstOrDefault()?.Content,
            
            // Count unread messages (messages not sent by current user and not read)
            UnreadCount = c.Messages?.Count(m => m.SenderId != userId.Value && !m.IsRead) ?? 0,
            
            // Determine other user info based on current user
            OtherUserId = c.BuyerId == userId.Value ? c.SellerId : c.BuyerId,
            OtherUserName = c.BuyerId == userId.Value ? (c.Seller?.FullName ?? string.Empty) : (c.Buyer?.FullName ?? string.Empty),
            OtherUserAvatar = c.BuyerId == userId.Value ? c.Seller?.AvatarUrl : c.Buyer?.AvatarUrl
        }).ToList();

        return result.BuildSuccess(response, "Conversations retrieved successfully");
    }
}
