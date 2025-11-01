using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Features.Category.Commands;
using FoodConnect.Backend.Application.Features.Product.Commands;
using FoodConnect.Backend.Application.Features.Shop.Commands;
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
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeliveryType,
                           opt => opt.MapFrom(src => src.Category.DeliveryType.ToString()))
                .ForMember(dest => dest.ProductAssets,
                           opt => opt.MapFrom(src => src.ProductAssets))
                .ForMember(dest => dest.ShopName,
                           opt => opt.MapFrom(src => src.Shop.ShopName));


            CreateMap<Product, GetListProductItemResponse>()
                .ForMember(dest => dest.ThumbnailUrl, 
                opt => opt.MapFrom(src => (src.ProductAssets != null && src.ProductAssets.Any())
                           ? src.ProductAssets.FirstOrDefault(a => a.IsThumbnail).AssetUrl ?? src.ProductAssets.FirstOrDefault().AssetUrl
                           : null
                ))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ShopName,
                           opt => opt.MapFrom(src => src.Shop.ShopName));

            // update product
            CreateMap<UpdateProductCommand, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ShopId, opt => opt.Ignore())
                .ForMember(dest => dest.Shop, opt => opt.Ignore())
                .ForMember(dest => dest.ProductAssets, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.StockQuantity, opt => opt.Ignore())
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
                    opt => opt.MapFrom(src => src.Parent.Name))
                .ForMember(dest => dest.DeliveryType,
                    opt => opt.MapFrom(src => src.DeliveryType.ToString()))
                .ForMember(dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products.Count()));

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

            #region shop mappings
            // get shop detail
            CreateMap<Domain.Entities.Shop, ShopResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PayoutMethod, opt => opt.MapFrom(src => (int)src.PayoutMethod))
                .ForMember(dest => dest.PayoutMethodName, opt => opt.MapFrom(src => src.PayoutMethod.ToString()))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => 
                    string.Join(", ", new[] { src.Street, src.Ward, src.District, src.City, src.Country }
                        .Where(s => !string.IsNullOrWhiteSpace(s)))))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtUtc))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAtUtc))
                .ForMember(dest => dest.Assets, opt => opt.MapFrom(src => src.Assets))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ShopCategories))
                .ForMember(dest => dest.OperatingHours, opt => opt.MapFrom(src => src.OperatingHours));

            CreateMap<ShopAsset, ShopAssetResponse>()
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => (int)src.AssetType))
                .ForMember(dest => dest.AssetTypeName, opt => opt.MapFrom(src => src.AssetType.ToString()));

            CreateMap<ShopCategory, ShopCategoryResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<ShopOperatingHour, ShopOperatingHourResponse>();

            // get shop list
            CreateMap<Domain.Entities.Shop, ShopListResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => 
                    string.Join(", ", new[] { src.Street, src.Ward, src.District, src.City, src.Country }
                        .Where(s => !string.IsNullOrWhiteSpace(s)))));

            // update shop - only map non-null properties
            CreateMap<UpdateShopCommand, Domain.Entities.Shop>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Assets, opt => opt.Ignore())
                .ForMember(dest => dest.ShopCategories, opt => opt.Ignore())
                .ForMember(dest => dest.OperatingHours, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CoverImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.AdminReason, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.PayoutMethod, opt => opt.MapFrom((src, dest) => 
                    src.PayoutMethod.HasValue ? (PaymentMethodEnum)src.PayoutMethod.Value : dest.PayoutMethod))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
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
            if (Enum.IsDefined(typeof(TEnum),(int) sourceMember))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), sourceMember);
            }

            return default;
        }
    }
}
