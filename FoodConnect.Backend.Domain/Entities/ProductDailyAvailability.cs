

namespace FoodConnect.Backend.Domain.Entities
{
    public class ProductDailyAvailability:BaseEntity
    {
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
