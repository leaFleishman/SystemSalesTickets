using AutoMapper;
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

        private static int counter = new Random().Next();
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDTO> AddUser(UserDTO user)
        {
            var tmp = _mapper.Map<User>(user);
            tmp.Id = counter++;
            var res = await _userRepository.Add(tmp);
            return _mapper.Map<UserDTO>(res);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsers()
        {
            var res = await _userRepository.GetAll();
            return _mapper.Map<IEnumerable<UserDTO>>(res);
        }

        public async Task<UserDTO> GetUserById(int id)
        {
            var res = await _userRepository.GetById(id);
            return _mapper.Map<UserDTO>(res);
        }

        public async Task<User> Login(LoginModel loginModel)
        {
          return  await _userRepository.Login(loginModel);
        }
    }
}

