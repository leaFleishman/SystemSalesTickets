using AutoMapper;
using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository,IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<Order> AddOrder(Order order)
        {
            return await _orderRepository.Add(order);
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orderRepository.GetAll();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _orderRepository.GetById(id);
        }
    }
}

