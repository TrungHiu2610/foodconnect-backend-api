namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

public class StartChatResponse
{
    public Guid ConversationId { get; set; }
    public MessageResponse? SystemMessage { get; set; }
    public bool IsNewConversation { get; set; }
}
