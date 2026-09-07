using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IUserRepository : IRepository<User>
    {

        Task<User> Add(User user);
        Task<IEnumerable<User>> GetAll();
        Task<User> GetById(int id);
        Task<User> Login(LoginModel loginModel);
        Task<User> MakeUserManager(int id);
    }
}
