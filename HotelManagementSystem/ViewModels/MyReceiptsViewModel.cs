using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace HotelManagementSystem.ViewModels
{
    public class MyReceiptsViewModel : BaseViewModel
    {
        private User _client;
        public ObservableCollection<Reservation> Receipts { get; set; }

        private bool _hasNoReceipts;
        public bool HasNoReceipts
        {
            get { return _hasNoReceipts; }
            set
            {
                _hasNoReceipts = value;
                OnPropertyChanged(nameof(HasNoReceipts));
            }
        }
        
        public RelayCommand ViewReceiptCommand { get; private set; }

        public MyReceiptsViewModel(User client, ClientDashboardViewModel dashboardVM)
        {
            _client = client;
            
            // Incarcam chitantele
            LoadReceipts();

            // Comanda pentru a vedea detaliile
            // Cand dam click, schimbam CurrentClientSection din Dashboard cu un InvoiceViewModel in mod readonly
            ViewReceiptCommand = new RelayCommand(o => 
            {
                var reservation = o as Reservation;
                if (reservation != null)
                {
                    dashboardVM.CurrentClientSection = new InvoiceViewModel(_client, reservation, isAdminMode: false);
                }
            });
        }

        private void LoadReceipts()
        {
            using (var db = new HotelDBContext())
            {
                // Luam doar rezervarile terminate si aprobate
                var list = db.Reservations
                    .Where(r => r.UserId == _client.Id && r.Status == ReservationStatus.Completed && r.IsInvoiceApproved)
                    .Include(r => r.Rooms)
                    .Include(r => r.FoodOrders)
                    .Include(r => r.SpaAppointments.Select(s => s.SpaService))
                    .OrderByDescending(r => r.CheckOutDate)
                    .ToList();

                Receipts = new ObservableCollection<Reservation>(list);
                HasNoReceipts = Receipts.Count == 0;
            }
        }
    }
}
