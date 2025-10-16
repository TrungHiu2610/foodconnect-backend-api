using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Features.Product.Commands;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Behaviors
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region product mappings

            // create product
            CreateMap<CreateProductCommand, Product>()
                .ForMember(dest => dest.Status,
                    opt => opt.ConvertUsing(new StringToEnumConverter<ProductStatusEnum>(), src => src.Status))
                 .ForMember(dest => dest.ProductAssets, opt => opt.MapFrom(src => src.ProductAssets));

            CreateMap<ProductAssetCreateDto, ProductAsset>()
                .ForMember(dest => dest.AssetUrl, opt => opt.MapFrom(src => src.AssetUrl))
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ReverseMap();

            // get product
            CreateMap<ProductAssetGetDto, ProductAsset>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Product, GetListProductDetail>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => nameof(src.Status)))
                .ForMember(dest => dest.ProductAssets,
                           opt => opt.MapFrom(src => src.ProductAssets))
                .ForMember(dest => dest.ProductDailyAvailabilities,
                           opt => opt.MapFrom(src => src.ProductDailyAvailabilities));

            // update product
            CreateMap<UpdateProductCommand, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Shop, opt => opt.Ignore())
                .ForMember(dest => dest.ProductAssets, opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.ConvertUsing(new StringToEnumConverter<ProductStatusEnum>(), src => src.Status))
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            #endregion

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
