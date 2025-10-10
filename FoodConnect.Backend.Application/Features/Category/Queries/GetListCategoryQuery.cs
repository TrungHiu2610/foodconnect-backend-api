using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetListCategoryQuery : IRequest<BaseResponse<GetListCategoryResponse>>
    {
    }
}
