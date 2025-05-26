using Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Supplier : BaseEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }

        // Navigation property
        public virtual ICollection<Product>? Products { get; set; } = new List<Product>();
    }
}
