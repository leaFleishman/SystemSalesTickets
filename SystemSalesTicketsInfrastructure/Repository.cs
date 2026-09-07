using Microsoft.EntityFrameworkCore;
using System.Collections;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DataContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DataContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> Add(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            await Save(cancellationToken);
            return entity;
        }

        public async Task Save(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Update(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            await Save(cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
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



        public async Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetById(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        


    }
}
