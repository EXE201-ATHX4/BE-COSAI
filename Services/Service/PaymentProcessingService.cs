using Contract.Repositories.Entity;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelViews.OrderModelViews;
using Net.payOS;
using Repositories.Base;
using System.Runtime.ConstrainedExecution;

namespace Services.Service
{
    public class PaymentProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly PayOS _payOS;
        public PaymentProcessingService(IServiceProvider serviceProvider, IMemoryCache memoryCache, PayOS payOS)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _payOS = payOS;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // This method will be called when the service starts
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckPendingOrders(stoppingToken);
                await Task.Delay(10000, stoppingToken); // Simulate work
            }
        }
        private async Task CheckPendingOrders(CancellationToken token)
        {
            var keys = GetAllCacheKeys(); // You need to track keys
            foreach (var key in keys)
            {
                if (_memoryCache.TryGetValue(key, out TempOrderSession session))
                {
                    try
                    {
                        var info = await _payOS.getPaymentLinkInformation(long.Parse(key));
                        if (info.status == "PAID")
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<ComesticsSalesDBContext>();
                            var shippingMethod = await db.ShippingMethods.FirstOrDefaultAsync(m => m.Name == session.Shipment.ShippingMethod);
                            var shipmentInfo = session.Shipment;
                            var shippingAddress = new ShippingAddress
                            {
                                FullName = shipmentInfo.FullName,
                                Phone = shipmentInfo.PhoneNumber,
                                Street = shipmentInfo.Address,
                                City = shipmentInfo.ProvinceCity,
                                Ward = shipmentInfo.Ward,
                                District = shipmentInfo.District,
                                PostalCode = "", // Nếu có
                                IsDefault = false,
                                CreatedTime = DateTime.UtcNow
                            };
                            var shipment = new Shipment
                            {
                                ShippingAddress = shippingAddress,
                                ShippingMethod = shippingMethod,
                                Status = "Pending",
                                CreatedTime = DateTime.UtcNow
                            };
                            // Save order
                            var orderDetails = new List<OrderDetail>();
                            foreach (var item in session.Items)
                            {
                                var product = await db.Products.FindAsync(item.Id);
                                if (product == null || product.Quantity < item.Quantity)
                                {
                                    _memoryCache.Remove(key);
                                    throw new Exception($"Sản phẩm với ID {item.Id} không đủ số lượng hoặc không tồn tại.");
                                }

                                // Trừ số lượng
                                product.Quantity -= item.Quantity;

                                // Tạo chi tiết đơn hàng
                                orderDetails.Add(new OrderDetail
                                {
                                    ProductId = item.Id,
                                    Quantity = item.Quantity,
                                    Price = (int)item.Price
                                });
                            }

                            // Sau đó gán vào Order
                            var order = new Order
                            {
                                OrderDate = CoreHelper.SystemTimeNow.DateTime,
                                CreatedTime = DateTime.UtcNow,
                                TotalPrice = info.amount,
                                Status = info.status,
                                OrderDetails = orderDetails,
                                Shipment = shipment,
                            };

                            db.Orders.Add(order);
                            await db.SaveChangesAsync(token);

                            _memoryCache.Remove(key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process order {key}: {ex.Message}");
                    }
                }
            }
        }
        private List<string> GetAllCacheKeys()
        {
            // Best practice: store keys in a List<string> in memory
            return KeyTracker.OrderKeys.ToList();
        }

    }

}
