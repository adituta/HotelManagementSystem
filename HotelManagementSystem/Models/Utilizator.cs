using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class Utilizator
    {
        public int Id { get; set; }
        public string NumeUtilizator { get; set; }
        public string Parola { get; set; }
        public string Rol { get; set; } // admin, receptionist, etc.

        //orientativ, sa fie ceva aici, mai revenim daca e nevoie
        //asa fac si la celelalte tabele, don't worry
    }
}
