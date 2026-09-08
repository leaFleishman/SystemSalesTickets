using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        [Required(ErrorMessage = "Row is required")]
        public int Row { get; set; }

        [ Required(ErrorMessage = "Line is required")]
        public int Line { get; set; }


        [Required]
        [ConcurrencyCheck]
        public Guid Version { get; set; } = Guid.NewGuid();
    }

}
