using Microsoft.EntityFrameworkCore;
using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsInfrastructure
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Event> Events { get; set; }
    }
}
