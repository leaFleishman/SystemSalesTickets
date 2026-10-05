using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    /// <summary>Editable fields of an event (PUT api/Event/{id}).</summary>
    public class UpdateEventDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of seats must be at least 1")]
        public int NumberOfSeats { get; set; }
    }
}
