namespace HotelManagementSystem.Migrations
{
    using HotelManagementSystem.Enums;
    using HotelManagementSystem.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Windows.Controls;

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
        }
    }
}
