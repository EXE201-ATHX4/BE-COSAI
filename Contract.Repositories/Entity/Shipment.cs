using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Shipment : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ShippingAddressId")]
        public virtual ShippingAddress? ShippingAddress { get; set; }

        [ForeignKey("ShippingMethodId")]
        public virtual ShippingMethod? ShippingMethod { get; set; }

        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public virtual ICollection<ShipmentStatusHistory>? StatusHistories { get; set; }
    }
}
