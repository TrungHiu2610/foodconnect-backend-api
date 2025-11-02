using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public bool IsDefault { get; set; } = false;
        public string? Note { get; set; }
        public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Home;
    }
}
