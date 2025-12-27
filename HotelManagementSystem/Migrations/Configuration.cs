namespace HotelManagementSystem.Migrations
{
    using HotelManagementSystem.Enums;
    using HotelManagementSystem.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows;

    internal sealed class Configuration : DbMigrationsConfiguration<HotelManagementSystem.Models.HotelDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(HotelManagementSystem.Models.HotelDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            // Adaugare administrator in baza de date
            context.Users.AddOrUpdate(u => u.Username, new User
            {
                Username = "admin",
                Password = "1234", // In productie, parolele trebuie sa fie hashuite
                FullName = "Administrator de Sistemm",
                Role = UserRole.Administrator
            });


            // Generez 60 de camere pentru hotel (5 etaje, 12 camere pe etaj)

            for (int floor = 1; floor <= 5; floor++)
            {
                for (int roomNumber = 1; roomNumber <= 12; roomNumber++)
                {
                    string nrCameraString = (floor * 100 + roomNumber).ToString();
                    context.Rooms.AddOrUpdate(r => r.RoomNumber, new Room
                    {
                        RoomNumber = nrCameraString,
                        Floor = floor,
                        PricePerNight = 200,         //pretul default
                        Type = (roomNumber > 8) ? RoomType.Triple : RoomType.Double,
                        Status = RoomStatus.Free
                    });
                }

            }


            // --- 1. POPULARE MENIU RESTAURANT ---
            context.MenuItems.AddOrUpdate(m => m.Name,
                // Mic Dejun (Preț 0, inclus)
                new Models.MenuItem { Name = "Mic Dejun Continental", Category = "Breakfast", Price = 0, InternalCost = 25, IsIncludedInStay = true },

                // Prânz
                new Models.MenuItem { Name = "Ciorbă de văcuță", Category = "Lunch", Price = 25, InternalCost = 10, IsIncludedInStay = false },
                new Models.MenuItem { Name = "Meniu Prânz (Pui cu cartofi)", Category = "Lunch", Price = 35, InternalCost = 20, IsIncludedInStay = false },
                new Models.MenuItem { Name = "Paste Carbonara", Category = "Lunch", Price = 30, InternalCost = 20, IsIncludedInStay = false },

                // Cină
                new Models.MenuItem { Name = "Friptură de porc la grătar", Category = "Dinner", Price = 45, InternalCost = 30, IsIncludedInStay=false },
                new Models.MenuItem { Name = "Salată Caesar", Category = "Dinner", Price = 35, InternalCost = 25, IsIncludedInStay = false},
                new Models.MenuItem { Name = "Pește cu legume", Category = "Dinner", Price = 50, InternalCost = 32, IsIncludedInStay = false}
            );

            // --- 2. POPULARE SERVICII SPA ---
            context.SpaServices.AddOrUpdate(s => s.Name,
                new SpaService { Name = "Saună Finlandeză", PricePerPerson = 50, MaxCapacityPerSlot = 10 },
                new SpaService { Name = "Masaj de relaxare", PricePerPerson = 120, MaxCapacityPerSlot = 10 },
                new SpaService { Name = "Hammam (Baie Turcească)", PricePerPerson = 80, MaxCapacityPerSlot = 10 }
            );

            // Salvează modificările
            context.SaveChanges();








        }
    }
}
