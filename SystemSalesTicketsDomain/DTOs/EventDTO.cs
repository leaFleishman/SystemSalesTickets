namespace SystemSalesTickets.Core.DTOs
{
    public class EventDTO
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public decimal Price { get; set; }

        public int NumberOfSeats { get; set; }
    }
}
