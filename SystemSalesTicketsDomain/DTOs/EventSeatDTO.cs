
using System.ComponentModel.DataAnnotations;


namespace SystemSalesTickets.Core.DTOs
{
    public class EventSeatDTO
    {
        public int EventId { get; set; }

        public int SeatId { get; set; }

        bool IsAvailable { get; set; } = true;

        [ConcurrencyCheck]
        public Guid Version { get; set; } = Guid.NewGuid();
    }
}
