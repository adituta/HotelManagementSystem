using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Data.Entity;

namespace HotelManagementSystem.ViewModels
{
    public class ReceptionMapViewModel : BaseViewModel
    {
        public ObservableCollection<Room> AllRooms { get; set; }
        public ObservableCollection<User> AllClients { get; set; } // Pentru a selecta un client existent la recepție

        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get => _selectedRoom;
            set { _selectedRoom = value; OnPropertyChanged(nameof(SelectedRoom)); OnPropertyChanged(nameof(IsRoomSelected)); }
        }

        public bool IsRoomSelected => SelectedRoom != null && SelectedRoom.Status == RoomStatus.Free;

        // Date pentru rezervarea pe loc
        public User SelectedClient { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        public RelayCommand BookOnSpotCommand { get; }

        public ReceptionMapViewModel()
        {
            LoadData();
            BookOnSpotCommand = new RelayCommand(o => ExecuteBookOnSpot());
        }

        private void LoadData()
        {
            using (var db = new HotelDBContext())
            {
                AllRooms = new ObservableCollection<Room>(db.Rooms.OrderBy(r => r.RoomNumber).ToList());
                AllClients = new ObservableCollection<User>(db.Users.Where(u => u.Role == UserRole.Client).ToList());
                OnPropertyChanged(nameof(AllRooms));
                OnPropertyChanged(nameof(AllClients));
            }
        }

        private void ExecuteBookOnSpot()
        {
            if (SelectedRoom == null || SelectedClient == null)
            {
                MessageBox.Show("Selectați o cameră liberă și un client!");
                return;
            }

            using (var db = new HotelDBContext())
            {
                var newRes = new Reservation
                {
                    UserId = SelectedClient.Id,
                    CheckInDate = StartDate,
                    CheckOutDate = EndDate,
                    Status = ReservationStatus.Active, // Fiind la recepție, este direct activă
                    TotalPrice = (decimal)(EndDate - StartDate).TotalDays * SelectedRoom.PricePerNight,
                    Rooms = new List<Room>()
                };

                var dbRoom = db.Rooms.Find(SelectedRoom.Id);
                newRes.Rooms.Add(dbRoom);
                dbRoom.Status = RoomStatus.Occupied;

                db.Reservations.Add(newRes);
                db.SaveChanges();

                MessageBox.Show($"Cameră {SelectedRoom.RoomNumber} a fost rezervată pentru {SelectedClient.FullName}!");
                LoadData(); // Refresh grid
                SelectedRoom = null;
            }
        }
    }
}