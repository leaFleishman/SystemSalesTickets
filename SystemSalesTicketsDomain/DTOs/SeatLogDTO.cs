using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemSalesTickets.Core.DTOs
{
    public class SeatLogDTO
    {
        public int Row { get; set; }

        public int Line { get; set; }

        public int Id { get; set; }
        public bool IsAvailable { get; set; }
    }
}
