using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Conversation
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
