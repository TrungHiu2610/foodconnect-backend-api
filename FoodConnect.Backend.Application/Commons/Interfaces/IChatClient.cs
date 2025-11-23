using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    /// <summary>
    /// Client-side methods that ChatHub can invoke
    /// </summary>
    public interface IChatClient
    {
        /// <summary>
        /// Receive a new message in real-time
        /// </summary>
        Task ReceiveMessage(MessageResponse message);

        /// <summary>
        /// Receive typing indicator from other user
        /// </summary>
        Task ReceiveTypingIndicator(string userId);

        /// <summary>
        /// Receive stop typing indicator
        /// </summary>
        Task ReceiveStopTypingIndicator(string userId);

        /// <summary>
        /// Receive read receipt when other user reads messages
        /// </summary>
        Task ReceiveReadReceipt(string userId, List<string> messageIds);

        /// <summary>
        /// Receive error messages
        /// </summary>
        Task ReceiveError(string message);
    }
}
