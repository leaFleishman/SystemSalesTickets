using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsInfrastructure;

namespace SystemSalesTicketsData
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DataContext context)
            : base(context)
        {
        }
    }
}
