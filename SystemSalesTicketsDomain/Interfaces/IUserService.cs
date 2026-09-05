using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IUserService
    {

        Task<UserLogDTO> AddUser(UserDTO user);
        Task<IEnumerable<UserDTO>> GetAllUsers();
        Task<UserLogDTO> GetUserById(int id);
        Task<User> Login(LoginModel loginModel);


    }
}
