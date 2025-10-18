using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class DeleteCategoryCommand : IRequest<BaseResponse<DeleteCategoryResponse>>
    {
        public required List<Guid> Ids { get; set; }
    }
}
