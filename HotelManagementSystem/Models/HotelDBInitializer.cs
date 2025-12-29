using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace HotelManagementSystem.Models
{
    // Folosim DropCreateDatabaseAlways pentru a fi siguri ca apare Sonia Onofrei
    // Schimb Drop... cu CreateDatabaseIfNotExists ca sa am persistenta la inchiderea aplicatiei
    public class HotelDbInitializer : CreateDatabaseIfNotExists<HotelDBContext>
    {
        protected override void Seed(HotelDBContext context)
        {
            // =============================================================
            // 1. MENIU & SPA
            // =============================================================
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Name = "Mic Dejun Continental", Category = "Breakfast", Price = 0, InternalCost = 15, IsIncludedInStay = true },
                new MenuItem { Name = "Omletă Țărănească", Category = "Breakfast", Price = 0, InternalCost = 12, IsIncludedInStay = true },
                new MenuItem { Name = "Ciorbă Rădăuțeană", Category = "Lunch", Price = 28, InternalCost = 10, IsIncludedInStay = false },
                new MenuItem { Name = "Paste Carbonara", Category = "Lunch", Price = 35, InternalCost = 15, IsIncludedInStay = false },
                new MenuItem { Name = "Mușchi de Vită", Category = "Dinner", Price = 85, InternalCost = 45, IsIncludedInStay = false },
                new MenuItem { Name = "Salată Caesar", Category = "Dinner", Price = 45, InternalCost = 20, IsIncludedInStay = false }
            };
            context.MenuItems.AddRange(menuItems);

            var spaServices = new List<SpaService>
            {
                new SpaService { Name = "Saună Finlandeză", PricePerPerson = 50, MaxCapacityPerSlot = 4 },
                new SpaService { Name = "Masaj Relaxare", PricePerPerson = 150, MaxCapacityPerSlot = 2 }
            };
            context.SpaServices.AddRange(spaServices);

            // =============================================================
            // 2. CAMERE
            // =============================================================
            var rooms = new List<Room>();
            for (int floor = 1; floor <= 4; floor++)
            {
                for (int r = 1; r <= 10; r++)
                {
                    rooms.Add(new Room
                    {
                        RoomNumber = string.Format("{0}{1:00}", floor, r),
                        Floor = floor,
                        Type = (r > 8) ? RoomType.Triple : RoomType.Double,
                        PricePerNight = 220,
                        Status = RoomStatus.Free
                    });
                }
            }
            context.Rooms.AddRange(rooms);

            // =============================================================
            // 3. UTILIZATORI (SONIA ONOFREI AICI)
            // =============================================================
            var users = new List<User>
            {
                // Staff
                new User { Username = "admin", Password = "1234", FullName = "Admin General", Role = UserRole.Administrator },
                new User { Username = "recep", Password = "1234", FullName = "Recepție Staff", Role = UserRole.Receptionist },
                new User { Username = "clean", Password = "1234", FullName = "Camerista Etaj 3", Role = UserRole.Cleaning, AssignedFloor = 3 },

                // --- CLIENTUL TARGET ---
                new User { Username = "sonia.onofrei", Password = "1234", FullName = "Sonia Onofrei", Role = UserRole.Client }
            };
            context.Users.AddRange(users);

            context.SaveChanges(); // Salvăm ca să avem ID-uri

            // =============================================================
            // 4. REZERVAREA SONIEI (ACTIVE, CONFIRMATĂ)
            // =============================================================

            var soniaUser = users[3]; // sonia.onofrei
            var soniaRoom = rooms[24]; // Camera 305 (Etaj 3)

            // Setăm datele cerute (folosim DateTime explicit pentru a fi siguri)
            // 23 Decembrie 2025 -> 31 Decembrie 2025
            DateTime checkIn = new DateTime(2025, 12, 23);
            DateTime checkOut = new DateTime(2025, 12, 31);

            // Important: Setăm camera ca Ocupată pentru ca Harta Recepției să fie corectă
            soniaRoom.Status = RoomStatus.Occupied;

            var soniaRes = new Reservation
            {
                UserId = soniaUser.Id,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                Status = ReservationStatus.Active, // CONFIRMATĂ DE RECEPȚIE
                NrPersons = 2,
                TotalPrice = (decimal)(checkOut - checkIn).TotalDays * soniaRoom.PricePerNight,
                Rooms = new List<Room> { soniaRoom },

                // Îi adăugăm și o mică comandă anterioară ca să nu fie gol istoricul
                FoodOrders = new List<FoodOrder>
                {
                    new FoodOrder { MenuItemId = 1, MealType = "Breakfast", FoodDetails = "Mic Dejun Continental", Cost = 0, OrderDate = new DateTime(2025, 12, 24, 09, 30, 0) }
                }
            };

            context.Reservations.Add(soniaRes);

            // Notificare de bun venit
            context.Notifications.Add(new Notification
            {
                UserId = soniaUser.Id,
                Message = "Bine ați venit, Doamna Onofrei! Camera 305 este pregătită.",
                CreatedAt = checkIn,
                IsRead = false
            });

            // Salvăm totul
            context.SaveChanges();
            base.Seed(context);
        }
    }
}