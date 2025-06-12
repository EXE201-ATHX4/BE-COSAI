using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.OrderModelViews;


namespace Contract.Services.Interface
{
    public interface IOrderService
    {
        Task CreateOrderAfterPaymentAsync(int orderCode);
        Task<BasePaginatedList<OrderResponse>> GetAllOrders(int pageNumber, int pageSize, string email = null, string sortBy = null, string sortOrder = "asc", DateTime? orderStartDate = null, DateTime? orderEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, int? userId = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
    }
}
