using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Cart.Queries;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, BaseResponse<CartResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public UpdateCartItemCommandHandler(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<BaseResponse<CartResponse>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartResponse>();

            var cartItem = await _cartItemRepository.GetByIdAsync(request.CartItemId, ci => ci.Cart);
            if (cartItem == null)
            {
                return result.BuildFail("Cart item not found");
            }

            var userId = _currentUserService.UserId;

            // Verify ownership
            if (userId.HasValue)
            {
                if (cartItem.Cart.UserId != userId.Value)
                {
                    return result.BuildFail("Unauthorized access to cart item");
                }
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                if (cartItem.Cart.SessionId != request.SessionId)
                {
                    return result.BuildFail("Unauthorized access to cart item");
                }
            }
            else
            {
                return result.BuildFail("Either user must be logged in or session ID must be provided");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.Quantity <= 0)
                {
                    // Remove item if quantity is 0 or less
                    _cartItemRepository.Remove(cartItem);
                }
                else
                {
                    // Update quantity
                    cartItem.Quantity = request.Quantity;
                    _cartItemRepository.Update(cartItem);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                // Get updated cart
                var getCartQuery = new GetCartQuery { SessionId = request.SessionId };
                var cartResponse = await _mediator.Send(getCartQuery, cancellationToken);

                return cartResponse;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Update cart item failed: {ex.Message}");
            }
        }
    }
}
