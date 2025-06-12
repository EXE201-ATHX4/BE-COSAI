using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.CartModelViews
{
    public class CartItemDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1000, long.MaxValue, ErrorMessage = "Price must be greager then 1000 dong")]
        public float Price { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        public int UnitsInStock { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;
    }
}
