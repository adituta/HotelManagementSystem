using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class SalaryPayment
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        // Optional: details like "Salariu Ianuarie 2026"
        public string Details { get; set; }
    }
}
