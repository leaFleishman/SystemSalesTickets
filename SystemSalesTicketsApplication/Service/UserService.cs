using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserLogDTO> AddUser(RegisterRequestDTO user, CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<User>(user);
            tmp.Password = _passwordHasher.HashPassword(tmp, tmp.Password);
            var res = await _userRepository.Add(tmp, cancellationToken);
            await _userRepository.Save(cancellationToken);
            return _mapper.Map<UserLogDTO>(res);
        }

        public async Task<PagedResponse<UserDTO>> GetAllUsers(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {

            var result = await _userRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            return new PagedResponse<UserDTO>(
                _mapper.Map<IEnumerable<UserDTO>>(result.Data),
                result.PageNumber,
                result.PageSize,
                result.TotalRecords);


        }

        public async Task<UserLogDTO> GetUserById(int id, CancellationToken cancellationToken = default)
        {
            var res = await _userRepository.GetById(id, cancellationToken);
            return _mapper.Map<UserLogDTO>(res);
        }

        public async Task<User> Login(LoginRequestDTO loginModel, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.Login(loginModel, cancellationToken);

            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                loginModel.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        public async Task<UserDTO> MakeUserManager(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.MakeUserManager(id, cancellationToken);
            await _userRepository.Save(cancellationToken);

            return _mapper.Map<UserDTO>(user);
        }
    }
}

