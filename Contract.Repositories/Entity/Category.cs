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
    public class Category : BaseEntity
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }


        [StringLength(500)]
        public string Description { get; set; }

        public int? ParentId { get; set; }

        public int Level { get; set; } // 0: Root, 1: Main Category, 2: Sub Category

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool AllowProducts { get; set; } = true; // Cho phép chứa products

        public bool ShowInMenu { get; set; } = true; // Hiển thị trong menu

        // Navigation Properties
        public virtual ICollection<Category>? SubCategory { get; set; } = new List<Category>();

        // Product Relationship
        public virtual ICollection<Product>? Products { get; set; } = new List<Product>();

    }
    
}
