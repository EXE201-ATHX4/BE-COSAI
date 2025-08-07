using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class ShippingAddress : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public bool? IsDefault { get; set; }
        public virtual ICollection<Shipment>? Shipment { get; set; }
    }
}
