using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AgroConnect.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(message))
            {
                // Save to DB
                var chatMessage = new ChatMessage
                {
                    ApplicationUserId = userId,
                    Content = message,
                    SentAt = DateTime.Now
                };
                
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                // Broadcast to all connected clients
                await Clients.All.SendAsync("ReceiveMessage", user, message, chatMessage.SentAt.ToString("HH:mm"));
            }
        }
    }
}
