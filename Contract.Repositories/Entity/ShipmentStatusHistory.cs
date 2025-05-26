using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class ShipmentStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ShipmentId")]
        public virtual Shipment? Shipment { get; set; }

        public string? Status { get; set; }
        public string? Note { get; set; }
    }
}
