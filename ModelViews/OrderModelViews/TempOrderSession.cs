using ModelViews.CartModelViews;
using ModelViews.ShipmentModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.OrderModelViews
{
    public class TempOrderSession
    {
        public int OrderCode { get; set; }
        public string CartId { get; set; }
        public ShipmentModel Shipment { get; set; }
        public List<CartItemDTO> Items { get; set; }
    }
}
