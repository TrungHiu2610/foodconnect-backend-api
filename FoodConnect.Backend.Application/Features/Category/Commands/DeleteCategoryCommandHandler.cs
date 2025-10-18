using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, BaseResponse<DeleteCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork,
            IMapper mapper, IFileStorageService fileStorageService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }
        public async Task<BaseResponse<DeleteCategoryResponse>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<DeleteCategoryResponse>();
            var response = new DeleteCategoryResponse();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var categories = await _categoryRepository.GetByIdsAsync(request.Ids);
                var foundIds = categories.Select(c => c.Id).ToHashSet();
                var missingIds = request.Ids.Where(id => !foundIds.Contains(id)).ToList();

                if (missingIds.Any())
                {
                    return result.BuildFail($"Some categories not found: {string.Join(", ", missingIds)}");
                }

                _categoryRepository.RemoveRange(categories);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.DeletedCount = request.Ids.Count;
                return result.BuildSuccess(response, "Categories deleted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction);
                return result.BuildFail($"An error occurred while deleting the category: {ex.Message}");
            }
        }
    }
}
