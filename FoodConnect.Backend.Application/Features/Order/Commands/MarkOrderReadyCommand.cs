using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class MarkOrderReadyCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid OrderId { get; set; }
    }
}
