using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemSalesTickets.Core.DTOs
{
    public class SeatLogDTO
    {

        public int Id { get; set; }

        public int EventId { get; set; }

        public int SeatId { get; set; }

        public string? EventName { get; set; }

        public DateTime OrderDate { get; set; }

        public string? Message { get; set; }
    }
}
