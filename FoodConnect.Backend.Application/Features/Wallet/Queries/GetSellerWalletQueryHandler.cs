using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetSellerWalletQueryHandler : IRequestHandler<GetSellerWalletQuery, BaseResponse<SellerWalletResponse>>
{
    private readonly ISellerWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSellerWalletQueryHandler(
        ISellerWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<SellerWalletResponse>> Handle(GetSellerWalletQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<SellerWalletResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var wallet = await _walletRepository.GetBySellerIdAsync(userId.Value);
        
        if (wallet == null)
        {
            wallet = new SellerWallet
            {
                SellerId = userId.Value,
                Balance = 0,
                TotalEarned = 0,
                TotalWithdrawn = 0,
                PendingBalance = 0,
                Status = SellerWalletStatusEnum.Active
            };

            await _walletRepository.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
        }

        var response = _mapper.Map<SellerWalletResponse>(wallet);
        return result.BuildSuccess(response, "Wallet retrieved successfully");
    }
}
