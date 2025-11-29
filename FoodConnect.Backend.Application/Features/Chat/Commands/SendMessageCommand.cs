using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Chat.Commands;

public class SendMessageCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid ConversationId { get; set; }
    public int MessageType { get; set; }
    public string? Content { get; set; }
    public IFormFile? MediaFile { get; set; }
}
