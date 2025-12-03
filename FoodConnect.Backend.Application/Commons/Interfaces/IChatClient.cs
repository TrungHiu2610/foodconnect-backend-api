using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageResponse message);
        Task ReceiveTypingIndicator(string userId);
        Task ReceiveStopTypingIndicator(string userId);
        Task ReceiveReadReceipt(string userId, List<string> messageIds);
        Task ReceiveError(string message);
    }
}
