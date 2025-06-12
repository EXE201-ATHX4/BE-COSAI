using ModelViews.OrderDetailModel;
using ModelViews.ShippingAddressModel;

namespace ModelViews.OrderModelViews
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public int? TotalPrice { get; set; }
        public List<OrderdetailResponse> OrderDetails { get; set; }
        public ShippingAddressResponse ShippingAddress { get; set; }
        public string ShipmentMethod { get; set; }
    }
}
