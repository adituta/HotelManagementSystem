using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Models
{
    internal class HotelDBContext : DbContext
    {
        public HotelDBContext() : base("name=HotelDBConnectionString")
        {
        }
        public DbSet<Utilizator> Utilizatori { get; set; }
        public DbSet<Camera> Camere { get; set; }
        public DbSet<Rezervare> rezervari { get; set; }


        //adaugam retul tabelelor care mai sunt de adaugat
    }
}
