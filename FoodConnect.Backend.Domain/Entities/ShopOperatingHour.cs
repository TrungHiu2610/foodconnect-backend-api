using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopOperatingHour : BaseEntity, IHardDelete
    {
        public DayOfWeek DayOfWeek { get; set; } 
        public TimeOnly OpenTime { get; set; }   
        public TimeOnly CloseTime { get; set; }

        public Guid? ShopId { get; set; }
        public virtual Shop? Shop { get; set; }
    }
}
