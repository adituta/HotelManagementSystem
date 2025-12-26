using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class Review
    {
        public int Id { get;set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; } //1-5 stele
        public DateTime Date { get; set; }
    }
}
