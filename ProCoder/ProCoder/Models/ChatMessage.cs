using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class ChatMessage
{
    public int ChatMessageId { get; set; }

    public int CoderId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public virtual Coder Coder { get; set; } = null!;
}
