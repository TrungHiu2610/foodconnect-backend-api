using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Product.Commands;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Behaviors
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // product mappings
            CreateMap<CreateProductCommand, Product>()
                .ForMember(dest=>dest.Status,
                    opt=>opt.ConvertUsing(new StringToEnumConverter<ProductStatusEnum>(), src => src.Status))
                 .ForMember(dest => dest.ProductAssets, opt => opt.MapFrom(src => src.ProductAssets));

            CreateMap<ProductAssetDto, ProductAsset>()
                .ForMember(dest=>dest.AssetUrl, opt=>opt.MapFrom(src=>src.AssetUrl))
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) 
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<Product, GetListProductDetail>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => nameof(src.Status)))
                .ForMember(dest => dest.ProductAssets,
                           opt => opt.MapFrom(src => src.ProductAssets))
                .ForMember(dest => dest.ProductDailyAvailabilities,
                           opt => opt.MapFrom(src => src.ProductDailyAvailabilities));

            // category mappings
            CreateMap<Category, GetListCategoryDetail>()
                .ForMember(dest => dest.DeliveryType,
                    opt => opt.MapFrom(src => nameof(src.DeliveryType)))
                .ForMember(dest => dest.ParentName,
                    opt => opt.MapFrom(src => src.Parent.Name));
        }
    }
    public class StringToEnumConverter<TEnum> : IValueConverter<string, TEnum> where TEnum : struct
    {
        public TEnum Convert(string sourceMember, ResolutionContext context)
        {
            if (Enum.TryParse<TEnum>(sourceMember, true, out var result))
                return result;

            return default;
        }
    }
}
