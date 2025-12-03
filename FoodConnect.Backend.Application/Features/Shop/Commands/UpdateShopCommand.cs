using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class UpdateShopCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid ShopId { get; set; }
        
        public string? ShopName { get; set; }
        public string? Description { get; set; }

        public string? SellerFullName { get; set; } 
        public string? SellerEmail { get; set; } 
        public string? SellerPhone { get; set; }

        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public int? PayoutMethod { get; set; }
        public string? PayoutAccountInfo { get; set; }
        public string? PayoutAccountName { get; set; }
        
        public List<Guid>? CategoryIds { get; set; }
        
        public List<Guid>? AssetIdsToDelete { get; set; }
        
        public IFormFile? IdCardFront { get; set; }
        public IFormFile? IdCardBack { get; set; }
        public IFormFile? PortraitPhoto { get; set; }
        public List<IFormFile>? FoodSafetyCertificates { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? CoverImage { get; set; }
        
        public string? OperatingHoursJson { get; set; }
    }
}
