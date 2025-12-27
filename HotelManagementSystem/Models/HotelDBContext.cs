using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using HotelManagementSystem.Enums;
using HotelManagementSystem.Models;


namespace HotelManagementSystem.Models
{
    public class HotelDBContext : DbContext
    {
        public HotelDBContext() : base("name=HotelDBConnectionString") {
        //Pe masursa ce dezvolt proiecctul si contruiesc baza de date (adaug si sterg tabele)
        // Adica daca fac modificari in cod, sterge si refa baza de date automat cand pornesc aplicatia.
        Database.SetInitializer( new DropCreateDatabaseIfModelChanges<HotelDBContext>());
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationRoom> ReservationRooms { get; set; }
        public DbSet<SpaAppointment> SpaAppointments { get; set; }
        public DbSet<FoodOrder> FoodOrders { get; set; }
        public DbSet<Notification> Notifications { get; set; }  


        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<SpaService> SpaServices { get; set; }


        protected override void OnModelCreating (DbModelBuilder modelBuilder)
        {
            //se lasa moemntan gol
            base.OnModelCreating(modelBuilder);
        }


       
    }
}
