using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Data;

namespace UnitTest
{
    public class ConcurrencyTests
    {
        [Fact]
        public async Task BookingSameEventSeat_SecondUser_ShouldGetConcurrencyException()
        {
            await using var connection =
                new SqliteConnection("DataSource=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            await using (var setupContext = new DataContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();
            }

            await using var context1 = new DataContext(options);
            await using var context2 = new DataContext(options);

            var eventSeat1 = await context1.EventSeats
                .SingleAsync(x =>
                    x.EventId == 1 &&
                    x.SeatId == 2);

            var eventSeat2 = await context2.EventSeats
                .SingleAsync(x =>
                    x.EventId == 1 &&
                    x.SeatId == 2);

            Assert.True(eventSeat1.IsAvailable);
            Assert.True(eventSeat2.IsAvailable);
            Assert.Equal(eventSeat1.Version, eventSeat2.Version);

            eventSeat1.IsAvailable = false;

            await context1.SaveChangesAsync();

            Assert.NotEqual(eventSeat1.Version, eventSeat2.Version);

            eventSeat2.IsAvailable = false;

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => context2.SaveChangesAsync());
        }

        [Fact]
        public async Task TwoUpdatesToSameEventSeat_FirstSucceeds_SecondFailsWithConcurrency()
        {
            await using var connection =
                new SqliteConnection("DataSource=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            await using (var setupContext = new DataContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();
            }

            await using var context1 = new DataContext(options);
            await using var context2 = new DataContext(options);

            var eventSeat1 = await context1.EventSeats
                .SingleAsync(es =>
                    es.EventId == 1 &&
                    es.SeatId == 2);

            var eventSeat2 = await context2.EventSeats
                .SingleAsync(es =>
                    es.EventId == 1 &&
                    es.SeatId == 2);

            Assert.Equal(eventSeat1.Version, eventSeat2.Version);

            eventSeat1.IsAvailable = false;
            eventSeat2.IsAvailable = false;

            await context1.SaveChangesAsync();

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => context2.SaveChangesAsync());

            await using var verifyContext = new DataContext(options);

            var savedEventSeat = await verifyContext.EventSeats
                .SingleAsync(es =>
                    es.EventId == 1 &&
                    es.SeatId == 2);

            Assert.False(savedEventSeat.IsAvailable);
            Assert.NotEqual(
                eventSeat2.Version,
                savedEventSeat.Version);
        }
    }
}