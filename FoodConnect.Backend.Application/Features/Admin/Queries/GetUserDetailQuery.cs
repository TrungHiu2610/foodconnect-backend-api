using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetUserDetailQuery : IRequest<BaseResponse<UserDetailResponse>>
    {
        public Guid UserId { get; set; }
    }
}
