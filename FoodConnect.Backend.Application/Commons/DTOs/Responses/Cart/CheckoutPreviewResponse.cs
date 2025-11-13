namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    /// <summary>
    /// Checkout preview response - shows how cart will be split into orders
    /// </summary>
    public class CheckoutPreviewResponse
    {
        public Guid CartId { get; set; }
        public List<CheckoutShopGroup> ShopGroups { get; set; } = new List<CheckoutShopGroup>();
        public CheckoutSummary Summary { get; set; } = new CheckoutSummary();
    }

    /// <summary>
    /// Shop group in checkout - contains delivery type groups with shipping fees
    /// </summary>
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

    /// <summary>
    /// Checkout summary - overall order breakdown
    /// </summary>
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
