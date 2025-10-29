using FoodConnect.Backend.Application.Features.Order.DTOs;

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
                ShippingAddressJson = order.ShippingAddressJson,
                Notes = order.Notes,
                CancelReason = order.CancelReason,
                RejectionReason = order.RejectionReason,
                CreatedAt = order.CreatedAtUtc,
                AcceptedAt = order.AcceptedAt,
                PreparedAt = order.PreparedAt,
                DeliveredAt = order.DeliveredAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt,
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
            return new OrderSummaryDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Total = order.Total,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAtUtc,
                ShopId = order.ShopId,
                ShopName = order.Shop?.ShopName,
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.FullName,
                TotalItems = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0
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
    }
}
