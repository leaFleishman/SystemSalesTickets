using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Api.Controllers;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;

namespace UnitTest
{
    public class EventControllerTests
    {
        private readonly Mock<IEventService> _serviceMock = new();
        private readonly EventController _controller;

        public EventControllerTests()
        {
            _controller = new EventController(
                _serviceMock.Object,
                new Mock<ILogger<EventController>>().Object);
        }

        private static UpdateEventDTO Dto() => new UpdateEventDTO
        {
            Name = "Concert",
            Date = DateTime.UtcNow.AddDays(10),
            Price = 100,
            NumberOfSeats = 50
        };

        // ------------------------------ Update ------------------------------

        [Fact]
        public async Task UpdateEvent_OnSuccess_ReturnsOkWithEvent()
        {
            var updated = new EventDTO { Id = 1, Name = "Concert" };
            _serviceMock
                .Setup(s => s.Update(1, It.IsAny<UpdateEventDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventResultDTO { Status = EventResultStatus.Success, Event = updated });

            var result = await _controller.UpdateEvent(1, Dto(), CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(updated, ok.Value);
        }

        [Theory]
        [InlineData(EventResultStatus.NotFound, typeof(NotFoundObjectResult))]
        [InlineData(EventResultStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(EventResultStatus.Invalid, typeof(BadRequestObjectResult))]
        public async Task UpdateEvent_OnFailure_MapsStatusToHttpResult(EventResultStatus status, Type expected)
        {
            _serviceMock
                .Setup(s => s.Update(1, It.IsAny<UpdateEventDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventResultDTO { Status = status, Message = "problem" });

            var result = await _controller.UpdateEvent(1, Dto(), CancellationToken.None);

            Assert.IsType(expected, result);
            var body = (ObjectResult)result;
            Assert.Equal("problem", body.Value);
        }

        // ------------------------------ Cancel ------------------------------

        [Fact]
        public async Task CancelEvent_OnSuccess_ReturnsOkWithResult()
        {
            var serviceResult = new EventResultDTO
            {
                Status = EventResultStatus.Success,
                Event = new EventDTO { Id = 1, IsCancelled = true },
                AffectedOrders = 4,
                NotificationsSent = 3
            };
            _serviceMock
                .Setup(s => s.Cancel(1, "Artist is ill", It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            var result = await _controller.CancelEvent(
                1, new CancelEventDTO { Reason = "Artist is ill" }, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<EventResultDTO>(ok.Value);
            Assert.True(body.Event!.IsCancelled);
            Assert.Equal(4, body.AffectedOrders);
            Assert.Equal(3, body.NotificationsSent);
        }

        [Fact]
        public async Task CancelEvent_WithoutBody_PassesNullReason()
        {
            _serviceMock
                .Setup(s => s.Cancel(1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventResultDTO { Status = EventResultStatus.Success, Event = new EventDTO() });

            var result = await _controller.CancelEvent(1, null, CancellationToken.None);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.Verify(s => s.Cancel(1, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(EventResultStatus.NotFound, typeof(NotFoundObjectResult))]
        [InlineData(EventResultStatus.Conflict, typeof(ConflictObjectResult))]
        public async Task CancelEvent_OnFailure_MapsStatusToHttpResult(EventResultStatus status, Type expected)
        {
            _serviceMock
                .Setup(s => s.Cancel(1, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventResultDTO { Status = status, Message = "problem" });

            var result = await _controller.CancelEvent(1, new CancelEventDTO(), CancellationToken.None);

            Assert.IsType(expected, result);
            Assert.Equal("problem", ((ObjectResult)result).Value);
        }
    }
}
