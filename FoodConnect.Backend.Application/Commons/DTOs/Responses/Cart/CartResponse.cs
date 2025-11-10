namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? SessionId { get; set; }
        public List<ShopCartGroup> ShopGroups { get; set; } = new List<ShopCartGroup>();
        public CartSummary Summary { get; set; } = new CartSummary();
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CartSummary
    {
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalOrdersWillBeCreated { get; set; }
        public decimal TotalShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ShopCartGroup
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ShopStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Items grouped by DeliveryType (Express/Standard)
        /// Each group represents one order that will be created
        /// </summary>
        public List<OrderPreviewGroup> OrderPreviewGroups { get; set; } = new List<OrderPreviewGroup>();
        
        public decimal ShopSubtotal { get; set; }
    }

    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductThumbnail { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
    }
}
