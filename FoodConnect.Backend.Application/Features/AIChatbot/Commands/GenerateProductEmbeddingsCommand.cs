using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class GenerateProductEmbeddingsCommand : IRequest<BaseResponse<GenerateEmbeddingsResponse>>
{
    public bool ForceRegenerate { get; set; } = false;
}

public class GenerateEmbeddingsResponse
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
