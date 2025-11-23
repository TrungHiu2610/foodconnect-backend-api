using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetWalletQueryHandler : IRequestHandler<GetWalletQuery, BaseResponse<WalletResponse>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetWalletQueryHandler(
        IWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<WalletResponse>> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<WalletResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var walletType = (WalletTypeEnum)request.WalletType;
        var wallet = await _walletRepository.GetByUserIdAndTypeAsync(userId.Value, walletType);
        
        if (wallet == null)
        {
            // Auto-create wallet if not exists
            wallet = new Domain.Entities.Wallet
            {
                UserId = userId.Value,
                WalletType = walletType,
                Balance = 0,
                TotalEarned = 0,
                TotalWithdrawn = 0,
                PendingBalance = 0,
                TotalSpent = 0,
                Status = WalletStatusEnum.Active
            };

            await _walletRepository.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
        }

        var response = _mapper.Map<WalletResponse>(wallet);
        return result.BuildSuccess(response, "Wallet retrieved successfully");
    }
}
