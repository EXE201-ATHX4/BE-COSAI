using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;
        public string Role { get; set; } = null!; // "user" hoặc "model"
        public string Content { get; set; } = null!;
        public DateTimeOffset Timestamp { get; set; } = DateTime.UtcNow;
    }
}
