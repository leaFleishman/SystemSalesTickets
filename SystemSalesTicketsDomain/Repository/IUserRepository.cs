using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IUserRepository : IRepository<User>
    {

        Task<User> Add(User user, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAll(CancellationToken cancellationToken = default);
        Task<User> GetById(int id, CancellationToken cancellationToken = default);
        Task<User> Login(LoginModel loginModel, CancellationToken cancellationToken = default);
        Task<User> MakeUserManager(int id, CancellationToken cancellationToken = default);
    }
}
