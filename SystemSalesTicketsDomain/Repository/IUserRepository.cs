

using SystemSalesTickets.Core.Repository;
using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsCore.Repository
{
    public interface IUserRepository :IRepository<User>
    {

        Task<User> Add(User user);
        Task<IEnumerable<User>> GetAll();
        Task<User?> GetById(int id);
    }
}
