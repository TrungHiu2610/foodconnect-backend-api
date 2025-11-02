namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.User
{
    public class AddressResponse
    {
        public Guid Id { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FullAddress => $"{Street}, {Ward}, {District}, {City}";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; }
        public string? Note { get; set; }
        public string AddressType { get; set; } = string.Empty;
    }
}
