using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface ISeatService
    {
        Task<IEnumerable<SeatDTO>> GetAll();
        Task<SeatLogDTO> GetById(int id);
        Task<SeatLogDTO> Add(SeatDTO seat);
    }
}
