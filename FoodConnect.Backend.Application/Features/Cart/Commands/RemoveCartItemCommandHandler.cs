using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Cart.Queries;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, BaseResponse<CartResponse>>
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;

        public RemoveCartItemCommandHandler(
            ICartItemRepository cartItemRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMediator mediator)
        {
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<BaseResponse<CartResponse>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartResponse>();

            if (request.CartItemIds == null || !request.CartItemIds.Any())
            {
                return result.BuildFail("No cart items to remove");
            }

            var cartItems = new List<Domain.Entities.CartItem>();
            foreach (var itemId in request.CartItemIds)
            {
                var cartItem = await _cartItemRepository.GetByIdAsync(itemId, ci => ci.Cart);
                if (cartItem != null)
                {
                    cartItems.Add(cartItem);
                }
            }

            if (!cartItems.Any())
            {
                return result.BuildFail("Cart items not found");
            }

            var userId = _currentUserService.UserId;
            var firstCart = cartItems.First().Cart;

            if (userId.HasValue)
            {
                if (cartItems.Any(ci => ci.Cart.UserId != userId.Value))
                {
                    return result.BuildFail("Unauthorized access to cart items");
                }
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                if (cartItems.Any(ci => ci.Cart.SessionId != request.SessionId))
                {
                    return result.BuildFail("Unauthorized access to cart items");
                }
            }
            else
            {
                return result.BuildFail("Either user must be logged in or session ID must be provided");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var cartItem in cartItems)
                {
                    _cartItemRepository.Remove(cartItem);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                var getCartQuery = new GetCartQuery { SessionId = request.SessionId };
                var cartResponse = await _mediator.Send(getCartQuery, cancellationToken);

                return cartResponse;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Remove cart items failed: {ex.Message}");
            }
        }
    }
}
