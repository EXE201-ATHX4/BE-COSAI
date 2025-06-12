using Contract.Services.Interface;
using Core.Utils;
using Microsoft.AspNetCore.Mvc;
using ModelViews.CartModelViews;
using Services.Service;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {

        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        private string GetCartId()
        {
            var id = CartHelper.GetCartId(HttpContext);
            if (id == null)
            {
                id = CartHelper.EnsureGuestCartId(HttpContext); 
            }
            return id;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cartId = GetCartId();
            var cart = await _cartService.GetCartAsync(cartId!);
            int totalPrice = 0;
            foreach (var item in cart?.Items ?? new List<CartItemDTO>())
            {
                totalPrice += (int)item.Price * item.Quantity;
            }
            return Ok(cart ?? new CartDTO { Id = cartId!, TotalPrice = totalPrice, Items = new List<CartItemDTO>() });
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDTO item)
        {
            var cartId = GetCartId()!;
            var cart = await _cartService.GetCartAsync(cartId);

            if (cart == null)
            {
                cart = new CartDTO
                {
                    Id = cartId,
                    TotalPrice = (int)item.Price * item.Quantity,
                    Items = new List<CartItemDTO> { item }
                };
            }
            else
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    // Nếu sản phẩm đã có trong giỏ, tăng số lượng
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    cart.Items.Add(item);
                }
            }

            var updated = await _cartService.UpdateCartAsync(cart);
            return Ok(updated);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var cartId = GetCartId()!;
            var cart = await _cartService.GetCartAsync(cartId);
            if (cart == null) return NotFound();

            cart.Items.RemoveAll(x => x.Id == productId);
            var updated = await _cartService.UpdateCartAsync(cart);
            return Ok(updated);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetItemCount()
        {
            var cartId = GetCartId();
            var count = await _cartService.GetCartItemsCount(cartId!);
            return Ok(count ?? 0);
        }
    }
}
