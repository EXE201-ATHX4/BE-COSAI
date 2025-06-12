using ModelViews.OrderModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.OrderDetailModel
{
    public class OrderdetailResponse
    {
        public ProductResponse Product { get; set; }
        public int? Quantity { get; set; }
        public int? Price { get; set; }
    }
}
