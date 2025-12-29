using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Data.Entity;

namespace HotelManagementSystem.ViewModels
{
    public class FacilitiesViewModel : BaseViewModel
    {
        private User _client;
        private Reservation _activeReservation;

        // Proprietatea care controlează vizibilitatea
        private bool _isSejurActiv;
        public bool IsSejurActiv
        {
            get { return _isSejurActiv; }
            set { _isSejurActiv = value; OnPropertyChanged("IsSejurActiv"); }
        }

        // Mesaj explicativ pentru utilizator când meniul e blocat
        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged("StatusMessage"); }
        }

        // Liste UI
        public List<MenuItem> LunchMenu { get; set; }
        public List<MenuItem> DinnerMenu { get; set; }
        public List<SpaService> SpaServices { get; set; }
        // Folosim o clasă ajutătoare pentru a afișa disponibilitatea
        public ObservableCollection<TimeSlotDto> TimeSlots { get; private set; }

        // Selecții
        public MenuItem SelectedLunch { get; set; }
        public MenuItem SelectedDinner { get; set; }
        public SpaService SelectedSpaService { get; set; }
        public int SelectedSlot { get; set; }
        public int SpaPersonsCount { get; set; }

        public RelayCommand OrderFoodCommand { get; private set; }
        public RelayCommand BookSpaCommand { get; private set; }
        public RelayCommand RequestCleaningCommand { get; private set; }

        public FacilitiesViewModel(User client)
        {
            TimeSlots = new ObservableCollection<TimeSlotDto>();
            SpaPersonsCount = 1;
            _client = client;
            LoadActiveData(); // Aici e logica nouă

            OrderFoodCommand = new RelayCommand(o => ExecuteOrderFood());
            BookSpaCommand = new RelayCommand(o => ExecuteBookSpa());
            RequestCleaningCommand = new RelayCommand(o => ExecuteRequestCleaning());
        }

        private void LoadActiveData()
        {
            using (var db = new HotelDBContext())
            {
                DateTime now = DateTime.Now;
                IsSejurActiv = false;
                StatusMessage = "Nu aveți nicio rezervare activă în acest moment.";

                // 1. Căutăm rezervarea confirmată de recepție (Active)
                // care acoperă calendaristic ziua de azi (chiar dacă ora nu corespunde încă)
                var reservation = db.Reservations
                    .Include(r => r.Rooms)
                    .FirstOrDefault(r => r.UserId == _client.Id &&
                                         r.Status == ReservationStatus.Active &&
                                         // Verificăm doar datele brute pentru a găsi rezervarea
                                         DbFunctions.TruncateTime(r.CheckInDate) <= DbFunctions.TruncateTime(now) &&
                                         DbFunctions.TruncateTime(r.CheckOutDate) >= DbFunctions.TruncateTime(now));

                if (reservation != null)
                {
                    _activeReservation = reservation;

                    // 2. APLICĂM REGULA DE ORE (12:00 CheckIn - 11:00 CheckOut)

                    // Construim momentul exact când începe dreptul la servicii: Ziua de CheckIn la ora 12:00 PM
                    DateTime accessStart = reservation.CheckInDate.Date.AddHours(12);

                    // Construim momentul exact când se termină dreptul: Ziua de CheckOut la ora 11:00 AM
                    DateTime accessEnd = reservation.CheckOutDate.Date.AddHours(11);

                    // Verificăm dacă suntem în interval
                    if (now >= accessStart && now < accessEnd)
                    {
                        // SUNTEM ÎN TIMPUL SEJURULUI -> Acces permis
                        IsSejurActiv = true;
                        StatusMessage = ""; // Nu afișăm mesaj de eroare

                        // Încărcăm meniurile
                        LunchMenu = db.MenuItems.Where(m => m.Category == "Lunch").ToList();
                        DinnerMenu = db.MenuItems.Where(m => m.Category == "Dinner").ToList();
                        SpaServices = db.SpaServices.ToList();

                        OnPropertyChanged("DinnerMenu");
                        OnPropertyChanged("SpaServices");
                        
                        LoadSlotsAvailability(); // Calculăm locurile libere
                    }
                    else if (now < accessStart)
                    {
                        // E ziua sosirii, dar e înainte de ora 12:00
                        StatusMessage = string.Format("Accesul la servicii începe la ora 12:00. (Ora curentă: {0:HH:mm})", now);
                    }
                    else if (now >= accessEnd)
                    {
                        // E ziua plecării, dar a trecut de ora 11:00
                        StatusMessage = "Accesul la servicii a expirat (Check-out la ora 11:00). Vă mulțumim pentru sejur!";
                    }
                }
            }
        }

        private void LoadSlotsAvailability()
        {
            TimeSlots.Clear();
            var hours = new List<int> { 10, 11, 12, 14, 15, 16 };

            using (var db = new HotelDBContext())
            {
                foreach (var h in hours)
                {
                    // Calculăm câți sunt deja programați la ora h azi (Inclusiv Pending!)
                    int occupied = db.SpaAppointments
                        .Where(a => a.AppointmentDate == DateTime.Today && a.StartTime.Hours == h) // Removed IsConfirmed
                        .Sum(a => (int?)a.PersonsCount) ?? 0;

                    int maxCapacity = 6;
                    int free = maxCapacity - occupied;
                    if (free < 0) free = 0;

                    TimeSlots.Add(new TimeSlotDto 
                    { 
                        Hour = h, 
                        Display = $"{h}:00  ({free} locuri libere)",
                        IsFull = free == 0
                    });
                }
            }
            OnPropertyChanged("TimeSlots");
        }

        private void ExecuteBookSpa()
        {
            if (!IsSejurActiv) { MessageBox.Show(StatusMessage); return; }
            if (SelectedSpaService == null || SelectedSlot == 0)
            {
                MessageBox.Show("Selectați serviciul și ora!");
                return;
            }

            using (var db = new HotelDBContext())
            {
                // STANDARD: Capacitate fixă de 6 persoane pe oră, indiferent de serviciu
                int maxCapacity = 6;

                // Calculăm ocuparea incluzând și cererile neconfirmate (Pending) pentru a evita overbooking-ul
                int occupied = db.SpaAppointments
                    .Where(a => a.AppointmentDate == DateTime.Today && a.StartTime.Hours == SelectedSlot)
                    .Sum(a => (int?)a.PersonsCount) ?? 0;

                int available = maxCapacity - occupied;

                if (SpaPersonsCount > available)
                {
                    MessageBox.Show(string.Format("Locuri insuficiente! Mai sunt doar {0} locuri libere.", available));
                    return;
                }

                db.SpaAppointments.Add(new SpaAppointment
                {
                    ReservationId = _activeReservation.Id,
                    SpaServiceId = SelectedSpaService.Id,
                    AppointmentDate = DateTime.Today,
                    StartTime = new TimeSpan(SelectedSlot, 0, 0),
                    PersonsCount = SpaPersonsCount,
                    IsConfirmed = false
                });
                db.SaveChanges();
                
                // NOTIFICARE CERERE SPA
                NotificationService.Send(_client.Id, $"Rezervarea SPA pentru data {DateTime.Today.ToShortDateString()} la ora {SelectedSlot}:00 a fost trimisă.");
                
                MessageBox.Show("Programare SPA trimisă!");
            }
        }

        private void ExecuteOrderFood()
        {
            if (!IsSejurActiv) { MessageBox.Show(StatusMessage); return; }
            if (SelectedLunch == null && SelectedDinner == null)
            {
                MessageBox.Show("Selectați mâncarea.");
                return;
            }

            using (var db = new HotelDBContext())
            {
                if (SelectedLunch != null)
                {
                    db.FoodOrders.Add(new FoodOrder
                    {
                        ReservationId = _activeReservation.Id,
                        MenuItemId = SelectedLunch.Id,
                        OrderDate = DateTime.Now,
                        MealType = "Lunch",
                        FoodDetails = SelectedLunch.Name,
                        Cost = SelectedLunch.Price,
                        Status = OrderStatus.Pending
                    });
                }
                if (SelectedDinner != null)
                {
                    db.FoodOrders.Add(new FoodOrder
                    {
                        ReservationId = _activeReservation.Id,
                        MenuItemId = SelectedDinner.Id,
                        OrderDate = DateTime.Now,
                        MealType = "Dinner",
                        FoodDetails = SelectedDinner.Name,
                        Cost = SelectedDinner.Price,
                        Status = OrderStatus.Pending
                    });
                }
                db.SaveChanges();
                NotificationService.Send(_client.Id, "Comanda trimisă la bucătărie!");
                MessageBox.Show("Comandă plasată!");
            }
        }

        private void ExecuteRequestCleaning()
        {
            if (!IsSejurActiv) { MessageBox.Show(StatusMessage); return; }
            using (var db = new HotelDBContext())
            {
                foreach (var room in _activeReservation.Rooms)
                {
                    var dbRoom = db.Rooms.Find(room.Id);
                    if (dbRoom != null) dbRoom.Status = RoomStatus.CleaningRequired;
                }
                db.SaveChanges();
                
                // NOTIFICARE CERERE CURATENIE
                NotificationService.Send(_client.Id, "Solicitarea de curățenie a fost transmisă cameristei.");

                MessageBox.Show("Camerista notificată!");
            }
        }
    }

    public class TimeSlotDto
    {
        public int Hour { get; set; }
        public string Display { get; set; }
        public bool IsFull { get; set; }
    }
}