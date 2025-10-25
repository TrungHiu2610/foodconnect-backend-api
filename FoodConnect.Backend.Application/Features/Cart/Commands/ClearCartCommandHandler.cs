using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, BaseResponse<CartResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ClearCartCommandHandler(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CartResponse>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartResponse>();

            var userId = _currentUserService.UserId;

            Domain.Entities.Cart? cart;
            if (userId.HasValue)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(userId, null);
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                cart = await _cartRepository.GetCartWithItemsAsync(null, request.SessionId);
            }
            else
            {
                return result.BuildFail("Either user must be logged in or session ID must be provided");
            }

            if (cart == null)
            {
                return result.BuildFail("Cart not found");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cartItems = await _cartItemRepository.GetCartItemsByCartIdAsync(cart.Id);
                _cartItemRepository.RemoveRange(cartItems);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                var response = new CartResponse
                {
                    Id = cart.Id,
                    UserId = cart.UserId != Guid.Empty ? cart.UserId : null,
                    SessionId = cart.SessionId,
                    Items = new List<CartItemResponse>(),
                    TotalItems = 0,
                    TotalAmount = 0,
                    CreatedAtUtc = cart.CreatedAtUtc,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                return result.BuildSuccess(response, "Cart cleared successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Clear cart failed: {ex.Message}");
            }
        }
    }
}
