using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.User;
using MediatR;

namespace FoodConnect.Backend.Application.Features.User.Queries
{
    public class GetUserProfileQuery : IRequest<BaseResponse<UserProfileResponse>>
    {
    }
}
