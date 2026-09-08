using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;
using MyApp.Application.Common.Models;
using Xunit;

namespace SystemSalesTickets.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;

        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _service = new UserService(
                _userRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }


        // =====================================================
        // AddUser
        // =====================================================

        [Fact]
        public async Task AddUser_ReturnsUserLogDTO()
        {
            // Arrange
            var userDto = new UserDTO
            {
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
            };

            var user = new User
            {
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "1234"
            };

            var addedUser = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "1234"
            };

            var expected = new UserLogDTO();

            _mapperMock
                .Setup(x => x.Map<User>(userDto))
                .Returns(user);

            _userRepositoryMock
                .Setup(x => x.Add(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(addedUser);

            _mapperMock
                .Setup(x => x.Map<UserLogDTO>(addedUser))
                .Returns(expected);

            // Act
            var result = await _service.AddUser(userDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            Assert.NotEqual(0, user.Id);

            _mapperMock.Verify(
                x => x.Map<User>(userDto),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Add(user, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<UserLogDTO>(addedUser),
                Times.Once);
        }


        // =====================================================
        // GetAllUsers
        // =====================================================

        [Fact]
        public async Task GetAllUsers_ReturnsUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    UserName = "User1",
                    Phone = "0501111111",
                    Email = "user1@test.com",
                    Password = "1234"
                },

                new User
                {
                    Id = 2,
                    UserName = "User2",
                    Phone = "0502222222",
                    Email = "user2@test.com",
                    Password = "5678"
                }
            };

            var expected = new List<UserDTO>
            {
                new UserDTO
                {
                    UserName = "User1",
                    Phone = "0501111111",
                    Email = "user1@test.com",
                },

                new UserDTO
                {
                    UserName = "User2",
                    Phone = "0502222222",
                    Email = "user2@test.com",
                }
            };

            _userRepositoryMock
                .Setup(x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<User>(users, 1, 20, users.Count));

            _mapperMock
                .Setup(x => x.Map<IEnumerable<UserDTO>>(users))
                .Returns(expected);

            // Act
            var result = await _service.GetAllUsers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result.Data);

            _userRepositoryMock.Verify(
                x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<IEnumerable<UserDTO>>(users),
                Times.Once);
        }


        // =====================================================
        // GetUserById
        // =====================================================

        [Fact]
        public async Task GetUserById_ReturnsUser()
        {
            // Arrange
            int id = 1;

            var user = new User
            {
                Id = id,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "1234"
            };

            var expected = new UserLogDTO();

            _userRepositoryMock
                .Setup(x => x.GetById(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(x => x.Map<UserLogDTO>(user))
                .Returns(expected);

            // Act
            var result = await _service.GetUserById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _userRepositoryMock.Verify(
                x => x.GetById(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<UserLogDTO>(user),
                Times.Once);
        }


        // =====================================================
        // Login
        // =====================================================

        [Fact]
        public async Task Login_ReturnsUser()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "test@test.com",
                Password = "1234"
            };

            var expected = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "1234"
            };

            _userRepositoryMock
                .Setup(x => x.Login(loginModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.Login(loginModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _userRepositoryMock.Verify(
                x => x.Login(loginModel, It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task MakeUserManager_ReturnsUserDTO()
        {
            // Arrange
            int id = 1;

            var user = new User
            {
                Id = id,
                UserName = "TestUser",
                Phone = "0500000000",
                Email = "test@test.com",
                Password = "1234"
            };

            var userDto = new UserDTO
            {
                UserName = "TestUser",
                Phone = "0500000000",
                Email = "test@test.com",
            };

            _userRepositoryMock
                .Setup(r => r.MakeUserManager(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(m => m.Map<UserDTO>(user))
                .Returns(userDto);

            // Act
            var result = await _service.MakeUserManager(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userDto.UserName, result.UserName);
            Assert.Equal(userDto.Email, result.Email);

            _userRepositoryMock.Verify(
                r => r.MakeUserManager(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                m => m.Map<UserDTO>(user),
                Times.Once);
        }

        [Fact]
        public async Task MakeUserManager_UserNotFound_ReturnsNull()
        {
            // Arrange
            int id = 999;

            _userRepositoryMock
                .Setup(r => r.MakeUserManager(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _mapperMock
                .Setup(m => m.Map<UserDTO>(null))
                .Returns((UserDTO)null);

            // Act
            var result = await _service.MakeUserManager(id);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                r => r.MakeUserManager(id, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
