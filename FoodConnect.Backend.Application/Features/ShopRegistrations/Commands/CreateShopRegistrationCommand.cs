using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class CreateShopRegistrationCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public short PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; }
        public string PayoutAccountName { get; set; }
        public List<Guid> CategoryIds { get; set; }

        public IFormFile IdCardFront { get; set; }
        public IFormFile IdCardBack { get; set; }
        public IFormFile PortraitPhoto { get; set; }
        public List<IFormFile> KitchenPhotos { get; set; }
        public IFormFile? FoodSafetyCertificate { get; set; } 
    }
}
