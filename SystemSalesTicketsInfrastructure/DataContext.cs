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
        public DbSet<EventSeat> EventSeats { get; set; }

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

            // Optimistic Concurrency עבור Seat
            modelBuilder.Entity<Seat>()
                .Property(s => s.Version)
                .IsConcurrencyToken();

            // EventSeat - מפתח מורכב
            modelBuilder.Entity<EventSeat>()
                .HasKey(es => new { es.EventId, es.SeatId });

            modelBuilder.Entity<EventSeat>()
                .Property(es => es.IsAvailable)
                .IsRequired();

            // Optimistic Concurrency עבור EventSeat
            modelBuilder.Entity<EventSeat>()
                .Property(es => es.Version)
                .IsConcurrencyToken();

            // קשר Event -> EventSeat
            modelBuilder.Entity<EventSeat>()
                .HasOne(es => es.Event)
                .WithMany()
                .HasForeignKey(es => es.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // קשר Seat -> EventSeat
            modelBuilder.Entity<EventSeat>()
                .HasOne(es => es.Seat)
                .WithMany()
                .HasForeignKey(es => es.SeatId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Role = UserRole.Manager,
                    Email = "admin@example.com",
                    Phone = "0556667788",
                    Password = "AQAAAAIAAYagAAAAEA1k61jf211sNrnVlnardNcGL3S3o4S7xxODit7eCsR8LChzkSZzH1LEABC8M47emg==",
                    Id = 1,
                    UserName = "Avi"
                },
                new User
                {
                    Role = UserRole.User,
                    Email = "user@example.com",
                    Phone = "0556367788",
                    Password = "AQAAAAIAAYagAAAAEEgW8PRhBaBx46pxAz/cboT2Ca/gB+JZ3XxBtqCudaDHMLhbhUNrKRHHtnfoDkHuUA==",
                    Id = 2,
                    UserName = "Moshe"
                }
            );

            // Seed Seats
            modelBuilder.Entity<Seat>().HasData(
                new Seat
                {
                    SeatId = 1,
                    Row = 1,
                    Line = 1,
                    Version = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                },
                new Seat
                {
                    SeatId = 2,
                    Row = 12,
                    Line = 12,
                    Version = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                }
            );

            // Seed Event
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventId = 1,
                    Date = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc
                    ),
                    Name = "Concert A",
                    NumberOfSeats = 2500,
                    Price = 15000
                }
            );

            // Seed EventSeats
            // Seat 1 כבר תפוס על ידי Order 1
            modelBuilder.Entity<EventSeat>().HasData(
                new EventSeat
                {
                    EventId = 1,
                    SeatId = 1,
                    IsAvailable = false,
                    Version = Guid.Parse(
                        "11111111-1111-1111-1111-111111111111"
                    )
                },
                new EventSeat
                {
                    EventId = 1,
                    SeatId = 2,
                    IsAvailable = true,
                    Version = Guid.Parse(
                        "22222222-2222-2222-2222-222222222222"
                    )
                }
            );

            // Seed Order
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderId = 1,
                    EventId = 1,
                    SeatId = 1,
                    UserId = 2,
                    EventName = "Concert A",
                    OrderDate = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc
                    )
                }
            );
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // עדכון Version של Seats ששונו
            var modifiedSeats = ChangeTracker
                .Entries<Seat>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedSeats)
            {
                entry.Property(s => s.Version).CurrentValue = Guid.NewGuid();
            }

            var modifiedEventSeats = ChangeTracker
                .Entries<EventSeat>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiedEventSeats)
            {
                entry.Property(es => es.Version).CurrentValue = Guid.NewGuid();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}