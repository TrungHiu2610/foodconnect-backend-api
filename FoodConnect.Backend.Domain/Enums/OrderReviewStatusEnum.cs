namespace FoodConnect.Backend.Domain.Enums
{
    /// <summary>
    /// Order review status based on product reviews
    /// </summary>
    public enum OrderReviewStatusEnum
    {
        /// <summary>
        /// At least one product in the order has not been reviewed yet
        /// </summary>
        NotReviewed = 0,
        
        /// <summary>
        /// All products in the order have been reviewed
        /// </summary>
        FullyReviewed = 1
    }
}
