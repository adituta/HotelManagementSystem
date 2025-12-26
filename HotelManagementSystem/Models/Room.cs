using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }          //ex: 101, 205
        public int Floor { get; set; }                  //nivelulrile 1-4
        public RoomType Type { get; set; }
        public decimal PricePerNight { get; set; }
        public RoomStatus Status { get; set; }          //starea curenta (pentru receptie si curatenie)
    }
}
