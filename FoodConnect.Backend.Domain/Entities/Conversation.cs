namespace FoodConnect.Backend.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    
    public virtual User Buyer { get; set; } = null!;
    public virtual User Seller { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
