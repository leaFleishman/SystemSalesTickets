using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IUserService
    {

        Task<UserLogDTO> AddUser(UserDTO user, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDTO>> GetAllUsers(CancellationToken cancellationToken = default);
        Task<UserLogDTO> GetUserById(int id, CancellationToken cancellationToken = default);
        Task<User> Login(LoginModel loginModel, CancellationToken cancellationToken = default);
        Task<UserDTO> MakeUserManager(int id, CancellationToken cancellationToken = default);

    }
}
