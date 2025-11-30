using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Domain.Enums;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.Order.Mappers
{
    public static class OrderMapper
    {
        public static OrderDetailDto MapToDetailDto(Domain.Entities.Order order)
        {
            return new OrderDetailDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                Discount = order.Discount,
                Total = order.Total,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                DeliveryType = order.DeliveryType,
                EstimatedDelivery = CalculateEstimatedDelivery(order.DeliveryType, order.ShippingAddressJson, order.Shop),
                ShippingAddressJson = order.ShippingAddressJson,
                Notes = order.Notes,
                CancelReason = order.CancelReason,
                CreatedAt = order.CreatedAtUtc,
                AcceptedAt = order.AcceptedAt,
                PreparedAt = order.PreparedAt,
                DeliveredAt = order.DeliveredAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt,
                // Standard Delivery fields
                PackagePhotoUrl = order.PackagePhotoUrl,
                TrackingCode = order.TrackingCode,
                DeliveryProofImageUrl = order.DeliveryProofImageUrl,
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.FullName ?? string.Empty,
                BuyerEmail = order.Buyer?.Email,
                BuyerPhone = order.Buyer?.PhoneNumber,
                ShopId = order.ShopId,
                ShopName = order.Shop?.ShopName ?? string.Empty,
                ShopPhone = order.Shop?.User?.PhoneNumber,
                ShopAddress = GetShopAddress(order.Shop),
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    ProductImageUrl = oi.Product?.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }

        public static OrderSummaryDto MapToSummaryDto(Domain.Entities.Order order)
        {
            var firstOrderItem = order.OrderItems?.FirstOrDefault();
            
            return new OrderSummaryDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Total = order.Total,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAtUtc,
                EstimatedDelivery = CalculateEstimatedDelivery(order.DeliveryType, order.ShippingAddressJson, order.Shop),
                ShopId = order.ShopId,
                ShopName = order.Shop?.ShopName,
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.FullName,
                TotalItems = order.OrderItems?.Count ?? 0,
                FirstProduct = firstOrderItem != null ? new FirstProductDto
                {
                    ProductName = firstOrderItem.Product?.Name ?? string.Empty,
                    ProductImageUrl = firstOrderItem.Product?.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                    Quantity = firstOrderItem.Quantity,
                    TotalPrice = firstOrderItem.TotalPrice
                } : null
            };
        }

        private static string GetShopAddress(Domain.Entities.Shop? shop)
        {
            if (shop == null) return string.Empty;

            var addressParts = new List<string>();
            if (!string.IsNullOrEmpty(shop.Street)) addressParts.Add(shop.Street);
            if (!string.IsNullOrEmpty(shop.Ward)) addressParts.Add(shop.Ward);
            if (!string.IsNullOrEmpty(shop.District)) addressParts.Add(shop.District);
            if (!string.IsNullOrEmpty(shop.City)) addressParts.Add(shop.City);

            return string.Join(", ", addressParts);
        }
        
        private static string CalculateEstimatedDelivery(
            DeliveryTypeEnum deliveryType,
            string shippingAddressJson,
            Domain.Entities.Shop? shop)
        {
            // Express delivery: 1-2 giờ
            if (deliveryType == DeliveryTypeEnum.Express)
            {
                return "1-2 giờ";
            }
            
            // Standard delivery: kiểm tra cùng tỉnh hay khác tỉnh
            try
            {
                var shippingAddress = JsonSerializer.Deserialize<ShippingAddressDto>(shippingAddressJson);
                if (shippingAddress != null && shop != null)
                {
                    // So sánh tỉnh/thành phố (normalize để tránh lỗi do khoảng trắng, hoa thường)
                    var buyerProvince = shippingAddress.Province?.Trim().ToLowerInvariant() ?? string.Empty;
                    var shopProvince = shop.City?.Trim().ToLowerInvariant() ?? string.Empty;
                    
                    if (buyerProvince == shopProvince)
                    {
                        return "2-3 ngày"; // Cùng tỉnh
                    }
                    else
                    {
                        return "3-4 ngày"; // Khác tỉnh
                    }
                }
            }
            catch
            {
                // Nếu có lỗi parse JSON, mặc định trả về 3-4 ngày
            }
            
            // Mặc định cho Standard
            return "3-4 ngày";
        }
    }
}
