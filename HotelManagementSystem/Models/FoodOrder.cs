using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class FoodOrder
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }
        public DateTime OrderDate { get; set; }    

        public int MenuItemId { get; set; }
        public virtual MenuItem MenuItem { get; set; }

        public string MealType { get; set; } //Mic dejun, pranz, cina
        public string FoodDetails { get; set; } //detalii despre mancare (2 X Ciorba, 3 x friptura etc)
        public decimal Cost { get; set; }

    }
}
