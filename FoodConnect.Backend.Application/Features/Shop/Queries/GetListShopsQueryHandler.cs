using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetListShopsQueryHandler : IRequestHandler<GetListShopsQuery, BaseResponse<PaginatedList<ShopListResponse>>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;

        public GetListShopsQueryHandler(
            IShopRepository shopRepository,
            IMapper mapper)
        {
            _shopRepository = shopRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PaginatedList<ShopListResponse>>> Handle(GetListShopsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<ShopListResponse>>();

            var (items, totalCount) = await _shopRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.Status,
                request.SearchTerm);

            var shopListResponses = _mapper.Map<List<ShopListResponse>>(items);

            var paginatedList = new PaginatedList<ShopListResponse>(
                shopListResponses,
                totalCount,
                request.Page,
                request.PageSize);

            return result.BuildSuccess(paginatedList, "Get shops successfully");
        }
    }
}
