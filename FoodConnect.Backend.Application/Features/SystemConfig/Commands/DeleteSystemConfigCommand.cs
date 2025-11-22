using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class DeleteSystemConfigCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid Id { get; set; }
}
