using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Queries
{
    public class GetListShopRegistrationsQuery : IRequest<BaseResponse<PagedResponse<ShopRegistrationListResponse>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ShopRegistrationStatusEnum? Status { get; set; }
        public string? SearchTerm { get; set; }
    }
}
