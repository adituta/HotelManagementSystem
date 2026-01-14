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
                OnPropertyChanged("IsRoomSelected"); // Keep this line as it's relevant for UI state

                if (_selectedRoom != null)
                {
                    LoadRoomSchedule(_selectedRoom.Id); // Force refresh of reservations
                    CheckAvailabilityForDate();
                }
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

        // --- CALENDAR LOGIC ---
        private DateTime _calendarSelectedDate = DateTime.Today;
        public DateTime CalendarSelectedDate
        {
            get { return _calendarSelectedDate; }
            set
            {
                _calendarSelectedDate = value;
                OnPropertyChanged("CalendarSelectedDate");
                CheckAvailabilityForDate(); // Check triggers immediately
            }
        }

        private string _availabilityStatusMessage;
        public string AvailabilityStatusMessage
        {
            get { return _availabilityStatusMessage; }
            set { _availabilityStatusMessage = value; OnPropertyChanged("AvailabilityStatusMessage"); }
        }

        private string _availabilityStatusColor;
        public string AvailabilityStatusColor
        {
            get { return _availabilityStatusColor; }
            set { _availabilityStatusColor = value; OnPropertyChanged("AvailabilityStatusColor"); }
        }

        private void CheckAvailabilityForDate()
        {
            try
            {
                if (SelectedRoom == null)
                {
                    AvailabilityStatusMessage = "Selectați o cameră mai întâi.";
                    AvailabilityStatusColor = "Gray";
                    return;
                }

                // Folosim lista deja încărcată (RoomFutureReservations) pentru a evita query-uri inutile
                // și pentru a putea compara DOAR datele (fără ore), rezolvând problema afișării.
                
                // Dacă lista e null (nu s-a încărcat încă), o ignorăm momentan
                var reservations = RoomFutureReservations ?? new List<Reservation>();

                var reservation = reservations.FirstOrDefault(r => 
                    r.Status != ReservationStatus.Cancelled &&
                    CalendarSelectedDate.Date >= r.CheckInDate.Date && 
                    CalendarSelectedDate.Date < r.CheckOutDate.Date);

                if (reservation != null)
                {
                    AvailabilityStatusMessage = $"OCUPAT - {reservation.User?.FullName ?? "Client"} (Până pe {reservation.CheckOutDate:dd.MM})";
                    AvailabilityStatusColor = "#E74C3C"; 
                }
                else
                {
                    if (CalendarSelectedDate.Date == DateTime.Today && 
                       (SelectedRoom.Status == RoomStatus.CleaningRequired || SelectedRoom.Status == RoomStatus.CleaningInProgress))
                    {
                         AvailabilityStatusMessage = $"INDISPONIBIL - Necesită Curățenie!";
                         AvailabilityStatusColor = "#F1C40F"; 
                    }
                    else
                    {
                        AvailabilityStatusMessage = "DISPONIBIL";
                        AvailabilityStatusColor = "#27AE60"; 
                    }
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Eroare la verificarea disponibilității: " + ex.ToString());
            }
        }

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
            try
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Today.AddDays(1);

                LoadData();
                
                BookOnSpotCommand = new RelayCommand(o => ExecuteBookOnSpot());
                RefreshCommand = new RelayCommand(o => LoadData());

                SetRoomCommand = new RelayCommand(obj =>
                {
                    var r = obj as Room;
                    if (r != null)
                    {
                        SelectedRoom = r;
                        CheckAvailabilityForDate(); // Update availability when room changes
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la inițializarea hărții: " + ex.ToString());
            }
        }

        private void LoadData()
        {
            try
            {
                using (var db = new HotelDBContext())
                {
                    // 1. Încărcăm lista de camere brută
                    var roomsList = db.Rooms.OrderBy(r => r.RoomNumber).ToList();

                    // 2. Căutăm ID-urile camerelor care sunt ocupate ASTĂZI
                    var today = DateTime.Today;

                    var occupiedRoomIds = db.Reservations
                        .Where(r => r.Status == ReservationStatus.Active &&
                                    r.CheckInDate <= today &&
                                    r.CheckOutDate > today)
                        .SelectMany(r => r.Rooms)
                        .Select(r => r.Id)
                        .ToList();

                    // 3. Actualizăm statusul DOAR ÎN MEMORIE
                    foreach (var room in roomsList)
                    {
                        if (room.Status == RoomStatus.CleaningRequired || room.Status == RoomStatus.CleaningInProgress)
                        {
                            continue;
                        }

                        if (occupiedRoomIds.Contains(room.Id))
                        {
                            room.Status = RoomStatus.Occupied;
                        }
                    }

                    // 4. Trimitem lista către interfață
                    AllRooms = new ObservableCollection<Room>(roomsList);

                    // 5. Încărcăm și clienții
                    AllClients = new ObservableCollection<User>(db.Users.Where(u => u.Role == UserRole.Client).ToList());
                }

                // Notificăm UI-ul
                OnPropertyChanged("AllRooms");
                OnPropertyChanged("AllClients");
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Eroare la încărcarea datelor: " + ex.Message);
            }
        }

        private void LoadRoomSchedule(int roomId)
        {
            using (var db = new HotelDBContext())
            {
                // Modificare: Include doar rezervările Active (Confirmate).
                // Cele Pending vor apărea cu Verde (Disponibile) până când sunt confirmate.
                RoomFutureReservations = db.Reservations
                    .Include(r => r.User) 
                    .Where(r => r.Status == ReservationStatus.Active && r.CheckOutDate >= DateTime.Today)
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
                MessageBoxHelper.Show("Selectați o cameră!", "Eroare");
                return;
            }

            // Validare date
            if (StartDate >= EndDate)
            {
                MessageBoxHelper.Show("Data de plecare trebuie să fie după data de sosire.", "Eroare");
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
                    MessageBoxHelper.Show("Camera este deja rezervată în perioada selectată! Verificați calendarul.", "Info");
                    return;
                }

                int userIdToBook = 0;

                // 2. Gestionare Client (Existent vs Nou)
                if (IsNewClientMode)
                {
                    if (string.IsNullOrWhiteSpace(NewClientFullName) || string.IsNullOrWhiteSpace(NewClientPhone))
                    {
                        MessageBoxHelper.Show("Introduceți Numele și Telefonul clientului nou.", "Eroare");
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
                        MessageBoxHelper.Show("Selectați un client sau creați unul nou.", "Eroare");
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

                MessageBoxHelper.Show(string.Format("Rezervare efectuată cu succes pentru camera {0}!", SelectedRoom.RoomNumber), "Succes");

                // Refresh Calendar și Grid
                LoadData();
                LoadRoomSchedule(SelectedRoom.Id);
                SelectedRoom = null; // Reset
            }
        }
    }
}