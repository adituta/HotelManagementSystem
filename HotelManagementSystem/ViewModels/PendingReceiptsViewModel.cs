using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace HotelManagementSystem.ViewModels
{
    public class PendingReceiptsViewModel : BaseViewModel
    {
        public ObservableCollection<Reservation> PendingReceipts { get; set; }

        private bool _hasNoPendingReceipts;
        public bool HasNoPendingReceipts
        {
            get { return _hasNoPendingReceipts; }
            set
            {
                _hasNoPendingReceipts = value;
                OnPropertyChanged(nameof(HasNoPendingReceipts));
            }
        }
        
        public RelayCommand ReviewReceiptCommand { get; private set; }

        public PendingReceiptsViewModel(AdminViewModel adminVM)
        {
            LoadPendingReceipts();

            ReviewReceiptCommand = new RelayCommand(o => 
            {
                var reservation = o as Reservation;
                if (reservation != null)
                {
                    // Deschidem InvoiceView in mod Admin pentru aprobare
                    // Trimitem un User dummy sau null fiindca in mod admin user-ul nu conteaza asa mult, dar constructorul cere un User client. 
                    // Putem trimite userul din rezervare.
                    using (var db = new HotelDBContext())
                    {
                       // Reincarcam userul ca sa fim siguri (desi EF include ar trebui sa rezolve daca il cerem)
                       // Dar aici avem deja reservation cu UserId.
                       var client = db.Users.Find(reservation.UserId);
                       adminVM.CurrentAdminSection = new InvoiceViewModel(client, reservation, isAdminMode: true);
                    }
                }
            });
        }

        private void LoadPendingReceipts()
        {
            using (var db = new HotelDBContext())
            {
                // Luam rezervarile complete dar neaprobate
                var list = db.Reservations
                    .Where(r => r.Status == ReservationStatus.Completed && !r.IsInvoiceApproved)
                    .Include(r => r.User) // Avem nevoie de numele userului
                    .Include(r => r.Rooms)
                    .Include(r => r.FoodOrders)
                    .Include(r => r.SpaAppointments.Select(s => s.SpaService))
                    .OrderBy(r => r.CheckOutDate)
                    .ToList();

                PendingReceipts = new ObservableCollection<Reservation>(list);
                HasNoPendingReceipts = PendingReceipts.Count == 0;
            }
        }
    }
}
