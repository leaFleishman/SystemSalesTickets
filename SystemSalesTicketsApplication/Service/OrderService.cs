using AutoMapper;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class OrderService : IOrderService
    {
        private static int counter = 1;

        private readonly IOrderRepository _orderRepository;

        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDTO> AddOrder(OrderDTO order)
        {
            var tmp = _mapper.Map<Order>(order);
            tmp.OrderId = counter++;
            var result = await _orderRepository.Add(tmp);
            return _mapper.Map<OrderDTO>(result);
        }



        public async Task<IEnumerable<OrderDTO>> GetAllOrders()
        {
            var tmp = await _orderRepository.GetAll();
            return _mapper.Map<IEnumerable<OrderDTO>>(tmp);
        }

        public async Task<OrderDTO> GetOrderById(int id)
        {
            var tmp = await _orderRepository.GetById(id);
            return _mapper.Map<OrderDTO>(tmp);
        }


    }
}

