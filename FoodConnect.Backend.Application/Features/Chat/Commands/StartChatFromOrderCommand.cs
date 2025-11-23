using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Commands;

public class StartChatFromOrderCommand : IRequest<BaseResponse<StartChatResponse>>
{
    public Guid OrderId { get; set; }
}
