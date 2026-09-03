using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        private readonly DataContext _dataContext;

        public SeatRepository(DataContext dataContext)
            : base(dataContext)
        {
        }


    }
}
