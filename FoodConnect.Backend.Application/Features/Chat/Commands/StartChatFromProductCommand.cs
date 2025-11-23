using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Commands;

public class StartChatFromProductCommand : IRequest<BaseResponse<StartChatResponse>>
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
}
