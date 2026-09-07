using System.ComponentModel.DataAnnotations;
using SystemSalesTickets.Core.Enums;

namespace SystemSalesTickets.Core.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 3)]

        public string UserName { get; set; }

        public UserRole Role { get; set; }

        [ Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
