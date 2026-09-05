using AutoMapper;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class OrderService : IOrderService
    {
        private static int counter = new Random().Next();
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;

        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper,ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderLogDTO> AddOrder(OrderDTO order)
        {
            var tmp = _mapper.Map<Order>(order);
            tmp.OrderId = counter++;
            var result = await _orderRepository.Add(tmp);
            return _mapper.Map<OrderLogDTO>(result);
        }



        public async Task<IEnumerable<OrderDTO>> GetAllOrders()
        {
            var tmp = await _orderRepository.GetAll();
            return _mapper.Map<IEnumerable<OrderDTO>>(tmp);
        }

        public async Task<OrderLogDTO> GetOrderById(int id)
        {
            var tmp = await _orderRepository.GetById(id);
            return _mapper.Map<OrderLogDTO>(tmp);
        }


    }
}

