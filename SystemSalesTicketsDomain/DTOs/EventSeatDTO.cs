using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class EventSeatDTO
    {
        public int EventId { get; set; }

        public int SeatId { get; set; }

        public bool IsAvailable { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; }
    }
}