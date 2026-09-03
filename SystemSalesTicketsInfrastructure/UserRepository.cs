using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.DTOs;
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
        
        public async Task< User>  Login(LoginModel loginModel)
        {
            return await _dbSet.FirstOrDefaultAsync(u =>
                u.Email == loginModel.Email &&
                u.Password == loginModel.Password);
        }
    }
}
