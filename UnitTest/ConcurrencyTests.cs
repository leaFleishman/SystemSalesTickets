using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Data;
namespace UnitTest
{
    public class ConcurrencyTests
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Database=SystemSalesTickets;Username=postgres;Password=my pass";
        private static DataContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseNpgsql(ConnectionString).Options;
            return new DataContext(options);
        }
        [Fact]
        public async Task BookingSameEventSeat_SecondUser_ShouldGetConcurrencyException()
        {
            int seatId = 0; try
            {
                await using (var setupContext = CreateContext())
                {
                    var seat = new Seat
                    {
                        Row = Random.Shared.Next(10000, 20000),
                        Line = Random.Shared.Next(10000, 20000)
                    };
                    setupContext.Seats.Add(seat); var eventSeat = new EventSeat { EventId = 1, Seat = seat, IsAvailable = true };
                    setupContext.EventSeats.Add(eventSeat);
                    await setupContext.SaveChangesAsync(); seatId = seat.Id;
                }
                await using var context1 = CreateContext();
                await using var context2 = CreateContext();
                var eventSeat1 = await context1.EventSeats.SingleAsync(x => x.EventId == 1 && x.SeatId == seatId);
                var eventSeat2 = await context2.EventSeats.SingleAsync(x => x.EventId == 1 && x.SeatId == seatId);
                Assert.True(eventSeat1.IsAvailable); Assert.True(eventSeat2.IsAvailable);
                Assert.Equal(eventSeat1.Version, eventSeat2.Version);
                eventSeat1.IsAvailable = false; await context1.SaveChangesAsync();
                Assert.NotEqual(eventSeat1.Version, eventSeat2.Version);
                eventSeat2.IsAvailable = false; await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
            }
            finally
            {
                if (seatId != 0)
                {
                    await using var cleanupContext = CreateContext();
                    var eventSeat = await cleanupContext.EventSeats.SingleOrDefaultAsync(x => x.EventId == 1 && x.SeatId == seatId);
                    if (eventSeat != null) { cleanupContext.EventSeats.Remove(eventSeat); }
                    var seat = await cleanupContext.Seats.SingleOrDefaultAsync(x => x.Id == seatId);
                    if (seat != null) { cleanupContext.Seats.Remove(seat); }
                    await cleanupContext.SaveChangesAsync();
                }
            }
        }
    }
}