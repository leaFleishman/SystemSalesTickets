using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date is required")]

        public DateTime Date { get; set; }

        public double Price { get; set; }

        [Required(ErrorMessage = "Num of seats is required")]

        public int NumberOfSeats { get; set; }

    }
}
