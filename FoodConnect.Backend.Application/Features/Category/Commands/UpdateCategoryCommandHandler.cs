using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using MediatR;
using FoodConnect.Backend.Application.Commons.Constants;
using System.Text.RegularExpressions;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, BaseResponse<UpdateCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork,
            IMapper mapper, IFileStorageService fileStorageService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<BaseResponse<UpdateCategoryResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<UpdateCategoryResponse>();
            var response = new UpdateCategoryResponse();

            // validate if category exists
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
            {
                return result.BuildFail("Category not found");
            }
            // validate ParentId
            Domain.Entities.Category? parentCategory = null;
            if (request.ParentId != null)
            {
                parentCategory = await _categoryRepository.GetByIdAsync((Guid)request.ParentId);
                if (parentCategory == null)
                {
                    return result.BuildFail("Parent category not found");
                }
                
                if (!string.IsNullOrEmpty(request.DeliveryType) && request.DeliveryType != Domain.Enums.DeliveryTypeEnum.Standard.ToString())
                {
                    return result.BuildFail("Child category cannot have a custom delivery type. It will inherit from parent category.");
                }
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            string? uploadFile = null;

            try
            {
                // map to entity
                _mapper.Map(request, category);
                
                // If ParentId is being set/updated, inherit DeliveryType from parent
                if (parentCategory != null)
                {
                    category.DeliveryType = parentCategory.DeliveryType;
                }

                // update file
                if (request.File != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_CATEGORY}/{category.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.File, prefix);
                    uploadFile = url;
                    category.ImageUrl = url;
                }
                _categoryRepository.Update(category);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.Id = category.Id;
                response.IsSuccess = true;

                return result.BuildSuccess(response, "Update category successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction);
                // delete uploaded file if error
                if (uploadFile != null)
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(uploadFile);
                    }
                    catch(Exception delEx)
                    {
                        return result.BuildFail($"Failed to delete uploaded file: {delEx.Message}");
                    }
                }
                return result.BuildFail("Update category failed", 400, new List<string> { ex.Message });
            }
        }
    }
}
