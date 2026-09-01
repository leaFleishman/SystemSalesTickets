using SystemSalesTickets.Core.Repository;
using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsPresentation.Interfaces
{
    public interface ISeatService
    {
        Task<IEnumerable<Seat>> GetAll();
        Task<Seat?> GetById(int id);
        Task<Seat> Add(Seat seat);
        Task<Seat> Update(Seat seat);
    }
}
