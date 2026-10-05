using AutoMapper;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core;
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
        private readonly IUserRepository _userRepository;
        private readonly IOrderConfirmationEmailService _orderConfirmationEmailService;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<OrderService> logger,
            IEventSeatRepository eventSeatRepository,
            IUserRepository userRepository,
            IOrderConfirmationEmailService orderConfirmationEmailService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
            _eventSeatRepository = eventSeatRepository;
            _userRepository = userRepository;
            _orderConfirmationEmailService = orderConfirmationEmailService;
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

                // חובה למלא את השדות האלה לפני השמירה
                newOrder.EventName = eventSeat.Event.Name;
                newOrder.OrderDate = DateTime.UtcNow;

                await _orderRepository.Add(
                    newOrder,
                    cancellationToken);

                await _orderRepository.Save(
                    cancellationToken);

                // ההזמנה נשמרה בהצלחה.
                // עכשיו מביאים את המשתמש כדי לקבל את כתובת המייל
                // ולא מסתמכים על User navigation שלא נטען.
                var user = await _userRepository.GetById(
                    orderDto.UserId,
                    cancellationToken);

                if (user != null &&
                    !string.IsNullOrWhiteSpace(user.Email))
                {
                    // מכינים את הנתונים הדרושים למייל.
                    // ה-Order עצמו עדיין לא מכיל User/Seat/מבנה מלא,
                    // לכן נטען כאן את הנתונים הדרושים.
                    newOrder.User = user;
                    newOrder.Event = eventSeat.Event;

                    // אם EventSeatRepository טוען את Seat,
                    // הוא יהיה זמין כאן.
                    newOrder.Seat = eventSeat.Seat;

                    try
                    {
                        await _orderConfirmationEmailService.SendAsync(
                            newOrder,
                            user.Email,
                            user.UserName,
                            cancellationToken);

                        _logger.LogInformation(
                            "Order confirmation email sent for OrderId {OrderId} to {Email}",
                            newOrder.Id,
                            user.Email);
                    }
                    catch (Exception ex)
                    {
                        // ההזמנה כבר נשמרה בהצלחה.
                        // כשל בשליחת המייל לא אמור להפוך את ההזמנה ל-500.
                        _logger.LogError(
                            ex,
                            "Failed to send order confirmation email for OrderId {OrderId}",
                            newOrder.Id);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Could not send order confirmation email for OrderId {OrderId}: user {UserId} was not found or has no email",
                        newOrder.Id,
                        orderDto.UserId);
                }

                return new OrderResultDTO
                {
                    Status = OrderResultStatus.Success,
                    Order = _mapper.Map<OrderLogDTO>(newOrder)
                };
            }
            catch (ConcurrencyException ex)
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