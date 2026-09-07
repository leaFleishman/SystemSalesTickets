using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

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


        // =========================================================
        // GetAll
        // =========================================================

        [Fact]
        public async Task GetAll_ReturnsAllSeats()
        {
            // Arrange
            var seats = new List<Seat>
            {
                new Seat
                {
                    SeatId = 1,
                    Row = 1,
                    Line = 1,
                    IsAvailable = true
                },
                new Seat
                {
                    SeatId = 2,
                    Row = 1,
                    Line = 2,
                    IsAvailable = true
                }
            };

            var expected = new List<SeatDTO>
            {
                new SeatDTO
                {
                    Row = 1,
                    Line = 1,
                    IsAvailable = true
                },
                new SeatDTO
                {
                    Row = 1,
                    Line = 2,
                    IsAvailable = true
                }
            };

            _seatRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(seats);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<SeatDTO>>(seats))
                .Returns(expected);

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _seatRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<IEnumerable<SeatDTO>>(seats),
                Times.Once);
        }


        // =========================================================
        // GetById
        // =========================================================

        [Fact]
        public async Task GetById_WhenSeatExists_ReturnsSeatLogDTO()
        {
            // Arrange
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
                .Setup(x => x.GetById(id))
                .ReturnsAsync(seat);

            _mapperMock
                .Setup(x => x.Map<SeatLogDTO>(seat))
                .Returns(expected);

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _seatRepositoryMock.Verify(
                x => x.GetById(id),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<SeatLogDTO>(seat),
                Times.Once);
        }


        // =========================================================
        // Add
        // =========================================================

        [Fact]
        public async Task Add_WhenSeatIsValid_AddsSeatAndReturnsResult()
        {
            // Arrange
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
                .Setup(x => x.Add(seat))
                .ReturnsAsync(addedSeat);

            _mapperMock
                .Setup(x => x.Map<SeatLogDTO>(addedSeat))
                .Returns(expected);

            // Act
            var result = await _service.Add(seatDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            // בדיקה שה-ID קיבל ערך
            Assert.NotEqual(0, seat.SeatId);

            // בדיקה שה-Repository קיבל את הכיסא
            _seatRepositoryMock.Verify(
                x => x.Add(seat),
                Times.Once);

            // בדיקה שה-Mapping בוצע
            _mapperMock.Verify(
                x => x.Map<Seat>(seatDto),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<SeatLogDTO>(addedSeat),
                Times.Once);
        }


        // =========================================================
        // Add - Repository throws exception
        // =========================================================

        [Fact]
        public async Task Add_WhenRepositoryThrows_ThrowsException()
        {
            // Arrange
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
                .Setup(x => x.Add(seat))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.Add(seatDto));

            Assert.Equal("Database error", exception.Message);

            _seatRepositoryMock.Verify(
                x => x.Add(seat),
                Times.Once);
        }
    }
}
