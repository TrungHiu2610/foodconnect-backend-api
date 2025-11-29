using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Commands
{
    public class ChangeUserStatusCommand : IRequest<BaseResponse<UserStatusChangeResponse>>
    {
        public Guid UserId { get; set; }
        public UserStatusEnum NewStatus { get; set; }
        public string? Reason { get; set; }
    }
}
