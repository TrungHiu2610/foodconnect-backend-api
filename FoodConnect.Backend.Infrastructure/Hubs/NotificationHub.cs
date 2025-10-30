using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FoodConnect.Backend.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        private static readonly Dictionary<string, HashSet<string>> UserConnections = new();
        private static readonly object Lock = new();

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

                // Add to user's personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                Console.WriteLine($"[SignalR] User {userId} connected. ConnectionId: {Context.ConnectionId}");
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
                
                Console.WriteLine($"[SignalR] User {userId} disconnected. ConnectionId: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client can call this to join specific groups
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task JoinShopGroup(string shopId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");
        }

        public async Task LeaveShopGroup(string shopId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shop_{shopId}");
        }

        // Helper method to get user's connection IDs
        public static List<string> GetUserConnectionIds(string userId)
        {
            lock (Lock)
            {
                return UserConnections.ContainsKey(userId) 
                    ? UserConnections[userId].ToList() 
                    : new List<string>();
            }
        }

        // Helper to check if user is online
        public static bool IsUserOnline(string userId)
        {
            lock (Lock)
            {
                return UserConnections.ContainsKey(userId) && UserConnections[userId].Any();
            }
        }

        // Client can call to test connection
        public async Task Ping()
        {
            await Clients.Caller.ReceiveNotification(new NotificationDto
            {
                Title = "Connection Test",
                Message = "SignalR connection is working!",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
