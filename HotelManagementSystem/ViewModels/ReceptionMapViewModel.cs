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
        public ObservableCollection<User> AllClients { get; set; }

        // --- PROPRIETĂȚI PENTRU REZERVARE PE LOC ---
        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get { return _selectedRoom; }
            set
            {
                _selectedRoom = value;
                OnPropertyChanged("SelectedRoom");
                OnPropertyChanged("IsRoomSelected");

                // Când selectăm o cameră, încărcăm calendarul ei
                if (_selectedRoom != null) LoadRoomSchedule(_selectedRoom.Id);
            }
        }

        public bool IsRoomSelected { get { return SelectedRoom != null; } }

        // Selecție client existent
        private User _selectedClient;
        public User SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                OnPropertyChanged("SelectedClient");
                // Dacă selectăm un client existent, ștergem câmpurile de client nou
                if (value != null) IsNewClientMode = false;
            }
        }

        // --- CLIENT NOU (Dacă nu are cont) ---
        private bool _isNewClientMode;
        public bool IsNewClientMode
        {
            get { return _isNewClientMode; }
            set
            {
                _isNewClientMode = value;
                OnPropertyChanged("IsNewClientMode");
                if (value) SelectedClient = null; // Deselectăm clientul existent
            }
        }

        public string NewClientFullName { get; set; }
        public string NewClientPhone { get; set; } // Folosit ca username temporar

        // Date calendaristice
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // --- LISTA DE DATE OCUPATE (Pentru Calendar) ---
        // Vom trimite această listă către View pentru a bloca datele
        public List<Reservation> RoomFutureReservations { get; set; }

        // Eveniment pentru a notifica View-ul să redeseneze calendarul
        public event Action<List<Reservation>> OnScheduleLoaded;

        public RelayCommand BookOnSpotCommand { get; private set; }
        public RelayCommand SetRoomCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        public ReceptionMapViewModel()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);

            LoadData();
            LoadData();
            BookOnSpotCommand = new RelayCommand(o => ExecuteBookOnSpot());
            RefreshCommand = new RelayCommand(o => LoadData());

            SetRoomCommand = new RelayCommand(obj =>
            {
                var r = obj as Room;
                if (r != null)
                {
                    SelectedRoom = r;
                }
            });
        }

        private void LoadData()
        {
            using (var db = new HotelDBContext())
            {
                // 1. Încărcăm lista de camere brută
                var roomsList = db.Rooms.OrderBy(r => r.RoomNumber).ToList();

                // 2. Căutăm ID-urile camerelor care sunt ocupate ASTĂZI
                // O cameră e ocupată dacă există o rezervare ACTIVĂ unde:
                // CheckIn <= Azi ȘI CheckOut > Azi
                var today = DateTime.Today;

                var occupiedRoomIds = db.Reservations
                    .Where(r => r.Status == ReservationStatus.Active &&
                                r.CheckInDate <= today &&
                                r.CheckOutDate > today)
                    .SelectMany(r => r.Rooms)
                    .Select(r => r.Id)
                    .ToList();

                // 3. Actualizăm statusul DOAR ÎN MEMORIE (pentru afișare corectă pe hartă)
                // Nu salvăm în DB, doar colorăm interfața
                foreach (var room in roomsList)
                {
                    // Dacă camera este în lista celor ocupate, o facem Roșie
                    if (occupiedRoomIds.Contains(room.Id))
                    {
                        room.Status = RoomStatus.Occupied;
                    }
                    // Altfel, o lăsăm cum e în baza de date (Free, CleaningRequired, etc.)
                }

                // 4. Trimitem lista către interfață
                AllRooms = new ObservableCollection<Room>(roomsList);

                // Încărcăm și clienții
                AllClients = new ObservableCollection<User>(db.Users.Where(u => u.Role == UserRole.Client).ToList());

                // Notificăm UI-ul
                OnPropertyChanged("AllRooms");
                OnPropertyChanged("AllClients");
            }
        }

        private void LoadRoomSchedule(int roomId)
        {
            using (var db = new HotelDBContext())
            {
                // Căutăm rezervările viitoare sau active pentru această cameră
                // Atenție: ReservationRoom face legătura
                RoomFutureReservations = db.Reservations
                    .Where(r => r.Status != ReservationStatus.Cancelled && r.CheckOutDate >= DateTime.Today)
                    .Where(r => r.Rooms.Any(room => room.Id == roomId))
                    .ToList();

                // Declanșăm evenimentul pentru ca View-ul (Code Behind) să actualizeze Calendarul grafic
                var handler = OnScheduleLoaded;
                if (handler != null) handler(RoomFutureReservations);
            }
        }

        private void ExecuteBookOnSpot()
        {
            if (SelectedRoom == null)
            {
                MessageBox.Show("Selectați o cameră!");
                return;
            }

            // Validare date
            if (StartDate >= EndDate)
            {
                MessageBox.Show("Data de plecare trebuie să fie după data de sosire.");
                return;
            }

            using (var db = new HotelDBContext())
            {
                // 1. Verificare disponibilitate (Backend check)
                bool isOccupied = db.Reservations.Any(r =>
                    r.Status != ReservationStatus.Cancelled &&
                    r.Rooms.Any(room => room.Id == SelectedRoom.Id) &&
                    (StartDate < r.CheckOutDate && EndDate > r.CheckInDate));

                if (isOccupied)
                {
                    MessageBox.Show("Camera este deja rezervată în perioada selectată! Verificați calendarul.");
                    return;
                }

                int userIdToBook = 0;

                // 2. Gestionare Client (Existent vs Nou)
                if (IsNewClientMode)
                {
                    if (string.IsNullOrWhiteSpace(NewClientFullName) || string.IsNullOrWhiteSpace(NewClientPhone))
                    {
                        MessageBox.Show("Introduceți Numele și Telefonul clientului nou.");
                        return;
                    }

                    // Creăm userul nou
                    var newUser = new User
                    {
                        FullName = NewClientFullName,
                        Username = NewClientPhone, // Username = Telefon (simplificare)
                        Password = "123",          // Parolă default
                        Role = UserRole.Client
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges(); // Salvăm ca să primim ID-ul
                    userIdToBook = newUser.Id;

                    // Îl adăugăm și în lista locală pentru viitor
                    AllClients.Add(newUser);
                }
                else
                {
                    if (SelectedClient == null)
                    {
                        MessageBox.Show("Selectați un client sau creați unul nou.");
                        return;
                    }
                    userIdToBook = SelectedClient.Id;
                }

                // 3. Creare Rezervare
                var newRes = new Reservation
                {
                    UserId = userIdToBook,
                    CheckInDate = StartDate,
                    CheckOutDate = EndDate,
                    Status = ReservationStatus.Active, // Direct activă
                    TotalPrice = (decimal)(EndDate - StartDate).TotalDays * SelectedRoom.PricePerNight,
                    Rooms = new List<Room>()
                };

                // Reatașăm camera de context
                var dbRoom = db.Rooms.Find(SelectedRoom.Id);
                newRes.Rooms.Add(dbRoom);

                // Dacă rezervarea începe AZI, ocupăm camera visual
                if (StartDate.Date == DateTime.Today)
                {
                    dbRoom.Status = RoomStatus.Occupied;
                }

                db.Reservations.Add(newRes);
                db.SaveChanges();

                MessageBox.Show(string.Format("Rezervare efectuată cu succes pentru camera {0}!", SelectedRoom.RoomNumber));

                // Refresh Calendar și Grid
                LoadData();
                LoadRoomSchedule(SelectedRoom.Id);
                SelectedRoom = null; // Reset
            }
        }
    }
}