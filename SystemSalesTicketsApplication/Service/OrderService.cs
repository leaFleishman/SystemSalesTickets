using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using MyApp.Application.Common.Models;
using Npgsql;

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
     OrderDTO orderDto,
     CancellationToken cancellationToken = default)
        {
            try
            {
                var seat = await _seatRepository.GetById(
                    orderDto.SeatId,
                    cancellationToken);

                if (seat == null)
                {
                    return new OrderLogDTO
                    {
                        Message = "Seat not found"
                    };
                }

                var isBooked = await _orderRepository.ExistsForEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    cancellationToken);

                if (isBooked)
                {
                    return new OrderLogDTO
                    {
                        Message = "Seat is already occupied"
                    };
                }

                // שינוי ב-Seat מפעיל את מנגנון ה-Optimistic Concurrency
                seat.Row = seat.Row;

                await _seatRepository.Update(seat, cancellationToken);

                var newOrder = _mapper.Map<Order>(orderDto);

                await _orderRepository.Add(newOrder, cancellationToken);

                await _orderRepository.Save(cancellationToken);

                return _mapper.Map<OrderLogDTO>(newOrder);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict for Seat {SeatId}, Event {EventId}",
                    orderDto.SeatId,
                    orderDto.EventId);

                return new OrderLogDTO
                {
                    Message = "Seat was booked by someone else, please try again"
                };
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException postgresException &&
                postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                _logger.LogWarning(
                    ex,
                    "Duplicate booking attempt for Seat {SeatId}, Event {EventId}",
                    orderDto.SeatId,
                    orderDto.EventId);

                return new OrderLogDTO
                {
                    Message = "Seat was just booked by someone else, please try again"
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

