using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItRoots.Data.Models
{
    class Invoice
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        // Optionally store a total, status, etc. 
        // public decimal Total { get; set; }
        // public string Status { get; set; }
    }
}
