

using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class SeatDTO
    {
        [Required(ErrorMessage = "Row is required")]
        public int Row { get; set; }

        [Required(ErrorMessage = "Line is required")]
        public int Line { get; set; }

        public bool IsAvailable { get; set; }
    }
}
