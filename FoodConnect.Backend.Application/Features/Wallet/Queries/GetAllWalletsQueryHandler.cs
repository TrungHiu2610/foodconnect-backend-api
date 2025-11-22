using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetAllWalletsQueryHandler : IRequestHandler<GetAllWalletsQuery, BaseResponse<List<WalletResponse>>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAllWalletsQueryHandler(
        IWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<WalletResponse>>> Handle(GetAllWalletsQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<List<WalletResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var wallets = await _walletRepository.GetWalletsByUserIdAsync(userId.Value);
        var response = _mapper.Map<List<WalletResponse>>(wallets);
        
        return result.BuildSuccess(response, "Wallets retrieved successfully");
    }
}
