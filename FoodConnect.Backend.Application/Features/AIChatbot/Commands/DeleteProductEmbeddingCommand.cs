using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class DeleteProductEmbeddingCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid ProductId { get; set; }
}
