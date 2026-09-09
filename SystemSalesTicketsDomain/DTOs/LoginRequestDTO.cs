using System.ComponentModel.DataAnnotations;
namespace SystemSalesTickets.Core.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }
}
