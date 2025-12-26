using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class ReservationRoom
    {
        public int Id { get; set; }
        public virtual Reservation Reservation { get; set; }    
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public int Adults { get; set; } 
        public int Children { get; set; }
        public bool ExtraBedRequested { get; set; } //true daca am copil intre 0 si 15 ani si   doresc pat suplimentar
    }
}
