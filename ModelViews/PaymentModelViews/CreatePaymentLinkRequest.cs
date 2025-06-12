

using ModelViews.CartModelViews;
using ModelViews.OrderModelViews;
using System.Collections.Generic;

namespace ModelViews.PaymentModelViews
{
    public record CreatePaymentLinkRequest
    (
        TempOrderSession order,
        string description,
        int price,
        string returnUrl,
        string cancelUrl
    );
}
