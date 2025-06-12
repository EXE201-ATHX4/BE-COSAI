using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.CartModelViews
{
    public class CartDTO
    {
        public CartDTO() { }

        public CartDTO(string id)
        {
            Id = id;
            Items = [];
        }

        [Required]
        public required string Id { get; set; }
        public int TotalPrice { get; set; }

        public List<CartItemDTO> Items { get; set; } = [];
    }
}
