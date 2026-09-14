
using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    public class RegisterRequestDTO
    {
        public string UserName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }




    }
}
