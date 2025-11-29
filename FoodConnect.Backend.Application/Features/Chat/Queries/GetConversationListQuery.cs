using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Chat.Queries;

public class GetConversationListQuery : IRequest<BaseResponse<List<ConversationResponse>>>
{
}
