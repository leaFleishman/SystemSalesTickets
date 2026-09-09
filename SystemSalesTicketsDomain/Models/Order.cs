using System.ComponentModel.DataAnnotations;
using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; }

        public int SeatId { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }

        public User User { get; set; }

        public Event Event { get; set; }

        [Required(ErrorMessage = "Seat is reqired")]
        public Seat Seat { get; set; }
    }
}
