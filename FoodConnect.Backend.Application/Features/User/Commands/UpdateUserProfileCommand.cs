using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.User.Commands
{
    public class UpdateUserProfileCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
