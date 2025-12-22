using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, BaseResponse<DeleteProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRedisService _redisService;

        public DeleteProductCommandHandler(IProductRepository productRepository, IShopRepository shopRepository,
            IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IRedisService redisService)
        {
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _redisService = redisService;
        }
        public async Task<BaseResponse<DeleteProductResponse>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<DeleteProductResponse>();

            var userId = _currentUserService.UserId;
            if(userId == null)
            {
                return result.BuildFail("User not found");
            }
            var shop = await _shopRepository.GetAsync(s=>s.User.Id==userId, s=>s.User);
            if (shop == null)
            {
                return result.BuildFail("Shop not found for this user");
            }
            if(shop.Status != ShopStatusEnum.Active)
            {
                return result.BuildFail("Shop is not active");
            }
            if(shop.User.Id != userId)
            {
                return result.BuildFail("You do not have permission to delete product for this shop");
            }

            if (request.Ids == null || !request.Ids.Any())
            {
                return result.BuildFail("Id is required");
            }
            var response = new DeleteProductResponse();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var products = await _productRepository.GetByIdsAsync(request.Ids);
                if (products == null || !products.Any())
                {
                    return result.BuildFail("Products not found");
                }
                _productRepository.RemoveRange(products);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                await _redisService.DeleteByPatternAsync("products:list:*");

                response.DeletedCount = request.Ids.Count;
                return result.BuildSuccess(response, "Delete products successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result.BuildFail(ex.Message);
            }
        }
    }
}
