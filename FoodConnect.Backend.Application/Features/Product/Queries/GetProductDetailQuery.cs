using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetProductDetailQuery : IRequest<BaseResponse<GetProductDetailResponse>>
    {
        public Guid Id { get; set; }
    }
}
