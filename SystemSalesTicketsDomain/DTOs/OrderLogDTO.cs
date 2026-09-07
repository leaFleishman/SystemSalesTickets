using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.DTOs
{
    public class OrderLogDTO
    {
        public string EventName { get; set; }

        public DateTime OrderDate { get; set; }

        public EventDTO EventDTO { get; set; }

        public SeatDTO SeatDTO { get; set; }

        public int Id { get; set; }

        public string Message { get; set; }
    }
}
