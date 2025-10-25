namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? SessionId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductThumbnail { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductUnit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
    }
}
