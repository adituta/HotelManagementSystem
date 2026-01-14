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
            get { return _selectedRoomType; }
            set
            {
                _selectedRoomType = value;
                OnPropertyChanged("SelectedRoomType");
            }
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NrPersons { get; set; }
        public List<RoomType> RoomTypes { get { return Enum.GetValues(typeof(RoomType)).Cast<RoomType>().ToList(); } }

        private ObservableCollection<Room> _availableRooms;
        public ObservableCollection<Room> AvailableRooms
        {
            get { return _availableRooms; }
            set { _availableRooms = value; OnPropertyChanged("AvailableRooms"); }
        }

        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand BookCommand { get; private set; }
        private User _client;

        public DateTime MinDate { get; set; } = DateTime.Today; // Added for UI constraint

        public MakeReservationViewModel(User client)
        {
            StartDate = DateTime.Today; // Reset to Today
            EndDate = DateTime.Today.AddDays(1);
            NrPersons = 1;
            _client = client;
            SearchCommand = new RelayCommand(o => ExecuteSearch());
            BookCommand = new RelayCommand(room => ExecuteBook(room as Room));
        }

        private void ExecuteSearch()
        {
            // Validare Data
            if (StartDate < DateTime.Today)
            {
                MessageBoxHelper.Show("Nu puteți selecta o dată din trecut!", "Eroare Dată");
                StartDate = DateTime.Today; // Reset check
                return;
            }

            if (EndDate <= StartDate)
            {
                MessageBoxHelper.Show("Data de plecare trebuie să fie după data sosirii (minim 1 noapte)!", "Eroare Dată");
                return;
            }

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
                // CRITICAL FIX: Daca rezervarea incepe AZI, camera trebuie sa fie 'Free' acum. 
                // Daca e 'CleaningRequired', nu o putem da clientului instant.
                bool isStartingToday = StartDate.Date == DateTime.Today;

                var freeRooms = db.Rooms
                    .Where(r => r.Type == SelectedRoomType && !busyRoomIds.Contains(r.Id))
                    .ToList(); // Aducem in memorie pentru filtrare complexa (Status check)

                if (isStartingToday)
                {
                    freeRooms = freeRooms.Where(r => r.Status == RoomStatus.Free).ToList();
                }

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

                MessageBoxHelper.Show("Rezervarea a fost trimisă spre confirmare la recepție!", "Rezervare Trimisă");
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
