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
    public class ReceptionConfirmationsViewModel : BaseViewModel
    {
        private ObservableCollection<Reservation> _pendingReservations;
        public ObservableCollection<Reservation> PendingReservations
        {
            get { return _pendingReservations; }
            set { _pendingReservations = value; OnPropertyChanged("PendingReservations"); }
        }

        public RelayCommand ConfirmCommand { get; private set; }

        public ReceptionConfirmationsViewModel()
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
                var dbRes = db.Reservations
                              .Include("Rooms")
                              .FirstOrDefault(r => r.Id == res.Id);

                if (dbRes != null)
                {
                    dbRes.Status = ReservationStatus.Active;
                    
                    // CRITICAL FIX: Daca CheckIn-ul e AZI (sau in trecut), camera devine fizic OCUPATA (Roșu)
                    if (dbRes.CheckInDate <= DateTime.Today)
                    {
                         foreach(var room in dbRes.Rooms)
                         {
                             // Re-aducem camera din context daca e nevoie, sau updated direct
                             // (EF urmareste obiectele din dbRes.Rooms fiindca e acelasi context)
                             room.Status = RoomStatus.Occupied;
                         }
                    }

                    db.SaveChanges();
                    NotificationService.Send(res.UserId, "Rezervarea dvs. a fost confirmată! Menul de facilități este acum activ.");
                    MessageBoxHelper.Show("Rezervare confirmată! Clientul are acum acces la servicii.", "Succes");
                    LoadPendingReservations();
                }
            }
        }
    }
}
