namespace SystemSalesTicketsDomain.models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; }

        public int SeatId { get; set; }

        public DateTime OrderDate { get; set; }

        public User User { get; set; }

        public Event Event { get; set; }

        public Seat Seat { get; set; }
    }
}
