using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Seat>()
                .Property(s => s.Version);

            modelBuilder.Entity<Seat>()
                .ToTable("Seat");

            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Date)
                .IsUnique();

            modelBuilder.Entity<User>().HasData(
                new User { Role = UserRole.Manager, Email = "admin@example.com", Phone = "0556667788", Password = "111", Id = 1, UserName = "Avi" },
                new User { Role = UserRole.User, Email = "user@example.com", Phone = "0556367788", Password = "222", Id = 2, UserName = "Moshe" }
            );

            modelBuilder.Entity<Seat>().HasData(
                new Seat { Row = 1, Line = 1, IsAvailable = true, SeatId = 1 },
                new Seat { Row = 12, Line = 12, IsAvailable = true, SeatId = 2 }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Name = "Concert A", NumberOfSeats = 2500, Price = 15000, EventId = 1 }
            );

            modelBuilder.Entity<Order>().HasData(
       new Order
       {
           OrderId = 1,
           EventId = 1,
           SeatId = 1,
           UserId = 2,
           EventName = "Concert A",
           OrderDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
       }
   );
        }

        public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            var modifiedSeats = ChangeTracker
                .Entries<Seat>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedSeats)
            {
                entry.Entity.Version = Guid.NewGuid();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
