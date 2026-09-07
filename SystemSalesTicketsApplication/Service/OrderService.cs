using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly ISeatRepository _seatRepository;
        private static int counter = new Random().Next();
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

        public async Task<OrderLogDTO> AddOrder(OrderDTO order, CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<Order>(order);

            var seat = await _seatRepository.GetById(tmp.Seat.SeatId, cancellationToken);
            if (seat == null)
            {
                _logger.LogInformation("Seat {seatId} not found", tmp.Seat.SeatId); 
                return new OrderLogDTO { Message = "Seat not found" };
            }

            if (!seat.IsAvailable)
            {
                _logger.LogInformation("Seat {seatId} is already occupied", seat.SeatId);
                return new OrderLogDTO { Message = "Seat is already occupied" };
            }
           
            seat.IsAvailable = false;

            try
            {
                await _seatRepository.Update(seat, cancellationToken); 
                await _orderRepository.Add(tmp, cancellationToken);     
            }
            catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is DbUpdateException || ex is InvalidOperationException)
            {
                _logger.LogWarning(ex, "Seat {seatId} was booked concurrently", seat.SeatId);
                return new OrderLogDTO
                {
                    Message = "Seat was just booked by someone else, please try again"
                };
            }
            catch (Exception)
            {
                _logger.LogWarning("Seat {seatId} update failed while creating order", seat.SeatId);
                return new OrderLogDTO
                {
                    Message = "Seat was just booked by someone else, please try again"
                };
            }
            return _mapper.Map<OrderLogDTO>(tmp);
        }


        public async Task<IEnumerable<OrderDTO>> GetAllOrders(CancellationToken cancellationToken = default)
        {
            var tmp = await _orderRepository.GetAll(cancellationToken);
            return _mapper.Map<IEnumerable<OrderDTO>>(tmp);
        }

        public async Task<OrderLogDTO> GetOrderById(int id, CancellationToken cancellationToken = default)
        {
            var tmp = await _orderRepository.GetById(id, cancellationToken);
            return _mapper.Map<OrderLogDTO>(tmp);
        }


    }
}

