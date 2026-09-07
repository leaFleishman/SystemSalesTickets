

using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }
}
