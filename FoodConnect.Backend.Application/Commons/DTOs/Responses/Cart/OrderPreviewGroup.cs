namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    /// <summary>
    /// Represents a group of cart items that will become one order
    /// Grouped by Shop + DeliveryType
    /// </summary>
    public class OrderPreviewGroup
    {
        /// <summary>
        /// "Express" or "Standard"
        /// </summary>
        public string DeliveryType { get; set; } = string.Empty;

        /// <summary>
        /// Cart items in this delivery type group
        /// </summary>
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();

        /// <summary>
        /// Estimated shipping fee for this group
        /// </summary>
        public decimal EstimatedShippingFee { get; set; }

        /// <summary>
        /// Subtotal of items + shipping fee
        /// </summary>
        public decimal GroupTotal { get; set; }

        /// <summary>
        /// Can this group be checked out (e.g., Express within 20km)
        /// </summary>
        public bool CanCheckout { get; set; } = true;

        /// <summary>
        /// Warning messages if any (e.g., "Express delivery unavailable - shop is 25km away")
        /// </summary>
        public List<string> CheckoutWarnings { get; set; } = new List<string>();

        /// <summary>
        /// Distance to shop in km (for Express delivery validation)
        /// </summary>
        public double? DistanceToShopKm { get; set; }
    }
}
