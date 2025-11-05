using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Product.Commands;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, BaseResponse<CreateCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, 
            IMapper mapper, IFileStorageService fileStorageService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }
        
        public async Task<BaseResponse<CreateCategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateCategoryResponse>();
            var response = new CreateCategoryResponse();

            // Validate ParentId
            Domain.Entities.Category? parentCategory = null;
            if (request.ParentId != null)
            {
                parentCategory = _categoryRepository.GetByIdAsync((Guid)request.ParentId).Result;
                if (parentCategory == null)
                {
                    return result.BuildFail("Parent category not found");
                }
                
                if (!string.IsNullOrEmpty(request.DeliveryType) && request.DeliveryType != DeliveryTypeEnum.Standard.ToString())
                {
                    return result.BuildFail("Child category cannot have a custom delivery type. It will inherit from parent category.");
                }
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            // for rollback to delete uploaded image if any error occurs
            string? uploadFile = null;
            try
            {
                // map to entity
                var category = _mapper.Map<Domain.Entities.Category>(request);
                category.Id = Guid.NewGuid();
                
                // If this is a child category, inherit DeliveryType from parent
                if (parentCategory != null)
                {
                    category.DeliveryType = parentCategory.DeliveryType;
                }

                // save file
                if (request.File != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_CATEGORY}/{category.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.File, prefix);
                    category.ImageUrl = url;
                    uploadFile = url;
                }
                await _categoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.Id = category.Id;
                response.IsSuccess = true;
                return result.BuildSuccess(response, "Category created successfully");
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction);
                if (string.IsNullOrEmpty(uploadFile))
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(uploadFile);
                    }
                    catch (Exception delEx)
                    {
                        return result.BuildFail($"Failed to delete uploaded file: {delEx.Message}");
                    }
                }
                return result.BuildFail($"Create category failed: {ex.Message}");
            }
        }
    }
}
