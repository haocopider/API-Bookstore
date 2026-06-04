using Bookstore.Shared.Dtos;
namespace Bookstore.Shared.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto request);
        Task<IEnumerable<OrderDto>> GetOrderHistoryAsync(int userId, int? status = null);
        Task<OrderDto> GetOrderDetailAsync(int userId, int orderId);
        Task<bool> CancelOrderAsync(int userId, int orderId, string cancelReason);
        Task<bool> CompleteOrderAsync(int userId, int orderId);

        Task<IEnumerable<OrderDto>> GetAllOrdersForAdminAsync(int? status = null);
        Task<bool> UpdateOrderStatusByAdminAsync(int orderId, int newStatus);
    }
}
