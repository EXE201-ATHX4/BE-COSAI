using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Review : BaseEntity
    {
        public int Id { get; set; }
        public string? ReviewContent { get; set; }
        public int? StarRating { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Products { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }
    }
}
