using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Queries
{
    public class GetShopRegistrationDetailQueryHandler : IRequestHandler<GetShopRegistrationDetailQuery, BaseResponse<ShopRegistrationResponse>>
    {
        private readonly IShopRegistrationRepository _shopRegistrationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetShopRegistrationDetailQueryHandler(
            IShopRegistrationRepository shopRegistrationRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _shopRegistrationRepository = shopRegistrationRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<ShopRegistrationResponse>> Handle(GetShopRegistrationDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ShopRegistrationResponse>();

            var registration = await _shopRegistrationRepository.GetByIdAsync(
                request.Id,
                r => r.User,
                r => r.Assets);

            if (registration == null)
            {
                return result.BuildFail("Shop registration not found");
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

            return result.BuildSuccess(response, "Get shop registration detail successfully");
        }
    }
}
