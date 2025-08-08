using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string MessageResponse { get; set; } 
        public DateTime Timestamp { get; set; }
    }
}
