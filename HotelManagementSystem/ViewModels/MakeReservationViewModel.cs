using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelManagementSystem.Helpers;
using HotelManagementSystem.Enums;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Windows;


namespace HotelManagementSystem.ViewModels
{
    public class MakeReservationViewModel: BaseViewModel
    {
        private RoomType _selectedRoomType;
        public RoomType SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                _selectedRoomType = value;
                OnPropertyChanged(nameof(SelectedRoomType));
            }
        }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
        public int NrPersons { get; set; } = 1;
        public List<RoomType> RoomTypes => Enum.GetValues(typeof(RoomType)).Cast<RoomType>().ToList();

        private ObservableCollection<Room> _availableRooms;
        public ObservableCollection<Room> AvailableRooms
        {
            get => _availableRooms;
            set { _availableRooms = value; OnPropertyChanged(nameof(AvailableRooms)); }
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand BookCommand { get; }
        private User _client;

        public MakeReservationViewModel(User client)
        {
            _client = client;
            SearchCommand = new RelayCommand(o => ExecuteSearch());
            BookCommand = new RelayCommand(room => ExecuteBook(room as Room));
        }

        // În interiorul MakeReservationViewModel.cs

        private void ExecuteSearch()
        {
            using (var db = new HotelDBContext())
            {
                // 1. Găsim rezervările care se suprapun cu perioada selectată
                var busyRoomIds = db.Reservations
                    .Where(res => res.Status != ReservationStatus.Cancelled &&
                                 (StartDate < res.CheckOutDate && EndDate > res.CheckInDate))
                    .SelectMany(res => res.Rooms) // Luăm toate camerele din acele rezervări
                    .Select(r => r.Id)
                    .Distinct()
                    .ToList();

                // 2. Filtrăm camerele disponibile de tipul selectat
                var freeRooms = db.Rooms
                    .Where(r => r.Type == SelectedRoomType && !busyRoomIds.Contains(r.Id))
                    .ToList();

                AvailableRooms = new ObservableCollection<Room>(freeRooms);
            }
        }

        private void ExecuteBook(Room room)
        {
            if (room == null) return;

            using (var db = new HotelDBContext())
            {
                var newReservation = new Reservation
                {
                    UserId = _client.Id,
                    CheckInDate = StartDate,
                    CheckOutDate = EndDate,
                    NrPersons = NrPersons,
                    Status = ReservationStatus.Pending,
                    TotalPrice = (decimal)(EndDate - StartDate).TotalDays * room.PricePerNight,
                    Rooms = new List<Room>()
                };

                // Găsim camera în DB și o adăugăm în listă
                var roomFromDb = db.Rooms.Find(room.Id);
                if (roomFromDb != null) newReservation.Rooms.Add(roomFromDb);

                db.Reservations.Add(newReservation);
                db.SaveChanges();

                MessageBox.Show("Rezervarea a fost trimisă spre confirmare la recepție!");
                ExecuteSearch();
            }
        }

        // Funcție simplă de calcul preț
        private decimal CalculateTotalPrice(Room room)
        {
            int days = (EndDate - StartDate).Days;
            if (days <= 0) days = 1;
            return room.PricePerNight * days;
        }
    }
}
