using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public int NrPersons { get; set; } // Adăugat pentru bucătărie

        // Relație Many-to-Many simplă (fără tabel de legătură vizibil)
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        public virtual ICollection<SpaAppointment> SpaAppointments { get; set; }
        public virtual ICollection<FoodOrder> FoodOrders { get; set; }

        public string ReviewComment { get; set; }
        public int? ReviewRating { get; set; }
    }
}
