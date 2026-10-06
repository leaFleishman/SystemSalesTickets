namespace SystemSalesTickets.Core.DTOs
{
    public class AdminDashboardDTO
    {
        public int ActiveEvents { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public int AvailableSeats { get; set; }
        public IReadOnlyList<DashboardEventDTO> UpcomingEvents { get; set; } = Array.Empty<DashboardEventDTO>();
    }

    public class DashboardEventDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TicketsSold { get; set; }
        public int AvailableSeats { get; set; }
    }
}