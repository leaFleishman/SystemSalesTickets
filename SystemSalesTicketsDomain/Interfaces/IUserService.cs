using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsPresentation.Interfaces
{
    public interface IUserService
    {

        Task<User> AddUser(User user);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User?> GetUserById(int id);
    }
}
