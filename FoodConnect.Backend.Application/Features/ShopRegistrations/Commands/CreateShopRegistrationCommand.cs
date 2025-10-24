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
        public string ShopName { get; set; } = string.Empty;
        public string ShopDescription { get; set; } = string.Empty;
        public short PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
        
        // Address fields
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        // Categories
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
        
        // Operating Hours
        public List<OperatingHourDto> OperatingHours { get; set; } = new List<OperatingHourDto>();

        // Files
        public IFormFile IdCardFront { get; set; } = null!;
        public IFormFile IdCardBack { get; set; } = null!;
        public IFormFile PortraitPhoto { get; set; } = null!;
        public List<IFormFile> KitchenPhotos { get; set; } = new List<IFormFile>();
        public IFormFile? FoodSafetyCertificate { get; set; } 
    }

    public class OperatingHourDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
    }
}
