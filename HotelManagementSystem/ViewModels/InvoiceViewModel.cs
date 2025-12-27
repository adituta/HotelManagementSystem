using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace HotelManagementSystem.ViewModels
{
    public class InvoiceViewModel : BaseViewModel
    {
        private User _client;
        public Reservation ActiveReservation { get; set; }

        // Liste pentru afișare în factură
        public ObservableCollection<FoodOrder> FoodItems { get; set; }
        public ObservableCollection<SpaAppointment> SpaItems { get; set; }

        // Sume calculate
        public decimal RoomTotal { get; set; }
        public decimal FoodTotal { get; set; }
        public decimal SpaTotal { get; set; }
        public decimal GrandTotal => RoomTotal + FoodTotal + SpaTotal;

        // Proprietăți pentru Review
        public string ReviewText { get; set; }
        public int ReviewStars { get; set; } = 5;

        public RelayCommand FinishStayCommand { get; }

        public InvoiceViewModel(User client)
        {
            _client = client;
            LoadInvoiceData();
            FinishStayCommand = new RelayCommand(o => ExecuteFinishStay());
        }

        private void LoadInvoiceData()
        {
            using (var db = new HotelDBContext())
            {
                // Încărcăm rezervarea activă cu toate detaliile (Eager Loading)
                ActiveReservation = db.Reservations
                    .Include(r => r.Rooms)
                    .Include(r => r.FoodOrders)
                    .Include(r => r.SpaAppointments.Select(s => s.SpaService))
                    .FirstOrDefault(r => r.UserId == _client.Id && r.Status == ReservationStatus.Active);

                if (ActiveReservation != null)
                {
                    // 1. Calcul Cazare
                    int days = (ActiveReservation.CheckOutDate - ActiveReservation.CheckInDate).Days;
                    if (days <= 0) days = 1;
                    RoomTotal = ActiveReservation.Rooms.Sum(r => r.PricePerNight) * days;

                    // 2. Calcul Mâncare
                    FoodItems = new ObservableCollection<FoodOrder>(ActiveReservation.FoodOrders);
                    FoodTotal = FoodItems.Sum(f => f.Cost);

                    // 3. Calcul SPA
                    SpaItems = new ObservableCollection<SpaAppointment>(ActiveReservation.SpaAppointments);
                    SpaTotal = SpaItems.Sum(s => s.SpaService.PricePerPerson * s.PersonsCount);

                    OnPropertyChanged(string.Empty); // Update UI
                }
            }
        }

        private void ExecuteFinishStay()
        {
            using (var db = new HotelDBContext())
            {
                var dbRes = db.Reservations.Find(ActiveReservation.Id);
                if (dbRes != null)
                {
                    dbRes.Status = ReservationStatus.Completed; // Închidem sejurul
                    dbRes.ReviewComment = ReviewText;
                    dbRes.ReviewRating = ReviewStars;
                    dbRes.TotalPrice = GrandTotal; // Salvăm prețul final plătit

                    db.SaveChanges();
                    MessageBox.Show("Vă mulțumim pentru ședere! Factura a fost achitată și review-ul a fost salvat.");

                    // Opțional: trimitem user-ul la login sau la un ecran de bun rămas
                }
            }
        }
    }
}