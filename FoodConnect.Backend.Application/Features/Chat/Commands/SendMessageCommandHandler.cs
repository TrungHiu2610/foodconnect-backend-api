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

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        IChatNotificationService chatNotificationService,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _chatNotificationService = chatNotificationService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null)
            return result.BuildNotFound("Conversation not found");

        if (conversation.BuyerId != userId.Value && conversation.SellerId != userId.Value)
            return result.BuildForbidden();

        var messageType = (MessageTypeEnum)request.MessageType;
        
        if (messageType == MessageTypeEnum.Text && string.IsNullOrWhiteSpace(request.Content))
            return result.BuildFail("Content is required for text message");

        if ((messageType == MessageTypeEnum.Image || messageType == MessageTypeEnum.Video) && request.MediaFile == null)
            return result.BuildFail("Media file is required");

        string? mediaUrl = null;
        if (request.MediaFile != null)
        {
            var maxSize = messageType == MessageTypeEnum.Image ? 10 * 1024 * 1024 : 100 * 1024 * 1024;
            if (request.MediaFile.Length > maxSize)
                return result.BuildFail($"File size exceeds {maxSize / (1024 * 1024)}MB limit");

            mediaUrl = await _fileStorageService.UploadFileAsync(
                request.MediaFile, 
                $"Chat/{conversation.Id}");
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = userId.Value,
            MessageType = messageType,
            Content = request.Content,
            MediaUrl = mediaUrl
        };

        await _messageRepository.AddAsync(message);
        conversation.LastMessageAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Broadcast message to conversation room via SignalR
        var messageResponse = _mapper.Map<MessageResponse>(message);
        await _chatNotificationService.SendMessageToConversationAsync(
            conversation.Id.ToString(), 
            messageResponse);

        return result.BuildSuccess(new CreateOrUpdateResponse { Id = message.Id }, "Message sent successfully");
    }
}
