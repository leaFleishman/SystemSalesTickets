namespace SystemSalesTicketsDomain.models
{
    public class Seat
    {
        public int SeatId { get; set; }

        public int Row { get; set; }

        public int Line { get; set; }

        public bool IsAvailable { get; set; }
    }
}
