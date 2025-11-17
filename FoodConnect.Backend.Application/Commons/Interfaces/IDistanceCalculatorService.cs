namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IDistanceCalculatorService
    {
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }
}
