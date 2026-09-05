

namespace SystemSalesTickets.Core.Repository
{
    public interface IRepository< T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task<T> Add(T entity);
    }
}
