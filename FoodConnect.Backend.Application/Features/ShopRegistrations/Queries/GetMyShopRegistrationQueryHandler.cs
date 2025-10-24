using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Queries
{
    public class GetMyShopRegistrationQueryHandler : IRequestHandler<GetMyShopRegistrationQuery, BaseResponse<ShopRegistrationResponse>>
    {
        private readonly IShopRegistrationRepository _shopRegistrationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyShopRegistrationQueryHandler(
            IShopRegistrationRepository shopRegistrationRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _shopRegistrationRepository = shopRegistrationRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<ShopRegistrationResponse>> Handle(GetMyShopRegistrationQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ShopRegistrationResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildFail("User not found");
            }

            // Lấy đơn đăng ký mới nhất của user (bất kể trạng thái)
            var registration = await _shopRegistrationRepository.GetAsync(
                sr => sr.UserId == userId,
                r => r.User,
                r => r.Assets);

            if (registration == null)
            {
                return result.BuildFail("You don't have any shop registration");
            }

            var response = _mapper.Map<ShopRegistrationResponse>(registration);

            // Lấy tên người review nếu có
            if (registration.ReviewedBy.HasValue)
            {
                var reviewer = await _userRepository.GetByIdAsync(registration.ReviewedBy.Value);
                if (reviewer != null)
                {
                    response.ReviewedByFullName = reviewer.FullName;
                }
            }

            return result.BuildSuccess(response, "Get my shop registration successfully");
        }
    }
}
