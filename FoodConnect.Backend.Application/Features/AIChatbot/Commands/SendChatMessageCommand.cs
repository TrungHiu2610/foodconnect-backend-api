using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.AIChatbot;
using MediatR;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class SendChatMessageCommand : IRequest<BaseResponse<ChatResponse>>
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}
