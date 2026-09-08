using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Enums;
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
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // אינדקס ייחודי למניעת הזמנה כפולה ברמת המסד
            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.EventId, o.SeatId })
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Date)
                .IsUnique();

            modelBuilder.Entity<Seat>()
                .ToTable("Seat");

            modelBuilder.Entity<Seat>()
                .Property(s => s.Version)
                .IsConcurrencyToken();


            modelBuilder.Entity<EventSeat>()
    .HasKey(es => new { es.EventId, es.SeatId });

            modelBuilder.Entity<EventSeat>()
                .Property(es => es.Version)
                .IsConcurrencyToken();
            // Seed Data עם ערכי Version קבועים
            modelBuilder.Entity<User>().HasData(
                new User { Role = UserRole.Manager, Email = "admin@example.com", Phone = "0556667788", Password = "111", Id = 1, UserName = "Avi" },
                new User { Role = UserRole.User, Email = "user@example.com", Phone = "0556367788", Password = "222", Id = 2, UserName = "Moshe" }
            );

            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatId = 1, Row = 1, Line = 1, Version = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                new Seat { SeatId = 2, Row = 12, Line = 12, Version = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Name = "Concert A", NumberOfSeats = 2500, Price = 15000 }
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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedSeats = ChangeTracker
                .Entries<Seat>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedSeats)
            {
                // עדכון רק של הערך החדש כדי לא לדרוס את ה-OriginalValue של ה-Concurrency Check
                entry.Property(s => s.Version).CurrentValue = Guid.NewGuid();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}