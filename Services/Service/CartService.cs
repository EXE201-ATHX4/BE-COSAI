using Contract.Services.Interface;
using ModelViews.CartModelViews;
using StackExchange.Redis;
using System.Text.Json;

namespace Services.Service
{
    public class CartService : ICartService
    {
        private readonly IDatabase _database;

        public CartService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<CartDTO?> GetCartAsync(string cartId)
        {
            var cart = await _database.StringGetAsync(cartId);
            if (string.IsNullOrEmpty(cart))
            {
                return null;
            }
            var result = JsonSerializer.Deserialize<CartDTO?>(cart.ToString());
            return result;
        }

        public async Task<CartDTO?> UpdateCartAsync(CartDTO cart)
        {
            var updatedCart = await _database.StringSetAsync(
                cart.Id,
                JsonSerializer.Serialize(cart),
                TimeSpan.FromDays(30)
            );
            if (!updatedCart)
                return null;
            return await GetCartAsync(cart.Id);
        }

        public Task<bool> DeleteCartAsync(string cartId)
        {
            return _database.KeyDeleteAsync(cartId);
        }

        public async Task<int?> GetCartItemsCount(string cartId)
        {
            var cartDto = await GetCartAsync(cartId);
            if (cartDto == null)
            {
                return null;
            }
            return cartDto.Items.Count();
        }
        public async Task MergeCartsOnLoginAsync(string anonymousId, string userId)
        {
            var anonCart = await GetCartAsync(anonymousId);
            var userCart = await GetCartAsync(userId);

            if (anonCart == null) return;

            if (userCart == null)
            {
                anonCart.Id = userId;
                await UpdateCartAsync(anonCart);
            }
            else
            {
                foreach (var item in anonCart.Items)
                {
                    var existing = userCart.Items.FirstOrDefault(x => x.Id == item.Id);
                    if (existing != null)
                        existing.Quantity += item.Quantity;
                    else
                        userCart.Items.Add(item);
                }
                await UpdateCartAsync(userCart);
            }

            await DeleteCartAsync(anonymousId);
        }

    }
}
