
using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsInfrastructure;

namespace SystemSalesTicketsData
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {

        public OrderRepository(DataContext context)
            : base(context)
        {
        }

       

    }
}
