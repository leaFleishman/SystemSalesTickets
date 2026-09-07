using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.Models
{
    public class Seat
    {
        public int SeatId { get; set; }

        public int Row { get; set; }

        public int Line { get; set; }

        public bool IsAvailable { get; set; }

        public uint Version { get; set; }
    }

}
