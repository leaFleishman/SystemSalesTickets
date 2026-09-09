using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IUserService
    {

        Task<UserLogDTO> AddUser(RegisterRequestDTO user, CancellationToken cancellationToken = default);
        Task<PagedResponse<UserDTO>> GetAllUsers(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<UserLogDTO> GetUserById(int id, CancellationToken cancellationToken = default);
        Task<User> Login(LoginRequestDTO loginModel, CancellationToken cancellationToken = default);
        Task<UserDTO> MakeUserManager(int id, CancellationToken cancellationToken = default);

    }
}
