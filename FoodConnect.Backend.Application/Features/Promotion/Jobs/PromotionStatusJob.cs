using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Features.Promotion.Services;
using FoodConnect.Backend.Application.Features.Wishlist.Services;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.Promotion.Jobs
{
    public class PromotionStatusJob
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PromotionStatusJob> _logger;
        private readonly PromotionNotificationService _promotionNotificationService;
        private readonly ShopFollowerNotificationService _shopFollowerNotificationService;

        public PromotionStatusJob(
            IPromotionRepository promotionRepository,
            IUnitOfWork unitOfWork,
            ILogger<PromotionStatusJob> logger,
            PromotionNotificationService promotionNotificationService,
            ShopFollowerNotificationService shopFollowerNotificationService)
        {
            _promotionRepository = promotionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _promotionNotificationService = promotionNotificationService;
            _shopFollowerNotificationService = shopFollowerNotificationService;
        }
        public async Task AutoActivatePromotionsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                _logger.LogInformation("[PromotionStatusJob] Running auto-activate job at {Time}", now);

                // Find all Approved promotions where StartDate has passed
                var promotionsToActivate = await _promotionRepository.GetPromotionsByStatusAsync(PromotionStatusEnum.Approved);
                
                var activatedCount = 0;
                foreach (var promotion in promotionsToActivate.Where(p => p.StartDate <= now))
                {
                    promotion.Status = PromotionStatusEnum.Active;
                    promotion.UpdatedAtUtc = now;
                    _promotionRepository.Update(promotion);
                    activatedCount++;
                    
                    _logger.LogInformation(
                        "[PromotionStatusJob] Auto-activated promotion {PromotionId} - {PromotionName}", 
                        promotion.Id, 
                        promotion.PromotionName);

                    // Send notification to shop owner (async but don't wait)
                    _ = _promotionNotificationService.NotifyPromotionActivatedAsync(promotion);
                    
                    // Notify all shop followers about new active promotion
                    _ = _shopFollowerNotificationService.NotifyFollowersAboutPromotionAsync(promotion);
                }

                if (activatedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("[PromotionStatusJob] Auto-activated {Count} promotions", activatedCount);
                }
                else
                {
                    _logger.LogInformation("[PromotionStatusJob] No promotions to activate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PromotionStatusJob] Error during auto-activate: {Message}", ex.Message);
            }
        }

        public async Task AutoExpirePromotionsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                _logger.LogInformation("[PromotionStatusJob] Running auto-expire job at {Time}", now);

                // Find all Active promotions where EndDate has passed
                var activePromotions = await _promotionRepository.GetPromotionsByStatusAsync(PromotionStatusEnum.Active);
                
                var expiredCount = 0;
                foreach (var promotion in activePromotions.Where(p => p.EndDate <= now))
                {
                    promotion.Status = PromotionStatusEnum.Expired;
                    promotion.UpdatedAtUtc = now;
                    _promotionRepository.Update(promotion);
                    expiredCount++;
                    
                    _logger.LogInformation(
                        "[PromotionStatusJob] Auto-expired promotion {PromotionId} - {PromotionName}", 
                        promotion.Id, 
                        promotion.PromotionName);

                    // Send notification (async but don't wait)
                    _ = _promotionNotificationService.NotifyPromotionExpiredAsync(promotion);
                }

                if (expiredCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("[PromotionStatusJob] Auto-expired {Count} promotions", expiredCount);
                }
                else
                {
                    _logger.LogInformation("[PromotionStatusJob] No promotions to expire");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PromotionStatusJob] Error during auto-expire: {Message}", ex.Message);
            }
        }

        public async Task ProcessPromotionStatusChangesAsync()
        {
            _logger.LogInformation("[PromotionStatusJob] Starting promotion status check...");
            
            await AutoActivatePromotionsAsync();
            await AutoExpirePromotionsAsync();
            
            _logger.LogInformation("[PromotionStatusJob] Completed promotion status check");
        }
    }
}
