using SystemSalesTickets.Core.Enums;

namespace SystemSalesTickets.Core.DTOs
{
    public class OrderResultDTO
    {
        public OrderResultStatus Status { get; set; }
        public OrderLogDTO Order { get; set; }
        public string? Message { get; set; }
    }
}