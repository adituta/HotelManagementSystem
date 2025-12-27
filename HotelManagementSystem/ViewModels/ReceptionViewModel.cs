using HotelManagementSystem.Enums;
using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class ReceptionViewModel : BaseViewModel
    {
        private ObservableCollection<Reservation> _pendingReservations;
        public ObservableCollection<Reservation> PendingReservations
        {
            get => _pendingReservations;
            set { _pendingReservations = value; OnPropertyChanged(nameof(PendingReservations)); }
        }

        public RelayCommand ConfirmCommand { get; }

        public ReceptionViewModel()
        {
            LoadPendingReservations();
            ConfirmCommand = new RelayCommand(res => ExecuteConfirm(res as Reservation));
        }

        private void LoadPendingReservations()
        {
            using (var db = new HotelDBContext())
            {
                // Luăm rezervările Pending și includem datele despre Client și Camere
                var list = db.Reservations
                    .Include("User")
                    .Include("Rooms")
                    .Where(r => r.Status == ReservationStatus.Pending)
                    .ToList();
                PendingReservations = new ObservableCollection<Reservation>(list);
            }
        }

        private void ExecuteConfirm(Reservation res)
        {
            if (res == null) return;
            using (var db = new HotelDBContext())
            {
                var dbRes = db.Reservations.Find(res.Id);
                if (dbRes != null)
                {
                    dbRes.Status = ReservationStatus.Active;
                    db.SaveChanges();
                    NotificationService.Send(res.UserId, "Rezervarea dvs. a fost confirmată! Menul de facilități este acum activ.");
                    MessageBox.Show("Rezervare confirmată! Clientul are acum acces la servicii.");
                    LoadPendingReservations();
                }
            }
        }
    }
}
