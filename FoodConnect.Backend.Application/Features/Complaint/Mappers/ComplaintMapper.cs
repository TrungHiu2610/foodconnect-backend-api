using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Complaint.Mappers
{
    public static class ComplaintMapper
    {
        public static ComplaintDetailDto MapToDetailDto(OrderComplaint complaint)
        {
            return new ComplaintDetailDto
            {
                Id = complaint.Id,
                Status = complaint.Status,
                StatusName = complaint.Status.ToString(),
                
                BuyerReason = complaint.BuyerReason,
                BuyerEvidences = complaint.Assets
                    .Where(a => a.AssetType == OrderComplaintAssetTypeEnum.BuyerEvidence)
                    .Select(MapToAssetDto)
                    .ToList(),
                
                SellerResponse = complaint.SellerResponse,
                SellerRespondedAt = complaint.SellerRespondedAt,
                SellerDesiredRefundPercentage = complaint.SellerDesiredRefundPercentage,
                SellerDesiredRefundAmount = complaint.SellerDesiredRefundAmount,
                IsSellerRefundFixedAmount = complaint.IsSellerRefundFixedAmount,
                SellerEvidences = complaint.Assets
                    .Where(a => a.AssetType == OrderComplaintAssetTypeEnum.SellerEvidence)
                    .Select(MapToAssetDto)
                    .ToList(),
                
                AdminDecisionReason = complaint.AdminDecisionReason,
                AdminDecidedAt = complaint.AdminDecidedAt,
                ApprovedRefundAmount = complaint.ApprovedRefundAmount,
                IsApproved = complaint.IsApproved,
                
                CreatedAt = complaint.CreatedAtUtc,
                CompletedAt = complaint.CompletedAt,
                
                OrderId = complaint.OrderId,
                OrderCode = complaint.Order.OrderCode,
                OrderTotal = complaint.Order.Total,
                
                BuyerId = complaint.BuyerId,
                BuyerName = complaint.Buyer.FullName,
                
                SellerId = complaint.SellerId,
                SellerName = complaint.Seller.FullName,
                ShopName = complaint.Order.Shop?.ShopName ?? "",
                
                AdminId = complaint.AdminId,
                AdminName = complaint.Admin?.FullName
            };
        }

        public static ComplaintSummaryDto MapToSummaryDto(OrderComplaint complaint)
        {
            return new ComplaintSummaryDto
            {
                Id = complaint.Id,
                Status = complaint.Status,
                StatusName = complaint.Status.ToString(),
                
                BuyerReason = complaint.BuyerReason.Length > 100 
                    ? complaint.BuyerReason.Substring(0, 100) + "..." 
                    : complaint.BuyerReason,
                HasSellerResponse = !string.IsNullOrEmpty(complaint.SellerResponse),
                
                CreatedAt = complaint.CreatedAtUtc,
                SellerRespondedAt = complaint.SellerRespondedAt,
                AdminDecidedAt = complaint.AdminDecidedAt,
                
                OrderId = complaint.OrderId,
                OrderCode = complaint.Order.OrderCode,
                OrderTotal = complaint.Order.Total,
                
                ShopName = complaint.Order.Shop?.ShopName,
                BuyerName = complaint.Buyer?.FullName
            };
        }

        public static ComplaintAssetDto MapToAssetDto(OrderComplaintAsset asset)
        {
            return new ComplaintAssetDto
            {
                Id = asset.Id,
                AssetUrl = asset.AssetUrl,
                AssetType = asset.AssetType,
                AssetTypeName = asset.AssetType.ToString(),
                CreatedAt = asset.CreatedAtUtc
            };
        }
    }
}
