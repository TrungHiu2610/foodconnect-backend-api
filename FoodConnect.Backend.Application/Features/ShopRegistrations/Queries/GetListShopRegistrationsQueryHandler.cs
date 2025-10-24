using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using AutoMapper;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Queries
{
    public class GetListShopRegistrationsQueryHandler : IRequestHandler<GetListShopRegistrationsQuery, BaseResponse<PagedResponse<ShopRegistrationListResponse>>>
    {
        private readonly IShopRegistrationRepository _shopRegistrationRepository;
        private readonly IMapper _mapper;

        public GetListShopRegistrationsQueryHandler(
            IShopRegistrationRepository shopRegistrationRepository,
            IMapper mapper)
        {
            _shopRegistrationRepository = shopRegistrationRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PagedResponse<ShopRegistrationListResponse>>> Handle(GetListShopRegistrationsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PagedResponse<ShopRegistrationListResponse>>();

            var (items, totalCount) = await _shopRegistrationRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.Status,
                request.SearchTerm);

            var response = new PagedResponse<ShopRegistrationListResponse>
            {
                Items = _mapper.Map<List<ShopRegistrationListResponse>>(items),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return result.BuildSuccess(response, "Get list shop registrations successfully");
        }
    }
}
