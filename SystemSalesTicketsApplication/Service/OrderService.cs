using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly IEventSeatRepository _eventSeatRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<OrderService> logger,
            IEventSeatRepository eventSeatRepository)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
            _eventSeatRepository = eventSeatRepository;
        }

        public async Task<OrderResultDTO> AddOrder(
    OrderDTO orderDto,
    CancellationToken cancellationToken = default)
        {
            try
            {
                var eventSeat = await _eventSeatRepository.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    cancellationToken);

                if (eventSeat == null)
                {
                    return new OrderResultDTO
                    {
                        Status = OrderResultStatus.NotFound,
                        Message = "Seat not found"
                    };
                }

                if (!eventSeat.IsAvailable)
                {
                    return new OrderResultDTO
                    {
                        Status = OrderResultStatus.Conflict,
                        Message = "Seat is already occupied"
                    };
                }

                // זה המשאב שעליו מתבצעת התחרות
                eventSeat.IsAvailable = false;

                var newOrder = _mapper.Map<Order>(orderDto);

                await _orderRepository.Add(
                    newOrder,
                    cancellationToken);

                await _orderRepository.Save(
                    cancellationToken);

                return new OrderResultDTO
                {
                    Status = OrderResultStatus.Success,
                    Order = _mapper.Map<OrderLogDTO>(newOrder)
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict for Seat {SeatId}, Event {EventId}",
                    orderDto.SeatId,
                    orderDto.EventId);

                return new OrderResultDTO
                {
                    Status = OrderResultStatus.Conflict,
                    Message = "Seat was just booked by someone else, please try again"
                };
            }
        }

        public async Task<PagedResponse<OrderDTO>> GetAllOrders(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _orderRepository.GetAllAsync(
                pageNumber,
                pageSize,
                cancellationToken);

            return new PagedResponse<OrderDTO>(
                _mapper.Map<IEnumerable<OrderDTO>>(result.Data),
                result.PageNumber,
                result.PageSize,
                result.TotalRecords);
        }

        public async Task<OrderLogDTO> GetOrderById(
            int id,
            CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetById(
                id,
                cancellationToken);

            return _mapper.Map<OrderLogDTO>(order);
        }
    }
}