using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System;
using System.Collections.Generic;
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
        public decimal GrandTotal { get { return RoomTotal + FoodTotal + SpaTotal; } }

        // Proprietăți pentru Review
        public string ReviewText { get; set; }
        public int ReviewStars { get; set; }

        // Proprietăți pentru Admin / Vizualizare Istoric
        public bool IsAdminMode { get; set; }
        
        private bool _isReadOnly;
        public bool IsReadOnly 
        { 
            get { return _isReadOnly; }
            set 
            { 
                _isReadOnly = value; 
                OnPropertyChanged("IsReadOnly");
                OnPropertyChanged("IsEditable");
            }
        }

        // Smart Checkout Logic
        private bool _hasActiveReservation = true;
        public bool HasActiveReservation 
        { 
            get { return _hasActiveReservation; }
            set 
            { 
                if (_hasActiveReservation != value)
                {
                    _hasActiveReservation = value;
                    OnPropertyChanged("HasActiveReservation");
                    OnPropertyChanged("InvoiceContentVisibility");
                    OnPropertyChanged("NoStayMessageVisibility");
                    OnPropertyChanged("IsEditable");
                }
            }
        }

        public Visibility InvoiceContentVisibility 
        { 
            get { return HasActiveReservation ? Visibility.Visible : Visibility.Collapsed; } 
        }
        
        public Visibility NoStayMessageVisibility 
        { 
            get { return !HasActiveReservation ? Visibility.Visible : Visibility.Collapsed; } 
        }

        // Helper pentru UI binding (ComboBox, TextBox)
        public bool IsEditable 
        { 
            get { return !IsReadOnly && HasActiveReservation; } 
        }

        public RelayCommand FinishStayCommand { get; private set; }
        public RelayCommand ApproveCommand { get; private set; }

        // Constructor modificat pentru a suporta și o rezervare specifică (istoric/admin)
        public InvoiceViewModel(User client, Reservation reservationToView = null, bool isAdminMode = false)
        {
            ReviewStars = 5;
            _client = client;
            IsAdminMode = isAdminMode;

            if (reservationToView != null)
            {
                // Mod vizualizare istoric sau aprobare admin
                ActiveReservation = reservationToView;
                IsReadOnly = true; 
                LoadInvoiceData(reservationToView);
            }
            else
            {
                // Mod standard checkout (client)
                IsReadOnly = false;
                LoadActiveReservation();
            }

            FinishStayCommand = new RelayCommand(o => ExecuteFinishStay(), o => !IsReadOnly && HasActiveReservation);
            ApproveCommand = new RelayCommand(o => ExecuteApprove(), o => IsAdminMode && !ActiveReservation.IsInvoiceApproved);
        }

        private void LoadActiveReservation()
        {
            using (var db = new HotelDBContext())
            {
                 var reservation = db.Reservations
                    .Include(r => r.Rooms)
                    .Include(r => r.FoodOrders)
                    .Include(r => r.SpaAppointments.Select(s => s.SpaService))
                    .FirstOrDefault(r => r.UserId == _client.Id && r.Status == ReservationStatus.Active);
                
                if (reservation != null)
                {
                    HasActiveReservation = true;
                    LoadInvoiceData(reservation);
                }
                else
                {
                    // NU EXISTĂ REZERVARE ACTIVĂ
                    HasActiveReservation = false;
                }
                UpdateVisibility();
            }
        }


        private void UpdateVisibility()
        {
            // Logic moved to property setters
        }

        private void LoadInvoiceData(Reservation reservation)
        {
            ActiveReservation = reservation;

            if (ActiveReservation != null)
            {
                // 1. Calcul Cazare
                int days = (ActiveReservation.CheckOutDate - ActiveReservation.CheckInDate).Days;
                if (days <= 0) days = 1;
                RoomTotal = ActiveReservation.Rooms.Sum(r => r.PricePerNight) * days;

                // 2. Calcul Mâncare
                FoodItems = new ObservableCollection<FoodOrder>(ActiveReservation.FoodOrders ?? new List<FoodOrder>());
                FoodTotal = FoodItems.Sum(f => f.Cost);

                // 3. Calcul SPA
                if (ActiveReservation.SpaAppointments != null)
                {
                   SpaItems = new ObservableCollection<SpaAppointment>(ActiveReservation.SpaAppointments);
                   SpaTotal = SpaItems.Sum(s => s.SpaService.PricePerPerson * s.PersonsCount);
                }
                else
                {
                    SpaItems = new ObservableCollection<SpaAppointment>();
                    SpaTotal = 0;
                }

                // Restaurăm review-ul existent dacă e cazul
                if (IsReadOnly)
                {
                    ReviewText = ActiveReservation.ReviewComment;
                    ReviewStars = ActiveReservation.ReviewRating ?? 5;
                }

                OnPropertyChanged(string.Empty); // Update UI
            }
        }

        private void ExecuteApprove()
        {
             using (var db = new HotelDBContext())
            {
                var dbRes = db.Reservations.Find(ActiveReservation.Id);
                if (dbRes != null)
                {
                    dbRes.IsInvoiceApproved = true;
                    db.SaveChanges();
                    
                    ActiveReservation.IsInvoiceApproved = true;
                    OnPropertyChanged("ActiveReservation"); // Refresh button state
                    MessageBoxHelper.Show("Factura a fost aprobată și este acum vizibilă clientului.", "Aprobat");
                }
            }
        }

        private void ExecuteFinishStay()
        {
            // Logică Smart Checkout
            if (ActiveReservation.CheckOutDate > DateTime.Now)
            {
                bool confirm = MessageBoxHelper.ShowYesNo("Sejurul este mai lung, sunteți sigur că vreți să faceți check-out-ul?", "Confirmare Checkout Anticipat");
                if (!confirm)
                {
                    return; // Clientul a renunțat
                }
            }

            using (var db = new HotelDBContext())
            {
                var dbRes = db.Reservations
                    .Include(r => r.Rooms) // Include rooms to update their status
                    .FirstOrDefault(r => r.Id == ActiveReservation.Id);

                if (dbRes != null)
                {
                    dbRes.Status = ReservationStatus.Completed; // Închidem sejurul
                    dbRes.ReviewComment = ReviewText;
                    dbRes.ReviewRating = ReviewStars;
                    dbRes.TotalPrice = GrandTotal; // Salvăm prețul final plătit
                    dbRes.IsInvoiceApproved = false; // Necesită aprobare admin

                    // UPDATE ROOM STATUS: Mark as Dirty immediately after checkout
                    foreach (var room in dbRes.Rooms)
                    {
                        var dbRoom = db.Rooms.Find(room.Id);
                        if (dbRoom != null)
                        {
                            dbRoom.Status = RoomStatus.CleaningRequired;
                        }
                    }

                    db.SaveChanges();
                    MessageBoxHelper.Show("Ați făcut check-out-ul și plata pentru sejur.\nCamerele necesită curățenie.", "Plată Confirmată");

                    // După checkout, nu mai putem da checkout din nou
                    IsReadOnly = true; 
                    OnPropertyChanged("IsReadOnly");
                    OnPropertyChanged("ActiveReservation");
                }
            }
        }
    }
}