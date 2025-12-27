namespace HotelManagementSystem.Migrations
{
    using HotelManagementSystem.Enums;
    using HotelManagementSystem.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<HotelManagementSystem.Models.HotelDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true; // Permitem migrarea automată pentru dezvoltare
            AutomaticMigrationDataLossAllowed = true; // Permitem modificări de structură chiar dacă se pierd date
        }

        protected override void Seed(HotelManagementSystem.Models.HotelDBContext context)
        {
            // ---------------------------------------------------------
            // 1. CURĂȚARE BAZĂ DE DATE (Stergere in ordine inversa a dependentelor)
            // ---------------------------------------------------------
            context.Database.ExecuteSqlCommand("DELETE FROM Notifications");
            context.Database.ExecuteSqlCommand("DELETE FROM FoodOrders");
            context.Database.ExecuteSqlCommand("DELETE FROM SpaAppointments");
            context.Database.ExecuteSqlCommand("DELETE FROM ReservationRooms");
            context.Database.ExecuteSqlCommand("DELETE FROM Reservations");
            context.Database.ExecuteSqlCommand("DELETE FROM Rooms");
            context.Database.ExecuteSqlCommand("DELETE FROM Users");
            context.Database.ExecuteSqlCommand("DELETE FROM MenuItems");
            context.Database.ExecuteSqlCommand("DELETE FROM SpaServices");

            // Resetare indecși de auto-incrementare (opțional, pentru estetică ID = 1)
            try
            {
                context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Users', RESEED, 0)");
                context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Reservations', RESEED, 0)");
            }
            catch { /* Ignorăm eroarea dacă nu suntem pe SQL Server standard */ }

            // ---------------------------------------------------------
            // 2. ADĂUGARE NOMENCLATOARE (Meniu, SPA, Camere)
            // ---------------------------------------------------------

            // --- Meniu Restaurant ---
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Name = "Mic Dejun Continental", Category = "Breakfast", Price = 0, InternalCost = 15, IsIncludedInStay = true },
                new MenuItem { Name = "Omletă cu șuncă", Category = "Breakfast", Price = 0, InternalCost = 10, IsIncludedInStay = true },

                new MenuItem { Name = "Ciorbă de văcuță", Category = "Lunch", Price = 25, InternalCost = 10, IsIncludedInStay = false },
                new MenuItem { Name = "Paste Carbonara", Category = "Lunch", Price = 32, InternalCost = 12, IsIncludedInStay = false },
                new MenuItem { Name = "Burger Vita Black Angus", Category = "Lunch", Price = 45, InternalCost = 25, IsIncludedInStay = false },

                new MenuItem { Name = "Somon la grătar", Category = "Dinner", Price = 55, InternalCost = 30, IsIncludedInStay = false },
                new MenuItem { Name = "Mușchi de vită", Category = "Dinner", Price = 70, InternalCost = 40, IsIncludedInStay = false },
                new MenuItem { Name = "Salată Caesar", Category = "Dinner", Price = 35, InternalCost = 15, IsIncludedInStay = false }
            };
            context.MenuItems.AddOrUpdate(m => m.Name, menuItems.ToArray());

            // --- Servicii SPA ---
            var spaServices = new List<SpaService>
            {
                new SpaService { Name = "Saună Finlandeză", PricePerPerson = 50, MaxCapacityPerSlot = 4 },
                new SpaService { Name = "Masaj Relaxare (50 min)", PricePerPerson = 120, MaxCapacityPerSlot = 2 },
                new SpaService { Name = "Masaj Pietre Vulcanice", PricePerPerson = 150, MaxCapacityPerSlot = 2 },
                new SpaService { Name = "Jacuzzi Privat", PricePerPerson = 80, MaxCapacityPerSlot = 6 }
            };
            context.SpaServices.AddOrUpdate(s => s.Name, spaServices.ToArray());

            // --- Generare Camere (5 etaje) ---
            // Etaj 1: Camere 101-110 (Standard)
            // Etaj 2: Camere 201-210 (Standard)
            // ...
            for (int floor = 1; floor <= 5; floor++)
            {
                for (int roomNum = 1; roomNum <= 10; roomNum++)
                {
                    string rNumber = $"{floor}{roomNum:00}"; // ex: 101, 205
                    var rType = (roomNum > 8) ? RoomType.Triple : RoomType.Double;
                    decimal price = (floor == 5) ? 350 : (rType == RoomType.Double ? 200 : 280); // Etaj 5 e VIP

                    context.Rooms.AddOrUpdate(r => r.RoomNumber, new Room
                    {
                        RoomNumber = rNumber,
                        Floor = floor,
                        Type = rType,
                        PricePerNight = price,
                        Status = RoomStatus.Free // Default free, le ocupăm mai jos prin rezervări
                    });
                }
            }
            context.SaveChanges(); // Salvăm ca să avem ID-urile camerelor disponibile

            // ---------------------------------------------------------
            // 3. ADĂUGARE UTILIZATORI (ANGAJAȚI + CLIENȚI)
            // ---------------------------------------------------------

            var users = new List<User>
            {
                // Admin
                new User { Username = "admin", Password = "123", FullName = "Director General", Role = UserRole.Administrator },
                
                // Staff
                new User { Username = "recep", Password = "123", FullName = "Elena Receptioner", Role = UserRole.Receptionist },
                new User { Username = "clean1", Password = "123", FullName = "Maria Camerista (Et 1)", Role = UserRole.Cleaning, AssignedFloor = 1 },
                new User { Username = "clean2", Password = "123", FullName = "Ioana Camerista (Et 2)", Role = UserRole.Cleaning, AssignedFloor = 2 },
                new User { Username = "chef", Password = "123", FullName = "Chef Scarlatescu", Role = UserRole.Cook }, // Atentie la typo-ul din Enum 'Coook'
                new User { Username = "spa", Password = "123", FullName = "Terapeut SPA", Role = UserRole.SpaStaff },

                // Clienți
                new User { Username = "client1", Password = "123", FullName = "Ion Popescu (Activ)", Role = UserRole.Client },
                new User { Username = "client2", Password = "123", FullName = "Andreea Marin (Istoric)", Role = UserRole.Client },
                new User { Username = "client3", Password = "123", FullName = "Gigel Viitor (Pending)", Role = UserRole.Client }
            };

            foreach (var u in users)
                context.Users.AddOrUpdate(x => x.Username, u);

            context.SaveChanges();

            // Preluăm entitățile din DB pentru a face legături (Foreign Keys)
            var dbClientActive = context.Users.FirstOrDefault(u => u.Username == "client1");
            var dbClientPast = context.Users.FirstOrDefault(u => u.Username == "client2");
            var dbClientPending = context.Users.FirstOrDefault(u => u.Username == "client3");

            var room101 = context.Rooms.FirstOrDefault(r => r.RoomNumber == "101");
            var room102 = context.Rooms.FirstOrDefault(r => r.RoomNumber == "102");
            var room205 = context.Rooms.FirstOrDefault(r => r.RoomNumber == "205");

            var burger = context.MenuItems.FirstOrDefault(m => m.Name.Contains("Burger"));
            var massage = context.SpaServices.FirstOrDefault(s => s.Name.Contains("Relaxare"));

            // ---------------------------------------------------------
            // 4. CREARE REZERVĂRI ȘI SCENARII
            // ---------------------------------------------------------

            // SCENARIU 1: Client ACTIV (Ion Popescu) - Cazat acum
            // Are o cameră ocupată, o comandă la restaurant și o programare SPA
            if (dbClientActive != null && room101 != null)
            {
                // Setăm camera ca ocupată
                room101.Status = RoomStatus.Occupied;

                var resActive = new Reservation
                {
                    UserId = dbClientActive.Id,
                    CheckInDate = DateTime.Now.AddDays(-1), // A venit ieri
                    CheckOutDate = DateTime.Now.AddDays(2), // Pleacă peste 2 zile
                    Status = ReservationStatus.Active,
                    NrPersons = 2,
                    TotalPrice = 600, // Estimativ
                    Rooms = new List<Room> { room101 },
                    FoodOrders = new List<FoodOrder>
                    {
                        new FoodOrder { MenuItemId = burger.Id, MealType = "Lunch", FoodDetails = "1x Burger", Cost = burger.Price, OrderDate = DateTime.Now }
                    },
                    SpaAppointments = new List<SpaAppointment>
                    {
                        new SpaAppointment { SpaServiceId = massage.Id, AppointmentDate = DateTime.Today, StartTime = new TimeSpan(14,0,0), PersonsCount = 1, IsConfirmed = true }
                    }
                };
                context.Reservations.Add(resActive);

                // Trimitem o notificare
                context.Notifications.Add(new Notification
                {
                    UserId = dbClientActive.Id,
                    Message = "Bun venit! Vă dorim un sejur plăcut.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            // SCENARIU 2: Client ISTORIC (Andreea Marin) - A plecat deja
            // Cameră liberă acum, dar murdară
            if (dbClientPast != null && room102 != null)
            {
                room102.Status = RoomStatus.CleaningRequired; // Necesită curățenie

                var resPast = new Reservation
                {
                    UserId = dbClientPast.Id,
                    CheckInDate = DateTime.Now.AddDays(-10),
                    CheckOutDate = DateTime.Now.AddDays(-5),
                    Status = ReservationStatus.Completed,
                    NrPersons = 1,
                    TotalPrice = 1000,
                    ReviewComment = "Totul a fost excelent, mâncarea foarte bună!",
                    ReviewRating = 5,
                    Rooms = new List<Room> { room102 }
                };
                context.Reservations.Add(resPast);
            }

            // SCENARIU 3: Rezervare PENDING (Gigel) - Urmează să vină
            // Cameră liberă momentan
            if (dbClientPending != null && room205 != null)
            {
                var resPending = new Reservation
                {
                    UserId = dbClientPending.Id,
                    CheckInDate = DateTime.Now.AddDays(5),
                    CheckOutDate = DateTime.Now.AddDays(7),
                    Status = ReservationStatus.Pending,
                    NrPersons = 2,
                    TotalPrice = 400,
                    Rooms = new List<Room> { room205 }
                };
                context.Reservations.Add(resPending);
            }

            // Salvăm toate modificările finale
            context.SaveChanges();
        }
    }
}