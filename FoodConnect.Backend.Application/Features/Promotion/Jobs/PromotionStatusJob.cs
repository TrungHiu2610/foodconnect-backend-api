using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Features.Promotion.Services;
using FoodConnect.Backend.Application.Features.Wishlist.Services;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Hangfire;

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
        
        [DisableConcurrentExecution(timeoutInSeconds: 600)] // Prevent concurrent runs, 10 min timeout
        public async Task AutoActivatePromotionsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                _logger.LogInformation("[PromotionStatusJob] Running auto-activate job at {Time}", now);

                var promotionsToActivate = await _promotionRepository.GetPromotionsByStatusAsync(PromotionStatusEnum.Approved);
                
                var toActivate = promotionsToActivate.Where(p => p.StartDate <= now).ToList();
                
                if (toActivate.Count == 0)
                {
                    _logger.LogInformation("[PromotionStatusJob] No promotions to activate");
                    return;
                }

                foreach (var promotion in toActivate)
                {
                    promotion.Status = PromotionStatusEnum.Active;
                    promotion.UpdatedAtUtc = now;
                    _promotionRepository.Update(promotion);
                    
                    _logger.LogInformation(
                        "[PromotionStatusJob] Auto-activated promotion {PromotionId} - {PromotionName}", 
                        promotion.Id, 
                        promotion.PromotionName);

                    // Send notification to shop owner (async but don't wait)
                    _ = _promotionNotificationService.NotifyPromotionActivatedAsync(promotion);
                    
                    // Notify all shop followers about new active promotion
                    _ = _shopFollowerNotificationService.NotifyFollowersAboutPromotionAsync(promotion);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("[PromotionStatusJob] Auto-activated {Count} promotions", toActivate.Count);

                foreach (var promotion in toActivate)
                {
                    try
                    {
                        await _promotionNotificationService.NotifyPromotionActivatedAsync(promotion);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "[PromotionStatusJob] Failed to send notification for promotion {PromotionId}", promotion.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PromotionStatusJob] Error during auto-activate: {Message}", ex.Message);
            }
        }

        [DisableConcurrentExecution(timeoutInSeconds: 600)] // Prevent concurrent runs, 10 min timeout
        public async Task AutoExpirePromotionsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                _logger.LogInformation("[PromotionStatusJob] Running auto-expire job at {Time}", now);

                var activePromotions = await _promotionRepository.GetPromotionsByStatusAsync(PromotionStatusEnum.Active);
                
                var toExpire = activePromotions.Where(p => p.EndDate <= now).ToList();
                
                if (toExpire.Count == 0)
                {
                    _logger.LogInformation("[PromotionStatusJob] No promotions to expire");
                    return;
                }

                foreach (var promotion in toExpire)
                {
                    promotion.Status = PromotionStatusEnum.Expired;
                    promotion.UpdatedAtUtc = now;
                    _promotionRepository.Update(promotion);
                    
                    _logger.LogInformation(
                        "[PromotionStatusJob] Auto-expired promotion {PromotionId} - {PromotionName}", 
                        promotion.Id, 
                        promotion.PromotionName);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("[PromotionStatusJob] Auto-expired {Count} promotions", toExpire.Count);

                foreach (var promotion in toExpire)
                {
                    try
                    {
                        await _promotionNotificationService.NotifyPromotionExpiredAsync(promotion);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "[PromotionStatusJob] Failed to send notification for promotion {PromotionId}", promotion.Id);
                    }
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
