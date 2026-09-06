using SystemSalesTickets.Core.Enums;

namespace SystemSalesTickets.Core.Models
{
    public class User
    {

        public int Id { get; set; }

        public string UserName { get; set; }

        public UserRole Role { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
