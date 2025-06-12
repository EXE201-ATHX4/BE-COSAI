using ModelViews.CartModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface ICartService
    {
        Task<CartDTO?> GetCartAsync(string cartId);
        Task<CartDTO?> UpdateCartAsync(CartDTO cart);
        Task<bool> DeleteCartAsync(string cartId);
        Task<int?> GetCartItemsCount(string cartId);
        Task MergeCartsOnLoginAsync(string anonymousId, string userId);
    }
}
