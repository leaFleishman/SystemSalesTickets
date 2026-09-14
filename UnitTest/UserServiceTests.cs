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
                 _passwordHasherMock.Object

            );
        }

        [Fact]
        public async Task AddUser_ReturnsUserLogDTO()
        {
            // Arrange
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

            _userRepositoryMock
                .Setup(x => x.Add(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(addedUser);

            _userRepositoryMock
                .Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<UserLogDTO>(addedUser))
                .Returns(expected);

            // Act
            var result = await _service.AddUser(userDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _mapperMock.Verify(
                x => x.Map<User>(userDto),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Add(user, It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<UserLogDTO>(addedUser),
                Times.Once);
        }

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

            // Act
            var result = await _service.GetAllUsers();

            // Assert
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

            _mapperMock.Verify(
                x => x.Map<IEnumerable<UserDTO>>(users),
                Times.Once);
        }

        [Fact]
        public async Task GetUserById_ReturnsUser()
        {
            // Arrange
            var id = 1;

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
                .Setup(x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()))
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
                x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<UserLogDTO>(user),
                Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsUser_WhenPasswordIsCorrect()
        {
            // Arrange
            var loginModel = new LoginRequestDTO
            {
                Email = "test@test.com",
                Password = "1234"
            };

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Phone = "0501234567",
                Email = "test@test.com"
            };

            // UserService משתמש ב-PasswordHasher אמיתי,
            // לכן חייבים לשמור Hash אמיתי במסד המדומה.
            var passwordHasher =
                new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

            user.Password =
                passwordHasher.HashPassword(user, loginModel.Password);

            _userRepositoryMock
                .Setup(x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.Login(loginModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user, result);

            _userRepositoryMock.Verify(
                x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var loginModel = new LoginRequestDTO
            {
                Email = "notfound@test.com",
                Password = "1234"
            };

            _userRepositoryMock
                .Setup(x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _service.Login(loginModel);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenPasswordIsIncorrect()
        {
            // Arrange
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
                Email = "test@test.com"
            };

            var passwordHasher =
                new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

            user.Password =
                passwordHasher.HashPassword(user, "CorrectPassword");

            _userRepositoryMock
                .Setup(x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.Login(loginModel);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                x => x.Login(
                    loginModel,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task MakeUserManager_ReturnsUserDTO()
        {
            // Arrange
            var id = 1;

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

            // Act
            var result = await _service.MakeUserManager(id);

            // Assert
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

            _mapperMock.Verify(
                x => x.Map<UserDTO>(user),
                Times.Once);
        }

        [Fact]
        public async Task MakeUserManager_UserNotFound_ReturnsNull()
        {
            // Arrange
            var id = 999;

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

            // Act
            var result = await _service.MakeUserManager(id);

            // Assert
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
            // Arrange
           var userDto = new RegisterRequestDTO { UserName = "TestUser", Phone = "0501234567", Email = "test@test.com", Password = "Password123!" };
            var user = new User { UserName = "TestUser", Phone = "0501234567", Email = "test@test.com", Password = "Password123!" };
            var addedUser = new User { Id = 1, UserName = "TestUser", Phone = "0501234567", Email = "test@test.com" };
            _mapperMock .Setup(x => x.Map<User>(userDto)) 
                .Returns(user); _userRepositoryMock
                .Setup(x => x.Add( It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(addedUser); _userRepositoryMock
                .Setup(x => x.Save(It.IsAny<CancellationToken>())) .Returns(Task.CompletedTask); 
            _mapperMock .Setup(x => x.Map<UserLogDTO>(addedUser)) .Returns(new UserLogDTO());
            // Act
            await _service.AddUser(userDto); 
            // Assert
            Assert.NotEqual("Password123!", user.Password);
            Assert.NotNull(user.Password); _userRepositoryMock.Verify( x => x.Add( It.Is<User>(u => u.Password != "Password123!"),
                It.IsAny<CancellationToken>()), Times.Once); }
        }
}
