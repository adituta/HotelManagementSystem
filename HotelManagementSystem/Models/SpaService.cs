using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class SpaService
    {
        public int Id { get; set; }
        public string Name { get; set; }           //Numele serviciului (Sauna, Masaj etc)
        public decimal PricePerPerson { get; set; } //Pret per persoana
        public int MaxCapacityPerSlot { get; set; } //Capacitatea maxima per slot orar
    }
}
