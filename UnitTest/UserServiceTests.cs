using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace UnitTest
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();

            _service = new UserService(
                _userRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _passwordHasherMock.Object);
        }

        [Fact]
        public async Task AddUser_ReturnsUserLogDTO()
        {
            var userDto = new RegisterRequestDTO
            {
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "Password123!"
            };

            var user = new User
            {
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "Password123!"
            };

            var addedUser = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "hashedPassword"
            };

            var expected = new UserLogDTO();

            _mapperMock
                .Setup(x => x.Map<User>(userDto))
                .Returns(user);

            _passwordHasherMock
                .Setup(x => x.HashPassword(
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns("hashedPassword");

            _userRepositoryMock
                .Setup(x => x.Add(
                    user,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(addedUser);

            _userRepositoryMock
                .Setup(x => x.Save(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<UserLogDTO>(addedUser))
                .Returns(expected);

            var result = await _service.AddUser(
                userDto,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(expected, result);
            Assert.Equal("hashedPassword", user.Password);

            _passwordHasherMock.Verify(
                x => x.HashPassword(
                    user,
                    "Password123!"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Add(
                    user,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsUsers()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    UserName = "User1",
                    Phone = "0501111111",
                    Email = "user1@test.com"
                },
                new User
                {
                    Id = 2,
                    UserName = "User2",
                    Phone = "0502222222",
                    Email = "user2@test.com"
                }
            };

            var expected = new List<UserDTO>
            {
                new UserDTO
                {
                    UserName = "User1",
                    Phone = "0501111111",
                    Email = "user1@test.com"
                },
                new UserDTO
                {
                    UserName = "User2",
                    Phone = "0502222222",
                    Email = "user2@test.com"
                }
            };

            var pagedUsers = new PagedResponse<User>(
                users,
                1,
                20,
                users.Count);

            _userRepositoryMock
                .Setup(x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedUsers);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<UserDTO>>(users))
                .Returns(expected);

            var result = await _service.GetAllUsers(
                1,
                20,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(expected, result.Data);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(20, result.PageSize);
            Assert.Equal(2, result.TotalRecords);

            _userRepositoryMock.Verify(
                x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUserById_ReturnsUser()
        {
            const int id = 1;

            var user = new User
            {
                Id = id,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com"
            };

            var expected = new UserLogDTO();

            _userRepositoryMock
                .Setup(x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(x => x.Map<UserLogDTO>(user))
                .Returns(expected);

            var result = await _service.GetUserById(
                id,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _userRepositoryMock.Verify(
                x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsUser_WhenPasswordIsCorrect()
        {
            var loginModel = new LoginRequestDTO
            {
                Email = "test@test.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "hashedPassword"
            };

            _userRepositoryMock
                .Setup(x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(
                    user,
                    "hashedPassword",
                    "Password123!"))
                .Returns(PasswordVerificationResult.Success);

            var result = await _service.Login(
                loginModel,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(user, result);

            _passwordHasherMock.Verify(
                x => x.VerifyHashedPassword(
                    user,
                    "hashedPassword",
                    "Password123!"),
                Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenUserDoesNotExist()
        {
            var loginModel = new LoginRequestDTO
            {
                Email = "notfound@test.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var result = await _service.Login(
                loginModel,
                CancellationToken.None);

            Assert.Null(result);

            _passwordHasherMock.Verify(
                x => x.VerifyHashedPassword(
                    It.IsAny<User>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenPasswordIsIncorrect()
        {
            var loginModel = new LoginRequestDTO
            {
                Email = "test@test.com",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "hashedPassword"
            };

            _userRepositoryMock
                .Setup(x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(
                    user,
                    "hashedPassword",
                    "WrongPassword"))
                .Returns(PasswordVerificationResult.Failed);

            var result = await _service.Login(
                loginModel,
                CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task MakeUserManager_ReturnsUserDTO()
        {
            const int id = 1;

            var user = new User
            {
                Id = id,
                UserName = "TestUser",
                Phone = "0500000000",
                Email = "test@test.com"
            };

            var userDto = new UserDTO
            {
                UserName = "TestUser",
                Phone = "0500000000",
                Email = "test@test.com"
            };

            _userRepositoryMock
                .Setup(x => x.MakeUserManager(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.Save(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<UserDTO>(user))
                .Returns(userDto);

            var result = await _service.MakeUserManager(
                id,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(userDto, result);

            _userRepositoryMock.Verify(
                x => x.MakeUserManager(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task MakeUserManager_UserNotFound_ReturnsNull()
        {
            const int id = 999;

            _userRepositoryMock
                .Setup(x => x.MakeUserManager(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(x => x.Save(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<UserDTO>(null))
                .Returns((UserDTO?)null);

            var result = await _service.MakeUserManager(
                id,
                CancellationToken.None);

            Assert.Null(result);

            _userRepositoryMock.Verify(
                x => x.MakeUserManager(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddUser_HashesPasswordBeforeSaving()
        {
            var userDto = new RegisterRequestDTO
            {
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "Password123!"
            };

            var user = new User
            {
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com",
                Password = "Password123!"
            };

            var addedUser = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com"
            };

            _mapperMock
                .Setup(x => x.Map<User>(userDto))
                .Returns(user);

            _passwordHasherMock
                .Setup(x => x.HashPassword(
                    It.IsAny<User>(),
                    It.IsAny<string>()))
                .Returns("hashedPassword");

            _userRepositoryMock
                .Setup(x => x.Add(
                    It.IsAny<User>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(addedUser);

            _userRepositoryMock
                .Setup(x => x.Save(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<UserLogDTO>(addedUser))
                .Returns(new UserLogDTO());

            await _service.AddUser(
                userDto,
                CancellationToken.None);

            Assert.Equal("hashedPassword", user.Password);

            _userRepositoryMock.Verify(
                x => x.Add(
                    It.Is<User>(u => u.Password == "hashedPassword"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}