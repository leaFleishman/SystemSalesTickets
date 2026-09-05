using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.DTOs
{
    public class OrderLogDTO
    {
        public string EventName { get; set; }

        public DateTime OrderDate { get; set; }

        public EventDTO EventDTO { get; set; }

        public Seat SeatDTO { get; set; }

        public int Id { get; set; }
    }
}
