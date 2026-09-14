

using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class SeatDTO
    {
        // Not [Required]: omitted on create (DB-generated), populated on read.
        public int Id { get; set; }

        [Required(ErrorMessage = "Row is required")]
        public int Row { get; set; }

        [Required(ErrorMessage = "Line is required")]
        public int Line { get; set; }

    }
}
