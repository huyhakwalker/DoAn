using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;

namespace ProCoder.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SqlExerciseScoringContext _context;

        public ChatHub(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == user);
            if (coder != null)
            {
                var chatMessage = new ChatMessage
                {
                    CoderId = coder.CoderId,
                    Message = message,
                    SentAt = DateTime.Now
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("ReceiveMessage", user, message, chatMessage.SentAt.ToString("dd/MM/yyyy HH:mm"));
            }
        }
    }
} 