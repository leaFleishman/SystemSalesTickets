using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class OrderDTO
    {
        [Range(1, int.MaxValue)]

        public int EventId { get; set; }

        [Range(1, int.MaxValue)]

        public int SeatId { get; set; }

        [Range(1, int.MaxValue)]

        public int UserId { get; set; }
    }
}
