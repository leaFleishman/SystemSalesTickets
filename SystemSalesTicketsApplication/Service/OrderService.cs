using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using MyApp.Application.Common.Models;

namespace SystemSalesTickets.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly ISeatRepository _seatRepository;
        //private static int counter = new Random().Next();
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;

        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper, ILogger<OrderService> logger, ISeatRepository seatRepository)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
            _seatRepository = seatRepository;
        }

        public async Task<OrderLogDTO> AddOrder(
     OrderDTO order,
     CancellationToken cancellationToken = default)
        {
            var seat = await _seatRepository.GetById(
                order.SeatId,
                cancellationToken);

            if (seat == null)
            {
                return new OrderLogDTO
                {
                    Message = "Seat not found"
                };
            }

            if (await _orderRepository.ExistsForEventAndSeat(
                    order.EventId,
                    order.SeatId,
                    cancellationToken))
            {
                return new OrderLogDTO
                {
                    Message = "Seat is already occupied"
                };
            }

            var newOrder = _mapper.Map<Order>(order);

            try
            {
                await _orderRepository.Add(
                    newOrder,
                    cancellationToken);

                // Save אחד בלבד
                await _orderRepository.Save(cancellationToken);

                return _mapper.Map<OrderLogDTO>(newOrder);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new OrderLogDTO
                {
                    Message =
                        "Seat was just booked by someone else, please try again"
                };
            }
            catch (DbUpdateException)
            {
                return new OrderLogDTO
                {
                    Message = "Seat is already occupied"
                };
            }
        }


        public async Task<PagedResponse<OrderDTO>> GetAllOrders(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _orderRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            return new PagedResponse<OrderDTO>(
                _mapper.Map<IEnumerable<OrderDTO>>(result.Data),
                result.PageNumber,
                result.PageSize,
                result.TotalRecords);
        }

        public async Task<OrderLogDTO> GetOrderById(int id, CancellationToken cancellationToken = default)
        {
            var tmp = await _orderRepository.GetById(id, cancellationToken);
            return _mapper.Map<OrderLogDTO>(tmp);
        }


    }
}

