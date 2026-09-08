using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Models;

using MyApp.Application.Common.Models;

namespace SystemSalesTickets.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DataContext _datacontext;
        protected readonly DbSet<T> _dbSet;

        public Repository(DataContext context)
        {
            _datacontext = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> Add(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task Save(CancellationToken cancellationToken = default)
        {
            await _datacontext.SaveChangesAsync(cancellationToken);
        }

        public async Task Update(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
        }

        public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            _dbSet.Remove(entity);
            await Save(cancellationToken);
            return true;
        }



        public async Task<PagedResponse<T>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking();

            var count = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<T>(items, pageNumber, pageSize, count);
        }
        public async Task<T?> GetById(int id,
                 CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(
                new object[] { id },
                cancellationToken);
        }


    }
}
