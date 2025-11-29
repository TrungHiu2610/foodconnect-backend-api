using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Queries;

public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, BaseResponse<PaginatedList<MessageResponse>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetChatHistoryQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<MessageResponse>>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaginatedList<MessageResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null)
            return result.BuildNotFound("Conversation not found");

        if (conversation.BuyerId != userId.Value && conversation.SellerId != userId.Value)
            return result.BuildForbidden();

        var messages = await _messageRepository.GetByConversationIdAsync(
            request.ConversationId, 
            request.PageNumber, 
            request.PageSize);

        var totalCount = await _messageRepository.CountByConversationIdAsync(request.ConversationId);

        var messageResponses = _mapper.Map<List<MessageResponse>>(messages);

        var paginatedList = new PaginatedList<MessageResponse>(
            messageResponses,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return result.BuildSuccess(paginatedList, "Chat history retrieved successfully");
    }
}
