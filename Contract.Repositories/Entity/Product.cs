using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Product : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        //Tên sản phẩm 
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        // Mã sản phẩm
        [Required, MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        // Giá gốc
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        // Giá khuyến mãi
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalePrice { get; set; }

        // % Giảm giá
        //public int? DiscountPercent { get; set; } 

        // Tính % giảm giá tự động (không lưu lên DB, chỉ getter)
        [NotMapped]
        public int DiscountPercent
            => (SalePrice.HasValue && OriginalPrice > 0)
                ? (int)Math.Round((OriginalPrice - SalePrice.Value) / OriginalPrice * 100)
                : 0;
        
        // Trạng thái bán sản phẩm
        public bool IsOnSale { get; set; } = false;
        public int Quantity { get; set; }

        // Mô tả sản phẩm dài (HTML hoặc text thuần)
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        // Mô tả ngắn (HTML hoặc text thuần)
        [Column(TypeName = "nvarchar(max)")]
        public string? ShortDescription { get; set; }

        // Thành phần (ingredients)
        [Column(TypeName = "nvarchar(max)")]
        public string? Ingredients { get; set; }

        // Hướng dẫn sử dụng
        [Column(TypeName = "nvarchar(max)")]
        public string? Usage { get; set; }

        // Thương hiệu
        public virtual Brand? Brand { get; set; }

        // Danh mục sản phẩm
        public virtual Category? Category { get; set; }

        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();


    }
}

