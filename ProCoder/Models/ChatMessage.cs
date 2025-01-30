using System;

namespace ProCoder.Models
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public int CoderId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }

        // Navigation property
        public virtual Coder Coder { get; set; }
    }
} 