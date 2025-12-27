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
    public class FacilitiesViewModel : BaseViewModel
    {
        private User _client;
        private Reservation _activeReservation;

        // Liste pentru UI
        public List<MenuItem> LunchMenu { get; set; }
        public List<MenuItem> DinnerMenu { get; set; }
        public List<SpaService> SpaServices { get; set; }
        public List<int> TimeSlots { get; } = new List<int> { 10, 11, 12, 14, 15, 16 }; // Cele 6 sloturi orare

        // Selecții utilizator
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
            LoadActiveData();

            OrderFoodCommand = new RelayCommand(o => ExecuteOrderFood());
            BookSpaCommand = new RelayCommand(o => ExecuteBookSpa());
            RequestCleaningCommand = new RelayCommand(o => ExecuteRequestCleaning());
        }
        

        public bool IsSejurActiv => _activeReservation != null;



        private void LoadActiveData()
        {
            using (var db = new HotelDBContext())
            {
                DateTime today = DateTime.Now.Date;

                // Căutăm rezervarea ACTIVĂ a clientului pentru ziua de azi
                _activeReservation = db.Reservations
                    .Include(r => r.Rooms)
                    .FirstOrDefault(r => r.UserId == _client.Id &&
                                         r.Status == ReservationStatus.Active &&
                                         today >= r.CheckInDate &&
                                         today <= r.CheckOutDate);

                if (_activeReservation == null) return;

                // Încărcăm meniurile din cataloage
                LunchMenu = db.Set<MenuItem>().Where(m => m.Category == "Lunch").ToList();
                DinnerMenu = db.Set<MenuItem>().Where(m => m.Category == "Dinner").ToList();
                SpaServices = db.Set<SpaService>().ToList();

                OnPropertyChanged(nameof(IsSejurActiv));
            }
        }

        private void ExecuteBookSpa()
        {
            if (SelectedSpaService == null || SelectedSlot == 0)
            {
                MessageBox.Show("Selectați serviciul și ora!");
                return;
            }

            using (var db = new HotelDBContext())
            {
                // Calculăm locurile ocupate la acea oră (Max 10 per slot)
                int occupied = db.SpaAppointments
                    .Where(a => a.AppointmentDate == DateTime.Today && a.StartTime.Hours == SelectedSlot)
                    .Sum(a => (int?)a.PersonsCount) ?? 0;

                int available = 10 - occupied;

                if (SpaPersonsCount > available)
                {
                    MessageBox.Show($"Locuri insuficiente! Mai sunt doar {available} locuri libere la ora {SelectedSlot}:00.");
                    return;
                }

                db.SpaAppointments.Add(new SpaAppointment
                {
                    ReservationId = _activeReservation.Id,
                    SpaServiceId = SelectedSpaService.Id,
                    AppointmentDate = DateTime.Today,
                    StartTime = new TimeSpan(SelectedSlot, 0, 0),
                    PersonsCount = SpaPersonsCount,
                    IsConfirmed = true
                });
                db.SaveChanges();
                MessageBox.Show("Programare SPA confirmată!");
            }
        }

        private void ExecuteOrderFood()
        {
            if (SelectedLunch == null && SelectedDinner == null) return;

            using (var db = new HotelDBContext())
            {
                //pentru pranz
                if (SelectedLunch != null)
                    db.FoodOrders.Add(new FoodOrder
                    {
                        ReservationId = _activeReservation.Id,
                        MenuItemId = SelectedLunch.Id,
                        OrderDate = DateTime.Now,
                        MealType = "Lunch",
                        FoodDetails = SelectedLunch.Name,
                        Cost = SelectedLunch.Price
                    });

                //pentru cina
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
                NotificationService.Send(_client.Id, "Bucătăria a preluat comanda dvs. pentru " + SelectedLunch.Name);
                MessageBox.Show("Comanda de masă a fost trimisă!");
            }
        }

        private void ExecuteRequestCleaning()
        {
            using (var db = new HotelDBContext())
            {
                // Luăm prima cameră din rezervare (pentru simplitate)
                var room = _activeReservation.Rooms.FirstOrDefault();
                if (room != null)
                {
                    var dbRoom = db.Rooms.Find(room.Id);
                    dbRoom.Status = RoomStatus.CleaningRequired;
                    db.SaveChanges();
                    MessageBox.Show("Cameristele au fost notificate pentru curățenie!");
                }
            }
        }
    }
}