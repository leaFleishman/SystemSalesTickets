using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public decimal Price { get; set; }

        public int NumberOfSeats { get; set; }

    }
}
