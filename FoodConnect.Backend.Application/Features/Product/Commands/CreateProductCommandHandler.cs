using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<CreateProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductCommandHandler(IProductRepository productRepository, IShopRepository shopRepository,
            ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper, 
            IFileStorageService fileStorageService, ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
        }
        public async Task<BaseResponse<CreateProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateProductResponse>();
            var response = new CreateProductResponse();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildFail("User not found");
            }

            var shop = await _shopRepository.GetByUserIdAsync((Guid)userId);
            if (shop == null)
            {
                return result.BuildFail("Shop not found for this user");
            }

            if (shop.Status != ShopStatusEnum.Active)
            {
                return result.BuildFail("Shop is not active");
            }

            if(shop.User.Id !=  userId)
            {
                return result.BuildFail("You do not have permission to create product for this shop");
            }

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                return result.BuildFail("Category not found");
            }

            var product = _mapper.Map<Domain.Entities.Product>(request);
            product.ShopId = shop.Id;
            product.Id = Guid.NewGuid();

            // Apply inventory logic based on DeliveryType
            if (category.DeliveryType == DeliveryTypeEnum.Standard)
            {
                // Standard products: Auto-set IsAvailable based on stock only
                product.IsAvailable = request.StockQuantity > 0;
            }
            else
            {
                // Express products: Use seller's IsAvailable setting, ignore stock
                product.IsAvailable = request.IsAvailable;
                product.StockQuantity = null;
            }

            var uploadedFiles = new List<string>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.ProductAssets != null)
                {
                    // validate thumbnail
                    if (request.ProductAssets.All(a => !a.IsThumbnail))
                    {
                        return result.BuildFail("Thumbnail is required");
                    }
                    if (request.ProductAssets.Count(a => a.IsThumbnail) > 1)
                    {
                        return result.BuildFail("Only one thumbnail is allowed");
                    }

                    foreach (var assetDto in request.ProductAssets)
                    {
                        if (assetDto.File != null)
                        {
                            assetDto.AssetName = assetDto.File.FileName;
                            var prefix = (assetDto.File.ContentType.ToLower().StartsWith("image")) ? AWSDirectoryConstant.IMAGE_PRODUCT : AWSDirectoryConstant.VIDEO_PRODUCT;
                            prefix += $"/{shop.Id}/{product.Id}";
                            
                            var url = await _fileStorageService.UploadFileAsync(assetDto.File, prefix);
                            uploadedFiles.Add(url);
                            assetDto.AssetUrl = url;
                        }
                    }

                    product.ProductAssets = _mapper.Map<ICollection<ProductAsset>>(request.ProductAssets);
                }

                await _productRepository.AddAsync(product);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.Id = product.Id;
                response.IsSuccess = true;

                return result.BuildSuccess(response, "Create product success");
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                if (uploadedFiles.Any())
                {
                    foreach (var fileUrl in uploadedFiles)
                    {
                        try
                        {
                            await _fileStorageService.DeleteFileAsync(fileUrl);
                        }
                        catch (Exception delEx)
                        {
                            return result.BuildFail($"Failed to delete uploaded file: {delEx.Message}");
                        }
                    }
                }
                return result.BuildFail($"Create product failed: {ex.Message}");
            }
        }
    }
}
