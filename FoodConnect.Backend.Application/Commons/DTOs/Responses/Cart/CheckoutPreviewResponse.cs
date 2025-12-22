namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    public class CheckoutPreviewResponse
    {
        public Guid CartId { get; set; }
        public List<CheckoutShopGroup> ShopGroups { get; set; } = new List<CheckoutShopGroup>();
        public CheckoutSummary Summary { get; set; } = new CheckoutSummary();
    }
    public class CheckoutShopGroup
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ShopStatus { get; set; } = string.Empty;
        public List<OrderPreviewGroup> OrderPreviewGroups { get; set; } = new List<OrderPreviewGroup>();
        public decimal ShopSubtotal { get; set; }
        public decimal ShopTotalShipping { get; set; }
        public decimal ShopGrandTotal { get; set; }
    }
    public class CheckoutSummary
    {
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalOrdersWillBeCreated { get; set; }
        public decimal TotalShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public bool CanCheckout { get; set; }
        public List<string> CheckoutBlockers { get; set; } = new List<string>(); // Reasons why can't checkout
    }
}
