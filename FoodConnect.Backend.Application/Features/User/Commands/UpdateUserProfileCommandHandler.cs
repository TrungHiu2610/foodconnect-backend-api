using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.User.Commands
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;

        public UpdateUserProfileCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(
            UpdateUserProfileCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized();
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return result.BuildNotFound();
            }

            string? oldAvatarUrl = user.AvatarUrl;
            string? newAvatarUrl = null;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Avatar != null)
                {
                    newAvatarUrl = await _fileStorageService.UploadFileAsync(
                        request.Avatar,
                        $"Avatars/{userId}");
                }

                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                user.DateOfBirth = request.DateOfBirth;
                user.Gender = request.Gender;

                if (newAvatarUrl != null)
                {
                    user.AvatarUrl = newAvatarUrl;
                }

                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(transaction);

                if (newAvatarUrl != null && !string.IsNullOrEmpty(oldAvatarUrl))
                {
                    await _fileStorageService.DeleteFileAsync(oldAvatarUrl);
                }

                return result.BuildSuccess(
                    new CreateOrUpdateResponse { Id = user.Id },
                    "Profile updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (newAvatarUrl != null)
                {
                    await _fileStorageService.DeleteFileAsync(newAvatarUrl);
                }

                return result.BuildFail($"Failed to update profile: {ex.Message}");
            }
        }
    }
}
