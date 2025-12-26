using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class SpaAppointment
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }

        public int SpaServiceId { get; set; }
        public virtual SpaService SpaService { get; set; }

        public ServiceType ServiceType { get; set; }        //Sauna sau Masaj
        public DateTime AppointmentDate { get; set; }       //Ziua
        public TimeSpan StartTime { get; set; }             //Ora de inceput
        public int PersonsCount { get; set; }               //Numar de persoane
        public bool IsConfirmed { get; set; }               //Daca rezervarea este confirmata (acceptata de personalul de la Spa)

    }
}
