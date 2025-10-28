using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopOperatingHour : BaseEntity, IHardDelete
    {
        public DayOfWeek DayOfWeek { get; set; } 
        public TimeOnly OpenTime { get; set; }   
        public TimeOnly CloseTime { get; set; }

        public Guid ShopId { get; set; }  // Required - OperatingHour phải thuộc về Shop
        public virtual Shop? Shop { get; set; }
    }
}
