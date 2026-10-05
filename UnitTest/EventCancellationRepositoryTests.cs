using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Data;

namespace UnitTest
{
    // Runs the new repository queries against a real (in-memory SQLite) database.
    // The seed data gives: Event 1 ("Concert A", 2026-01-01 UTC), Order 1 (user 2, seat 1), Seats 1 and 2.
    public class EventCancellationRepositoryTests
    {
        private static async Task<(SqliteConnection Connection, DbContextOptions<DataContext> Options)> CreateDatabaseAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            await using var setup = new DataContext(options);
            await setup.Database.EnsureCreatedAsync();

            return (connection, options);
        }

        [Fact]
        public async Task DateInUse_IsTrueForAnotherEventAndFalseWhenThatEventIsExcluded()
        {
            var (connection, options) = await CreateDatabaseAsync();
            await using var _ = connection;
            await using var context = new DataContext(options);
            var repository = new EventRepository(context);

            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.True(await repository.DateInUse(seedDate, excludeEventId: 99));
            Assert.False(await repository.DateInUse(seedDate, excludeEventId: 1));
            Assert.False(await repository.DateInUse(seedDate.AddDays(1), excludeEventId: 99));
        }

        [Fact]
        public async Task CountByEvent_CountsOnlyOrdersOfThatEvent()
        {
            var (connection, options) = await CreateDatabaseAsync();
            await using var _ = connection;
            await using var context = new DataContext(options);
            var repository = new OrderRepository(context);

            Assert.Equal(1, await repository.CountByEvent(1));
            Assert.Equal(0, await repository.CountByEvent(99));
        }

        [Fact]
        public async Task GetOrdersByEvent_LoadsUserAndSeat()
        {
            var (connection, options) = await CreateDatabaseAsync();
            await using var _ = connection;
            await using var context = new DataContext(options);
            var repository = new OrderRepository(context);

            var orders = await repository.GetOrdersByEvent(1);

            var order = Assert.Single(orders);
            Assert.NotNull(order.User);
            Assert.Equal("user@example.com", order.User.Email);
            Assert.NotNull(order.Seat);
            Assert.Equal(1, order.Seat.Id);
        }

        [Fact]
        public async Task GetOrdersPendingReminder_SkipsCancelledEvents()
        {
            var (connection, options) = await CreateDatabaseAsync();
            await using var _ = connection;
            await using var context = new DataContext(options);

            var now = DateTime.UtcNow;
            context.Events.AddRange(
                new Event { Id = 10, Name = "Active show", Date = now.AddHours(5), Price = 10, NumberOfSeats = 5 },
                new Event
                {
                    Id = 11,
                    Name = "Cancelled show",
                    Date = now.AddHours(6),
                    Price = 10,
                    NumberOfSeats = 5,
                    IsCancelled = true,
                    CancelledAt = now
                });
            context.Orders.AddRange(
                new Order { Id = 10, UserId = 2, EventId = 10, EventName = "Active show", SeatId = 2, OrderDate = now },
                new Order { Id = 11, UserId = 2, EventId = 11, EventName = "Cancelled show", SeatId = 2, OrderDate = now });
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var pending = await repository.GetOrdersPendingReminder(now, now.AddHours(24));

            var order = Assert.Single(pending);
            Assert.Equal(10, order.Id);
        }
    }
}
