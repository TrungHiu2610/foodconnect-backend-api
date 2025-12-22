using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FoodConnect.Backend.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IChatNotificationService _chatNotificationService;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly Dictionary<string, HashSet<string>> UserConnections = new();
        private static readonly object Lock = new();

        public ChatHub(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IChatNotificationService chatNotificationService,
            IUnitOfWork unitOfWork)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _chatNotificationService = chatNotificationService;
            _unitOfWork = unitOfWork;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                lock (Lock)
                {
                    if (!UserConnections.ContainsKey(userId))
                    {
                        UserConnections[userId] = new HashSet<string>();
                    }
                    UserConnections[userId].Add(Context.ConnectionId);
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                Console.WriteLine($"[ChatHub] User {userId} connected. ConnectionId: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                lock (Lock)
                {
                    if (UserConnections.ContainsKey(userId))
                    {
                        UserConnections[userId].Remove(Context.ConnectionId);
                        if (UserConnections[userId].Count == 0)
                        {
                            UserConnections.Remove(userId);
                        }
                    }
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                Console.WriteLine($"[ChatHub] User {userId} disconnected. ConnectionId: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a conversation room - validates user belongs to conversation
        /// </summary>
        public async Task JoinConversation(string conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.ReceiveError("Unauthorized");
                return;
            }

            var conversation = await _conversationRepository.GetByIdAsync(Guid.Parse(conversationId));
            if (conversation == null)
            {
                await Clients.Caller.ReceiveError("Conversation not found");
                return;
            }

            if (conversation.BuyerId.ToString() != userId && conversation.SellerId.ToString() != userId)
            {
                await Clients.Caller.ReceiveError("You don't belong to this conversation");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            Console.WriteLine($"[ChatHub] User {userId} joined conversation {conversationId}");
        }

        /// <summary>
        /// Leave a conversation room
        /// </summary>
        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[ChatHub] User {userId} left conversation {conversationId}");
        }

        /// <summary>
        /// Notify other user that someone is typing
        /// </summary>
        public async Task SendTypingIndicator(string conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.OthersInGroup($"conversation_{conversationId}")
                    .ReceiveTypingIndicator(userId);
            }
        }

        /// <summary>
        /// Notify other user that typing stopped
        /// </summary>
        public async Task SendStopTypingIndicator(string conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.OthersInGroup($"conversation_{conversationId}")
                    .ReceiveStopTypingIndicator(userId);
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        public async Task MarkMessagesAsRead(string conversationId, List<string> messageIds)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                // Update messages in database
                var guidMessageIds = messageIds.Select(Guid.Parse).ToList();
                var messages = await _messageRepository.GetByIdsAsync(guidMessageIds);
                
                foreach (var message in messages)
                {
                    if (message.SenderId.ToString() != userId) // Only mark messages you received
                    {
                        message.IsRead = true;
                        _messageRepository.Update(message);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();

                // Notify sender about read receipt
                await Clients.OthersInGroup($"conversation_{conversationId}")
                    .ReceiveReadReceipt(userId, messageIds);

                // Update unread count for current user
                var userGuid = Guid.Parse(userId);
                var unreadCount = await _messageRepository.GetUnreadCountByUserAsync(userGuid);
                await _chatNotificationService.UpdateUnreadCountAsync(userGuid, unreadCount);

                Console.WriteLine($"[ChatHub] User {userId} marked {messages.Count()} messages as read");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] Error marking messages as read: {ex.Message}");
                await Clients.Caller.ReceiveError($"Failed to mark messages as read: {ex.Message}");
            }
        }

        public static List<string> GetUserConnectionIds(string userId)
        {
            lock (Lock)
            {
                return UserConnections.ContainsKey(userId) 
                    ? UserConnections[userId].ToList() 
                    : new List<string>();
            }
        }

        public static bool IsUserOnline(string userId)
        {
            lock (Lock)
            {
                return UserConnections.ContainsKey(userId) && UserConnections[userId].Any();
            }
        }

        public async Task Ping()
        {
            await Clients.Caller.ReceiveError("Pong!");
        }
    }
}
