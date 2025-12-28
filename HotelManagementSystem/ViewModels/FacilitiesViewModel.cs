using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
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
            get => _isSejurActiv;
            set { _isSejurActiv = value; OnPropertyChanged(nameof(IsSejurActiv)); }
        }

        // Mesaj explicativ pentru utilizator când meniul e blocat
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        // Liste UI
        public List<MenuItem> LunchMenu { get; set; }
        public List<MenuItem> DinnerMenu { get; set; }
        public List<SpaService> SpaServices { get; set; }
        public List<int> TimeSlots { get; } = new List<int> { 10, 11, 12, 14, 15, 16 };

        // Selecții
        public MenuItem SelectedLunch { get; set; }
        public MenuItem SelectedDinner { get; set; }
        public SpaService SelectedSpaService { get; set; }
        public int SelectedSlot { get; set; }
        public int SpaPersonsCount { get; set; } = 1;

        public RelayCommand OrderFoodCommand { get; }
        public RelayCommand BookSpaCommand { get; }
        public RelayCommand RequestCleaningCommand { get; }

        public FacilitiesViewModel(User client)
        {
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

                        OnPropertyChanged(nameof(LunchMenu));
                        OnPropertyChanged(nameof(DinnerMenu));
                        OnPropertyChanged(nameof(SpaServices));
                    }
                    else if (now < accessStart)
                    {
                        // E ziua sosirii, dar e înainte de ora 12:00
                        StatusMessage = $"Accesul la servicii începe la ora 12:00. (Ora curentă: {now:HH:mm})";
                    }
                    else if (now >= accessEnd)
                    {
                        // E ziua plecării, dar a trecut de ora 11:00
                        StatusMessage = "Accesul la servicii a expirat (Check-out la ora 11:00). Vă mulțumim pentru sejur!";
                    }
                }
            }
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
                int occupied = db.SpaAppointments
                    .Where(a => a.AppointmentDate == DateTime.Today && a.StartTime.Hours == SelectedSlot)
                    .Sum(a => (int?)a.PersonsCount) ?? 0;

                int available = SelectedSpaService.MaxCapacityPerSlot - occupied;

                if (SpaPersonsCount > available)
                {
                    MessageBox.Show($"Locuri insuficiente! Mai sunt doar {available} locuri libere.");
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
                        Cost = SelectedLunch.Price
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
                        Cost = SelectedDinner.Price
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
                MessageBox.Show("Camerista notificată!");
            }
        }
    }
}