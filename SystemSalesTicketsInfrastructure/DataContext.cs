using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seat>()
                .Property(s => s.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Date)
                .IsUnique();
        }
    }
}
