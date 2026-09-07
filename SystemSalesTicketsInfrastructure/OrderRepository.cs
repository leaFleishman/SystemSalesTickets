
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {

        public OrderRepository(DataContext context)
            : base(context)
        {
        }

       

    }
}
