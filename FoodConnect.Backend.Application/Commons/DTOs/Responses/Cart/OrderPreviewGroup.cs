namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    public class OrderPreviewGroup
    {
        public string DeliveryType { get; set; } = string.Empty;
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        public decimal EstimatedShippingFee { get; set; }
        public decimal GroupTotal { get; set; }
        public bool CanCheckout { get; set; } = true;
        public List<string> CheckoutWarnings { get; set; } = new List<string>();
        public double? DistanceToShopKm { get; set; }
    }
}
