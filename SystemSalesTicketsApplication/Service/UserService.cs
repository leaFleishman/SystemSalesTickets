using AutoMapper;
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

        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository userRepository, IMapper mapper,ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger=logger;
        }

        public async Task<UserLogDTO> AddUser(UserDTO user, CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<User>(user);
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

        public async Task<User> Login(LoginModel loginModel, CancellationToken cancellationToken = default)
        {
            return await _userRepository.Login(loginModel, cancellationToken);
        }

        public async Task<UserDTO> MakeUserManager(int id, CancellationToken cancellationToken = default)
        {
            var user=await _userRepository.MakeUserManager(id, cancellationToken);
            await _userRepository.Save(cancellationToken);

            return _mapper.Map<UserDTO>(user);
        }
    }
}

