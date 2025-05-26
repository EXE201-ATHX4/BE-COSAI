using Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class OrderDetail : BaseEntity
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public double? Discount { get; set; }
    }
}
