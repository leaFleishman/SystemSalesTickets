using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DataContext context)
            : base(context)
        {
        }

        public async Task<User> Login(LoginModel loginModel)
        {

            return await _dbSet.FirstOrDefaultAsync(u =>
                u.Email == loginModel.Email &&
                u.Password == loginModel.Password);
        }

        public async Task<User> MakeUserManager(int id)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return null;
            user.Role = UserRole.Manager;
            await Update(user);
            return user;
        }
    }
}
