using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsPresentation.Interfaces
{
    public interface IOrderService
    {
        Task<Order> AddOrder(Order order);
        Task<IEnumerable<Order>> GetAllOrders();
        Task<Order?> GetOrderById(int id);

    }
}

