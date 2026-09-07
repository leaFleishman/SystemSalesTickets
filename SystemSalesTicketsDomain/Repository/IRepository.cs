

namespace SystemSalesTickets.Core.Repository
{
    public interface IRepository< T> where T : class
    {
        Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default);
        Task<T> GetById(int id, CancellationToken cancellationToken = default);
        Task<T> Add(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task Save(CancellationToken cancellationToken = default);
        public Task Update(T entity, CancellationToken cancellationToken = default);
    }
}
