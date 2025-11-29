using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Services
{
    public class WalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Wallet> GetOrCreateWalletAsync(Guid userId, WalletTypeEnum walletType, CancellationToken cancellationToken = default)
        {
            var wallet = await _walletRepository.GetByUserIdAndTypeAsync(userId, walletType);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    WalletType = walletType,
                    Balance = 0,
                    TotalEarned = 0,
                    TotalWithdrawn = 0,
                    PendingBalance = 0,
                    TotalSpent = 0,
                    Status = WalletStatusEnum.Active
                };

                await _walletRepository.AddAsync(wallet);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return wallet;
        }

        public async Task<Wallet> GetOrCreateBuyerWalletAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await GetOrCreateWalletAsync(userId, WalletTypeEnum.Buyer, cancellationToken);
        }

        public async Task<Wallet> GetOrCreateSellerWalletAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await GetOrCreateWalletAsync(userId, WalletTypeEnum.Seller, cancellationToken);
        }
    }
}
