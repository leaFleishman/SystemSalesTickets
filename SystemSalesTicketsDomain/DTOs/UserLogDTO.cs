using System.ComponentModel.DataAnnotations;


namespace SystemSalesTickets.Core.DTOs
{
    public class UserLogDTO
    {

        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Key]
        public int Id { get; set; }
    }
}
