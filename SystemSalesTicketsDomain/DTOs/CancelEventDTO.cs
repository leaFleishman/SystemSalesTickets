using System.ComponentModel.DataAnnotations;

namespace SystemSalesTickets.Core.DTOs
{
    /// <summary>Body of PUT api/Event/{id}/cancel. The whole body is optional.</summary>
    public class CancelEventDTO
    {
        [StringLength(500, ErrorMessage = "Reason can be at most 500 characters")]
        public string? Reason { get; set; }
    }
}
