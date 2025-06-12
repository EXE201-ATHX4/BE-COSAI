using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Base;

namespace Contract.Repositories.Entity
{
    public class Order : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string? ShipName { get; set; }
        public int? TotalPrice { get; set; }


        [ForeignKey("CustomerId")]
        public virtual User? User { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        public virtual Shipment? Shipment { get; set; }
    }
}
