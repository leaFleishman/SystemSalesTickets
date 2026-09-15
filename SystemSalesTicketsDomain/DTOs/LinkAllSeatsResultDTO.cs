using SystemSalesTickets.Core.Enums;

namespace SystemSalesTickets.Core.DTOs
{
    public class LinkAllSeatsResultDTO
    {
        public OrderResultStatus Status { get; set; }
        public int LinkedCount { get; set; }
        public string? Message { get; set; }
    }
}
