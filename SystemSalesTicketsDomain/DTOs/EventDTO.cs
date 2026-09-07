using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class EventDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date is required")]

        public DateTime Date { get; set; }

        public decimal Price { get; set; }

        [Required(ErrorMessage = "Num of seats is required")]

        public int NumberOfSeats { get; set; }


    }
}
