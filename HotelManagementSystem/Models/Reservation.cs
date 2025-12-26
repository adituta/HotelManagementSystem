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

        //O rezervare poate avea mai multe camere
        public virtual ICollection<Room> Rooms { get; set; }

        //servicii  extra comandate pe parcursul sedereii (mese in fiecare zi, spa)
        public virtual ICollection<SpaAppointment>SpaAppointments { get; set; }
        public virtual ICollection<FoodOrder> FoodOrders { get; set; }

        //recenzia de la final
        public string ReviewComment { get; set; } 
        public int? ReviewRating { get; set; } //1-5 stele

    }
}
