using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public  string Username { get; set; }
        public string Password { get; set; }    //in productie, aceste parole sunt hashuite
        public string FullName { get; set; }
        public UserRole Role { get; set; }

        //date care vin in ajutorul cameristelor (nivelul 1 2 3 sau 4)
        public int? AssignedFloor { get; set; } 
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
