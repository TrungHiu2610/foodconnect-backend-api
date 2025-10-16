using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQuery : IRequest<BaseResponse<GetListProductResponse>>
    {
    }
}
