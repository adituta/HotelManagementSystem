using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } //Pranz si Cina
        public decimal Price { get; set; }
        public decimal InternalCost { get; set; } //costul pe care il vedee hotelul (Adminul)

        public bool IsIncludedInStay { get; set; } //daca este inclus in pachetul de cazare (mic dejun inclus etc)
    }
}
