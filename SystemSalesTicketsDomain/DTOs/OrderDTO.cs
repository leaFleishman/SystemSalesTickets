

using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.DTOs
{
    public class OrderDTO
    {
        public string EventName { get; set; }

        public DateTime OrderDate { get; set; }

        public EventDTO EventDTO { get; set; }

        public Seat SeatDTO { get; set; }
    }
}
