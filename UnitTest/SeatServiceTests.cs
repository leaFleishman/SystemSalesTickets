using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;
using MyApp.Application.Common.Models;

namespace SystemSalesTickets.Tests
{
    public class SeatServiceTests
    {
        private readonly Mock<ISeatRepository> _seatRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<SeatService>> _loggerMock;

        private readonly SeatService _service;

        public SeatServiceTests()
        {
            _seatRepositoryMock = new Mock<ISeatRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<SeatService>>();

            _service = new SeatService(
                _seatRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAll_ReturnsAllSeats()
        {
            var seats = new List<Seat>
            {
                new Seat { SeatId = 1, Row = 1, Line = 1, IsAvailable = true },
                new Seat { SeatId = 2, Row = 1, Line = 2, IsAvailable = true }
            };

            var expected = new List<SeatDTO>
            {
                new SeatDTO { Row = 1, Line = 1, IsAvailable = true },
                new SeatDTO { Row = 1, Line = 2, IsAvailable = true }
            };

            _seatRepositoryMock
                .Setup(x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<Seat>(seats, 1, 20, seats.Count));

            _mapperMock
                .Setup(x => x.Map<IEnumerable<SeatDTO>>(seats))
                .Returns(expected);

            var result = await _service.GetAll();

            Assert.NotNull(result);
            Assert.Equal(expected, result.Data);

            _seatRepositoryMock.Verify(
                x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<IEnumerable<SeatDTO>>(seats),
                Times.Once);
        }

        [Fact]
        public async Task GetById_WhenSeatExists_ReturnsSeatLogDTO()
        {
            int id = 1;

            var seat = new Seat
            {
                SeatId = id,
                Row = 1,
                Line = 1,
                IsAvailable = true
            };

            var expected = new SeatLogDTO();

            _seatRepositoryMock
                .Setup(x => x.GetById(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);

            _mapperMock
                .Setup(x => x.Map<SeatLogDTO>(seat))
                .Returns(expected);

            var result = await _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _seatRepositoryMock.Verify(
                x => x.GetById(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<SeatLogDTO>(seat),
                Times.Once);
        }

        [Fact]
        public async Task Add_WhenSeatIsValid_AddsSeatAndReturnsResult()
        {
            var seatDto = new SeatDTO
            {
                Row = 1,
                Line = 1,
                IsAvailable = true
            };

            var seat = new Seat
            {
                Row = 1,
                Line = 1,
                IsAvailable = true
            };

            var addedSeat = new Seat
            {
                SeatId = 100,
                Row = 1,
                Line = 1,
                IsAvailable = true
            };

            var expected = new SeatLogDTO();

            _mapperMock
                .Setup(x => x.Map<Seat>(seatDto))
                .Returns(seat);

            _seatRepositoryMock
                .Setup(x => x.Add(seat, It.IsAny<CancellationToken>()))
                .ReturnsAsync(addedSeat);

            _mapperMock
                .Setup(x => x.Map<SeatLogDTO>(addedSeat))
                .Returns(expected);

            var result = await _service.Add(seatDto);

            Assert.NotNull(result);
            Assert.Equal(expected, result);
            Assert.NotEqual(0, seat.SeatId);

            _seatRepositoryMock.Verify(
                x => x.Add(seat, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<Seat>(seatDto),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<SeatLogDTO>(addedSeat),
                Times.Once);
        }

        [Fact]
        public async Task Add_WhenRepositoryThrows_ThrowsException()
        {
            var seatDto = new SeatDTO
            {
                Row = 1,
                Line = 1,
                IsAvailable = true
            };

            var seat = new Seat
            {
                Row = 1,
                Line = 1,
                IsAvailable = true
            };

            _mapperMock
                .Setup(x => x.Map<Seat>(seatDto))
                .Returns(seat);

            _seatRepositoryMock
                .Setup(x => x.Add(seat, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.Add(seatDto));

            Assert.Equal("Database error", exception.Message);

            _seatRepositoryMock.Verify(
                x => x.Add(seat, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSeatAsync_WhenSeatExists_ReturnsTrue()
        {
            const int id = 1;

            _seatRepositoryMock
                .Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteSeatAsync(id);

            Assert.True(result);
            _seatRepositoryMock.Verify(
                x => x.DeleteAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSeatAsync_WhenSeatDoesNotExist_ReturnsFalse()
        {
            const int id = 999;

            _seatRepositoryMock
                .Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.DeleteSeatAsync(id);

            Assert.False(result);
            _seatRepositoryMock.Verify(
                x => x.DeleteAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
