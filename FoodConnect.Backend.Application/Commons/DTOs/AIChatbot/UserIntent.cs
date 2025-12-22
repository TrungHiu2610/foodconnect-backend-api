namespace FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;

public class UserIntent
{
    public List<string> Categories { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> DietaryRestrictions { get; set; } = new();
    public string? NutritionFocus { get; set; }
    public string? PriceRange { get; set; }
    public string? MealType { get; set; }
    public string? CaloriesConstraint { get; set; }
    public List<string> Allergens { get; set; } = new();
    public List<string> SpecialRequests { get; set; } = new();
    public string? Location { get; set; }
}
