namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopOperatingHour : BaseEntity
    {
        public DayOfWeek DayOfWeek { get; set; } 
        public TimeOnly OpenTime { get; set; }   
        public TimeOnly CloseTime { get; set; }

        public Guid? ShopRegistrationId { get; set; }
        public Guid? ShopId { get; set; }
        
        public virtual ShopRegistration? ShopRegistration { get; set; }
        public virtual Shop? Shop { get; set; }
    }
}
