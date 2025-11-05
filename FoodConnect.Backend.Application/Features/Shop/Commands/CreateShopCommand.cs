using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class CreateShopCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public string SellerFullName { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
        public string SellerPhone { get; set; } = string.Empty;
        
        // Address
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public int PayoutMethod { get; set; } 
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
        
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
        
        public IFormFile? IdCardFront { get; set; }
        public IFormFile? IdCardBack { get; set; }
        public IFormFile? PortraitPhoto { get; set; }
        public List<IFormFile> FoodSafetyCertificates { get; set; } = new List<IFormFile>(); // Bắt buộc
        
        public string? OperatingHoursJson { get; set; }
    }
}
