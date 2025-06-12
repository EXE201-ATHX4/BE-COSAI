using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;
using Core.Base;
using ModelViews.PaymentModelViews;
using Microsoft.Extensions.Caching.Memory;
using ModelViews.OrderModelViews;
using Contract.Services.Interface;
using Contract.Repositories.Entity;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly IMemoryCache _memoryCache;
        private readonly IOrderService _orderService;
        public OrderController(PayOS payOS, IMemoryCache memoryCache, IOrderService orderService)
        {
            _payOS = payOS;
            _memoryCache = memoryCache;
            _orderService = orderService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                List<ItemData> items = new List<ItemData>();
                int totalPrice = 0;
                foreach (var CartItemDTO in body.order.Items)
                {
                    ItemData item = new ItemData(CartItemDTO.ProductName, CartItemDTO.Quantity, CartItemDTO.Quantity * (int)CartItemDTO.Price);
                    totalPrice += CartItemDTO.Quantity * (int)CartItemDTO.Price;
                    items.Add(item);
                }

                PaymentData paymentData = new PaymentData(orderCode, totalPrice, body.description, items, body.cancelUrl, body.returnUrl);

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
                string cacheKey = $"{orderCode}";
                _memoryCache.Set(cacheKey, body.order, TimeSpan.FromMinutes(30));
                KeyTracker.OrderKeys.Add(cacheKey);
                return Ok(new Response(0, "success", createPayment));
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }
        }
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<Order>>> GetAllOrders(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string email = null,
    [FromQuery] string sortBy = null,
    [FromQuery] string sortOrder = "desc",
    [FromQuery] DateTime? orderStartDate = null,
    [FromQuery] DateTime? orderEndDate = null,
    [FromQuery] DateTime? updatedStartDate = null,
    [FromQuery] DateTime? updatedEndDate = null,
    [FromQuery] int? userId = null,
    [FromQuery] DateTime? deletedStartDate = null,
    [FromQuery] DateTime? deletedEndDate = null,
    [FromQuery] string createdBy = null,
    [FromQuery] string updatedBy = null,
    [FromQuery] string deletedBy = null,
    [FromQuery] bool? isActive = null)
        {
            try
            {
                // Validation định dạng ngày
                if (orderStartDate.HasValue && !orderStartDate.Value.ToString("yyyy-MM-dd").Equals(orderStartDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid createdStartDate format. Use yyyy-MM-dd.");
                }
                if (orderEndDate.HasValue && !orderEndDate.Value.ToString("yyyy-MM-dd").Equals(orderEndDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid createdEndDate format. Use yyyy-MM-dd.");
                }
                if (updatedStartDate.HasValue && !updatedStartDate.Value.ToString("yyyy-MM-dd").Equals(updatedStartDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid updatedStartDate format. Use yyyy-MM-dd.");
                }
                if (updatedEndDate.HasValue && !updatedEndDate.Value.ToString("yyyy-MM-dd").Equals(updatedEndDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid updatedEndDate format. Use yyyy-MM-dd.");
                }
                if (deletedStartDate.HasValue && !deletedStartDate.Value.ToString("yyyy-MM-dd").Equals(deletedStartDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid deletedStartDate format. Use yyyy-MM-dd.");
                }
                if (deletedEndDate.HasValue && !deletedEndDate.Value.ToString("yyyy-MM-dd").Equals(deletedEndDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid deletedEndDate format. Use yyyy-MM-dd.");
                }

                // Validation khoảng thời gian
                if (orderStartDate.HasValue && orderEndDate.HasValue && orderStartDate > orderEndDate)
                {
                    return BadRequest("createdStartDate must be less than or equal to createdEndDate.");
                }
                if (updatedStartDate.HasValue && updatedEndDate.HasValue && updatedStartDate > updatedEndDate)
                {
                    return BadRequest("updatedStartDate must be less than or equal to updatedEndDate.");
                }
                if (deletedStartDate.HasValue && deletedEndDate.HasValue && deletedStartDate > deletedEndDate)
                {
                    return BadRequest("deletedStartDate must be less than or equal to deletedEndDate.");
                }

                var orders = await _orderService.GetAllOrders(pageNumber, pageSize, email, sortBy, sortOrder, orderStartDate, orderEndDate, updatedStartDate, updatedEndDate, userId, createdBy, updatedBy, deletedBy, isActive);
                return Ok(BaseResponse<BasePaginatedList<OrderResponse>>.OkResponse(orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving Orders: {ex.Message}");
            }
        }
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderId);
                return Ok(new Response(0, "Ok", paymentLinkInformation));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }

        }
        [HttpPut("{orderId}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderId);
                return Ok(new Response(0, "Ok", paymentLinkInformation));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }

        }
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhook body)
        {
            try
            {
                await _payOS.confirmWebhook(body.webhook_url);
                return Ok(new Response(0, "Ok", null));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }

        }
    }
}
