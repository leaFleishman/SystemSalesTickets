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
        /// <summary>A booking can be cancelled only if the event starts in MORE than this.</summary>
        public static readonly TimeSpan CancellationWindow = TimeSpan.FromHours(24);

        private readonly TimeProvider _timeProvider;
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
            IOrderConfirmationEmailService orderConfirmationEmailService,
            TimeProvider? timeProvider = null)
        {
            _timeProvider = timeProvider ?? TimeProvider.System;
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

                if (eventSeat.Event?.IsCancelled == true)
                {
                    return new OrderResultDTO
                    {
                        Status = OrderResultStatus.Conflict,
                        Message = "This event was cancelled"
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

        public async Task<IReadOnlyList<MyOrderDTO>> GetMyOrders(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetOrdersByUser(userId, cancellationToken);
            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            return orders.Select(o =>
            {
                var eventDate = ToUtc(o.Event.Date);
                var deadline = eventDate - CancellationWindow;

                return new MyOrderDTO
                {
                    OrderId = o.Id,
                    EventId = o.EventId,
                    EventName = o.Event.Name,
                    EventDate = eventDate,
                    Price = (decimal)o.Event.Price,
                    SeatId = o.SeatId,
                    Row = o.Seat.Row,
                    Line = o.Seat.Line,
                    OrderDate = o.OrderDate,
                    EventIsCancelled = o.Event.IsCancelled,
                    CancellationDeadline = deadline,
                    CanCancel = !o.Event.IsCancelled && nowUtc < deadline
                };
            }).ToList();
        }

        public async Task<OrderResultDTO> CancelOrder(
            int orderId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdForCancellation(orderId, cancellationToken);

                // Someone else's order looks exactly like a missing one (no information leak).
                if (order == null || order.UserId != userId)
                {
                    return new OrderResultDTO
                    {
                        Status = OrderResultStatus.NotFound,
                        Message = "Order not found"
                    };
                }

                if (order.Event.IsCancelled)
                {
                    return new OrderResultDTO
                    {
                        Status = OrderResultStatus.Conflict,
                        Message = "This event was cancelled"
                    };
                }

                // "More than 24 hours before the event": exactly 24h is already too late.
                var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
                var deadline = ToUtc(order.Event.Date) - CancellationWindow;
                if (nowUtc >= deadline)
                {
                    return new OrderResultDTO
                    {
                        Status = OrderResultStatus.Conflict,
                        Message = "Orders can only be cancelled more than 24 hours before the event"
                    };
                }

                // Release the seat and delete the order in ONE SaveChanges (single transaction).
                var eventSeat = await _eventSeatRepository.GetByEventAndSeat(
                    order.EventId, order.SeatId, cancellationToken);
                if (eventSeat != null)
                {
                    eventSeat.IsAvailable = true;
                }

                _orderRepository.Remove(order);
                await _orderRepository.Save(cancellationToken);

                _logger.LogInformation(
                    "Order {OrderId} of user {UserId} was cancelled (Event {EventId}, Seat {SeatId})",
                    orderId, userId, order.EventId, order.SeatId);

                return new OrderResultDTO
                {
                    Status = OrderResultStatus.Success,
                    Message = "Order cancelled"
                };
            }
            catch (ConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while cancelling order {OrderId}", orderId);

                return new OrderResultDTO
                {
                    Status = OrderResultStatus.Conflict,
                    Message = "The order was modified by someone else, please try again"
                };
            }
        }

        // The DB returns Event.Date with Kind=Unspecified; the whole system stores UTC.
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}