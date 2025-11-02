using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.User;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.User.Queries
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, BaseResponse<UserProfileResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUserProfileQueryHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<UserProfileResponse>> Handle(
            GetUserProfileQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<UserProfileResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized();
            }

            var user = await _userRepository.GetUserWithRolesAsync(userId.Value);
            if (user == null)
            {
                return result.BuildNotFound();
            }

            var response = new UserProfileResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Status = user.Status.ToString(),
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return result.BuildSuccess(response, "User profile retrieved successfully");
        }
    }
}
