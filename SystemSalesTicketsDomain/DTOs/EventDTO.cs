using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class EventDTO
    {
        // Not [Required] on purpose: callers creating an event don't send
        // this (the DB generates it), but callers reading an event need it
        // to place orders, look up seats, etc.
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]

        public decimal Price { get; set; }

        [Required(ErrorMessage = "Num of seats is required")]

        public int NumberOfSeats { get; set; }


    }
}
