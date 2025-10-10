using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQueryHandler : IRequestHandler<GetListProductQuery, BaseResponse<GetListProductResponse>>
    {
        private readonly IProductRepository _productRepository;

        public GetListProductQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<BaseResponse<GetListProductResponse>> Handle(GetListProductQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListProductResponse>();

            var listProducts = await _productRepository.GetListProductResponseAsync();
            if(listProducts == null || listProducts.Products == null)
            {
                return result.BuildFail("No products found");
            }
            return result.BuildSuccess(listProducts, "Get list products successfully");
        }
    }
}
