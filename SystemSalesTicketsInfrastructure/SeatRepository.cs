using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsInfrastructure;

namespace SystemSalesTicketsData
{
    public class SeatRepository: Repository<Seat>, ISeatRepository
    {
        private readonly DataContext _dataContext;

        public SeatRepository(DataContext dataContext)
            : base(dataContext)
        {
        }


    }
}
