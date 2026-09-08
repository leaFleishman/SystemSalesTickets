using System.ComponentModel.DataAnnotations;
using SystemSalesTickets.Core.Models;

public class EventSeat
{
    public int EventId { get; set; }
    public Event Event { get; set; }

    public int SeatId { get; set; }
    public Seat Seat { get; set; }


    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();
}