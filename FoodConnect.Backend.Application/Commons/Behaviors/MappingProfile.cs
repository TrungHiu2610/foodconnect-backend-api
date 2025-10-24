using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration;
using FoodConnect.Backend.Application.Features.Category.Commands;
using FoodConnect.Backend.Application.Features.Product.Commands;
using FoodConnect.Backend.Application.Features.ShopRegistrations.Commands;
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

            CreateMap<Product, GetProductDetailResponse>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => nameof(src.Status)))
                .ForMember(dest => dest.ProductAssets,
                           opt => opt.MapFrom(src => src.ProductAssets));
                //.ForMember(dest => dest.ProductDailyAvailabilities,
                //           opt => opt.MapFrom(src => src.ProductDailyAvailabilities));

            CreateMap<Product, GetListProductItemResponse>()
                .ForMember(dest => dest.ThumbnailUrl, 
                opt => opt.MapFrom(src => (src.ProductAssets != null && src.ProductAssets.Any())
                           ? src.ProductAssets.FirstOrDefault(a => a.IsThumbnail).AssetUrl ?? src.ProductAssets.FirstOrDefault().AssetUrl
                           : null
                ));

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

            #region category mappings
            // get category
            CreateMap<Category, GetListCategoryItem>()
                .ForMember(dest => dest.ParentName,
                    opt => opt.MapFrom(src => src.Parent.Name));

            // create category
            CreateMap<CreateCategoryCommand, Category>()
                .ForMember(dest=>dest.DeliveryType,
                    opt => opt.ConvertUsing(new StringToEnumConverter<DeliveryTypeEnum>(), src => src.DeliveryType))
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // update category
            CreateMap<UpdateCategoryCommand, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryType,
                    opt => opt.ConvertUsing(new StringToEnumConverter<DeliveryTypeEnum>(), src => src.DeliveryType))
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region shop registration mappings
            // create shop registration
            CreateMap<CreateShopRegistrationCommand, ShopRegistration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AdminReason, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Assets, opt => opt.Ignore())
                .ForMember(dest => dest.PayoutMethod, 
                    opt => opt.ConvertUsing(new ShortToEnumConverter<PayoutMethodTypeEnum>(), src => src.PayoutMethod));

            // shop registration to response
            CreateMap<ShopRegistration, ShopRegistrationResponse>()
                .ForMember(dest => dest.UserFullName, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.UserEmail, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.ReviewedByFullName, opt => opt.Ignore())
                .ForMember(dest => dest.Assets, 
                    opt => opt.MapFrom(src => src.Assets));

            CreateMap<ShopRegistrationAsset, ShopRegistrationAssetResponse>();

            // shop registration to list response
            CreateMap<ShopRegistration, ShopRegistrationListResponse>()
                .ForMember(dest => dest.UserFullName, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.UserEmail, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));
            #endregion
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

    public class ShortToEnumConverter<TEnum> : IValueConverter<short, TEnum> where TEnum : struct
    {
        public TEnum Convert(short sourceMember, ResolutionContext context)
        {
            if (Enum.IsDefined(typeof(TEnum), (int)sourceMember))
                return (TEnum)(object)sourceMember;

            return default;
        }
    }
}
