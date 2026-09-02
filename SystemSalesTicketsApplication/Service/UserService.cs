using AutoMapper;
using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Service.Service
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;

        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository,IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper=mapper;
        }

        public async Task<User> AddUser(User user)
        {
            return await _userRepository.Add(user);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAll();
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _userRepository.GetById(id);
        }
    }
}

