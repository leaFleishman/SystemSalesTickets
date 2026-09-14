using SystemSalesTickets.Core.Enums;

namespace SystemSalesTickets.Core.DTOs
{
    public class EventSeatResultDTO
    {
        public OrderResultStatus Status { get; set; }
        public EventSeatDTO EventSeat { get; set; }
        public string? Message { get; set; }
    }
}
