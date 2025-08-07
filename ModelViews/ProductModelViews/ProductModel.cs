using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.ProductModelViews
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int Quantity { get; set; }
        public int DiscountPercent
            => (SalePrice.HasValue && OriginalPrice > 0)
                ? (int)Math.Round((OriginalPrice - SalePrice.Value) / OriginalPrice * 100)
                : 0;

        // Trạng thái bán sản phẩm
        public bool IsOnSale { get; set; } = false;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public List<string> ProductImages { get; set; } = new();
    }
    public class CreateProductModel
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int Quantity { get; set; }
        // Trạng thái bán sản phẩm
        public bool IsOnSale { get; set; } = false;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public List<string>? ImageUrls { get; set; } = new List<string>();
    }
    public class UpdateProductModel 
    {
        public string? Name { get; set; } = string.Empty;
        public string? SKU { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? SalePrice { get; set; }

        // Trạng thái bán sản phẩm
        public bool? IsOnSale { get; set; } = false;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public int SupplierId { get; set; }

    }

}
