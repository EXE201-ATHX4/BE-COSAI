
using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Microsoft.Extensions.Logging;
using ModelViews.OrderModelViews;
using ModelViews.OrderDetailModel;
using ModelViews.ShippingAddressModel;
namespace Services.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayOS _payOS;
        private readonly CartService _cartSessionRepo;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IUnitOfWork unitOfWork, PayOS payOS, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _payOS = payOS;
            _logger = logger;
        }
        public async Task CreateOrderAfterPaymentAsync(int orderCode)
        {
            var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

            // 1. Load cart/session data associated with this orderCode
            var cartItems = await _cartSessionRepo.GetCartAsync(orderCode.ToString()); // e.g. from Redis or SQL

            // 2. Create Order
            var newOrder = new Order
            {

                CreatedTime = DateTime.UtcNow,
                TotalPrice = paymentInfo.amount,
                Status = paymentInfo.status,
                OrderDetails = cartItems.Items.Select(i => new OrderDetail
                {
                    ProductId = i.Id,
                    Quantity = i.Quantity,
                    Price = (int)i.Price
                }).ToList()
            };

            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            await _unitOfWork.SaveAsync();
        }
        public async Task<BasePaginatedList<OrderResponse>> GetAllOrders(int pageNumber, int pageSize, string email = null, string sortBy = null, string sortOrder = "asc", DateTime? orderStartDate = null, DateTime? orderEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, int? userId = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all BandBrands with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, Name: {Name}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, UserId: {userId}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}", pageNumber, pageSize, email, sortBy, sortOrder, orderStartDate, orderEndDate, updatedStartDate, updatedEndDate, userId, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var ordersQuery = _unitOfWork.GetRepository<Order>()
                    .Entities.Include(o => o.User).Include(o => o.OrderDetails).Include(o => o.Shipment).Include(o => o.Shipment.ShippingMethod)
                    .AsQueryable();

                // Áp dụng bộ lọc
                if (!string.IsNullOrWhiteSpace(email))
                {
                    ordersQuery = ordersQuery.Where(b => b.User.Email.Contains(email));
                }
                if (orderStartDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => b.OrderDate >= orderStartDate.Value.Date);
                }
                if (orderEndDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => b.OrderDate <= orderEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => b.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => b.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    ordersQuery = ordersQuery.Where(b => b.CreatedBy != null && b.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    ordersQuery = ordersQuery.Where(b => b.LastUpdatedBy != null && b.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    ordersQuery = ordersQuery.Where(b => b.DeletedBy != null && b.DeletedBy.Contains(deletedBy));
                }
                if (isActive.HasValue)
                {
                    ordersQuery = ordersQuery.Where(b => (b.DeletedTime.HasValue == !isActive.Value));
                }

                // Áp dụng sắp xếp
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "createdtime":
                            ordersQuery = sortOrder.ToLower() == "desc"
                                ? ordersQuery.OrderByDescending(b => b.CreatedTime)
                                : ordersQuery.OrderBy(b => b.CreatedTime);
                            break;
                        case "lastupdatedtime":
                            ordersQuery = sortOrder.ToLower() == "desc"
                                ? ordersQuery.OrderByDescending(b => b.LastUpdatedTime)
                                : ordersQuery.OrderBy(b => b.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            ordersQuery = sortOrder.ToLower() == "desc"
                                ? ordersQuery.OrderByDescending(b => b.DeletedTime ?? DateTimeOffset.MinValue)
                                : ordersQuery.OrderBy(b => b.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        default:
                            ordersQuery = ordersQuery.OrderByDescending(b => b.CreatedTime); // Mặc định
                            break;
                    }
                }
                else
                {
                    ordersQuery = ordersQuery.OrderByDescending(b => b.CreatedTime); // Mặc định
                }

                int totalCount = await ordersQuery.CountAsync();
                List<OrderResponse> orderResponses = await ordersQuery
                    .Select(o => new OrderResponse
                    {
                        Id = o.Id,
                        OrderDate = o.OrderDate,
                        ShipDate = o.ShipDate,
                        PaymentMethod = o.PaymentMethod,
                        Status = o.Status,
                        TotalPrice = o.TotalPrice,
                        OrderDetails = o.OrderDetails.Select(od => new OrderdetailResponse
                        {
                            Product = new ProductResponse
                            {
                                Id = od.Product.Id,
                                Name = od.Product.Name,
                            },
                            Quantity = od.Quantity,
                            Price = od.Price
                        }).ToList(),
                        ShippingAddress = new ShippingAddressResponse
                        {
                            FullName = o.Shipment.ShippingAddress.FullName,
                            Phone = o.Shipment.ShippingAddress.Phone,
                            Street = o.Shipment.ShippingAddress.Street,
                            Ward = o.Shipment.ShippingAddress.Ward,
                            District = o.Shipment.ShippingAddress.District,
                            City = o.Shipment.ShippingAddress.City,
                        },
                        ShipmentMethod = o.Shipment.ShippingMethod.Name 
                    })
                    .ToListAsync();
                var orders =  orderResponses
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize).ToList();

                return new BasePaginatedList<OrderResponse>(orders, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Order: {Message}", ex.Message);
                throw;
            }
        }
    }
}
