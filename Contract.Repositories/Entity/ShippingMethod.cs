using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class ShippingMethod : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? EstimatedDays { get; set; }
        public decimal? Cost { get; set; }
        public string? Description { get; set; }
        public virtual Shipment? Shipment { get; set; }
    }
}
