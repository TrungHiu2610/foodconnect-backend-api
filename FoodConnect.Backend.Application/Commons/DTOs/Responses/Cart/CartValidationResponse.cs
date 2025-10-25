namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart
{
    public class CartValidationResponse
    {
        public bool IsValid { get; set; }
        public List<CartValidationError> Errors { get; set; } = new List<CartValidationError>();
        public List<CartValidationWarning> Warnings { get; set; } = new List<CartValidationWarning>();
    }

    public class CartValidationError
    {
        public Guid CartItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty; // "OutOfStock", "ProductInactive", "ShopClosed", "PriceChanged"
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; } // Additional info (e.g., new price, available stock)
    }

    public class CartValidationWarning
    {
        public Guid CartItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string WarningType { get; set; } = string.Empty; // "LowStock", "ShopSuspended"
        public string Message { get; set; } = string.Empty;
    }
}
