using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wishlist;
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
                .ForMember(dest => dest.ParentCategoryId,
                           opt => opt.MapFrom(src => src.Category.ParentId))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeliveryType,
                           opt => opt.MapFrom(src => src.Category.DeliveryType.ToString()))
                .ForMember(dest => dest.ProductAssets,
                           opt => opt.MapFrom(src => src.ProductAssets))
                .ForMember(dest => dest.ShopName,
                           opt => opt.MapFrom(src => src.Shop.ShopName))
                .ForMember(dest => dest.ShopId,
                           opt => opt.MapFrom(src => src.Shop.Id));


            CreateMap<Product, GetListProductItemResponse>()
                .ForMember(dest => dest.ThumbnailUrl, 
                opt => opt.MapFrom(src => (src.ProductAssets != null && src.ProductAssets.Any())
                           ? src.ProductAssets.FirstOrDefault(a => a.IsThumbnail).AssetUrl ?? src.ProductAssets.FirstOrDefault().AssetUrl
                           : null
                ))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DeliveryType,
                           opt => opt.MapFrom(src => src.Category.DeliveryType.ToString()))
                .ForMember(dest => dest.ShopName,
                           opt => opt.MapFrom(src => src.Shop.ShopName))
                .ForMember(dest => dest.ShopId,
                           opt => opt.MapFrom(src => src.Shop.Id));

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

            #region address mappings
            // update address - only map non-null properties
            CreateMap<Application.Features.Address.Commands.UpdateAddressCommand, Domain.Entities.Address>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.AddressType, opt => opt.MapFrom((src, dest) => 
                    src.AddressType.HasValue ? (AddressTypeEnum)src.AddressType.Value : dest.AddressType))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region product review mappings

            // ProductReviewAsset → ProductReviewAssetDto
            CreateMap<ProductReviewAsset, ProductReviewAssetDto>()
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => (int)src.AssetType))
                .ForMember(dest => dest.AssetTypeName, opt => opt.MapFrom(src => src.AssetType.ToString()));

            // ProductReview → ProductReviewResponse
            CreateMap<ProductReview, ProductReviewResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => src.Buyer.FullName))
                .ForMember(dest => dest.BuyerAvatarUrl, opt => opt.MapFrom(src => src.Buyer.AvatarUrl))
                .ForMember(dest => dest.Assets, opt => opt.MapFrom(src => src.Assets))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtUtc));

            #endregion

            #region wishlist mappings

            CreateMap<Wishlist, WishlistResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtUtc))
                .ForMember(dest => dest.Item, opt => opt.MapFrom(src => 
                    src.Type == WishlistTypeEnum.Product && src.Product != null
                        ? new WishlistItemDto
                        {
                            Id = src.Product.Id,
                            Name = src.Product.Name,
                            Description = src.Product.Description,
                            ImageUrl = src.Product.ProductAssets != null && src.Product.ProductAssets.Any()
                                ? src.Product.ProductAssets.FirstOrDefault(a => a.IsThumbnail)!.AssetUrl ?? src.Product.ProductAssets.FirstOrDefault()!.AssetUrl
                                : null,
                            Price = src.Product.Price,
                            IsAvailable = src.Product.IsAvailable,
                            Status = src.Product.Status.ToString()
                        }
                        : src.Type == WishlistTypeEnum.Shop && src.Shop != null
                            ? new WishlistItemDto
                            {
                                Id = src.Shop.Id,
                                Name = src.Shop.ShopName,
                                Description = src.Shop.Description,
                                ImageUrl = src.Shop.LogoUrl,
                                Rating = src.Shop.Rating,
                                ReviewCount = src.Shop.ReviewCount,
                                Status = src.Shop.Status.ToString()
                            }
                            : null));

            #endregion

            #region promotion mappings

            CreateMap<Domain.Entities.Promotion, PromotionResponse>()
                .ForMember(dest => dest.PromotionType, opt => opt.MapFrom(src => (int)src.PromotionType))
                .ForMember(dest => dest.PromotionTypeName, opt => opt.MapFrom(src => src.PromotionType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.ShopName))
                .ForMember(dest => dest.RemainingUsage, opt => opt.MapFrom(src => 
                    src.MaxUsageCount.HasValue ? src.MaxUsageCount.Value - src.TotalUsedCount : 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtUtc))
                .ForMember(dest => dest.ApplicableProducts, opt => opt.MapFrom(src => 
                    src.PromotionProducts.Select(pp => new PromotionProductDto
                    {
                        ProductId = pp.ProductId,
                        ProductName = pp.Product.Name,
                        ProductPrice = pp.Product.Price,
                        ThumbnailUrl = pp.Product.ProductAssets != null && pp.Product.ProductAssets.Any()
                            ? pp.Product.ProductAssets.FirstOrDefault(a => a.IsThumbnail)!.AssetUrl ?? pp.Product.ProductAssets.FirstOrDefault()!.AssetUrl
                            : null
                    }).ToList()));

            CreateMap<Domain.Entities.Promotion, PromotionListResponse>()
                .ForMember(dest => dest.PromotionType, opt => opt.MapFrom(src => (int)src.PromotionType))
                .ForMember(dest => dest.PromotionTypeName, opt => opt.MapFrom(src => src.PromotionType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.ShopName))
                .ForMember(dest => dest.RemainingUsage, opt => opt.MapFrom(src => 
                    src.MaxUsageCount.HasValue ? src.MaxUsageCount.Value - src.TotalUsedCount : (int?)null))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src=>src.PromotionProducts.Count()));

            CreateMap<Domain.Entities.Promotion, PromotionDetailForBuyerResponse>()
                .ForMember(dest => dest.PromotionType, opt => opt.MapFrom(src => (int)src.PromotionType))
                .ForMember(dest => dest.PromotionTypeName, opt => opt.MapFrom(src => src.PromotionType.ToString()))
                .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.ShopId))
                .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.ShopName))
                .ForMember(dest => dest.ShopLogoUrl, opt => opt.MapFrom(src => src.Shop.LogoUrl))
                .ForMember(dest => dest.RemainingUsage, opt => opt.MapFrom(src => 
                    src.MaxUsageCount.HasValue ? src.MaxUsageCount.Value - src.TotalUsedCount : 0))
                .ForMember(dest => dest.CanUse, opt => opt.Ignore())
                .ForMember(dest => dest.UserUsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicableProducts, opt => opt.MapFrom(src => 
                    src.PromotionProducts.Select(pp => new PromotionProductForBuyerDto
                    {
                        ProductId = pp.ProductId,
                        ProductName = pp.Product.Name,
                        OriginalPrice = pp.Product.Price,
                        DiscountedPrice = src.PromotionType == PromotionTypeEnum.Percentage
                            ? pp.Product.Price - (pp.Product.Price * src.DiscountValue / 100)
                            : pp.Product.Price - src.DiscountValue,
                        DiscountAmount = src.PromotionType == PromotionTypeEnum.Percentage
                            ? pp.Product.Price * src.DiscountValue / 100
                            : src.DiscountValue,
                        ThumbnailUrl = pp.Product.ProductAssets != null && pp.Product.ProductAssets.Any()
                            ? pp.Product.ProductAssets.FirstOrDefault(a => a.IsThumbnail)!.AssetUrl ?? pp.Product.ProductAssets.FirstOrDefault()!.AssetUrl
                            : null,
                        IsAvailable = pp.Product.IsAvailable,
                        StockQuantity = pp.Product.StockQuantity
                    }).ToList()));

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
