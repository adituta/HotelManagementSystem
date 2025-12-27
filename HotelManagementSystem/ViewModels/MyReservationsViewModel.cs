using HotelManagementSystem.Enums;
using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.ViewModels
{
    public class MyReservationsViewModel : BaseViewModel
    {
        public ObservableCollection<Reservation> PastReservations { get; set; }
        public RelayCommand ViewInvoiceCommand { get; }
        private MainViewModel _main;
        private User _user;

        public MyReservationsViewModel(MainViewModel main, User user)
        {
            _main = main; _user = user;
            LoadHistory();
            ViewInvoiceCommand = new RelayCommand(o => _main.CurrentView = new InvoiceViewModel(_user));
        }

        private void LoadHistory()
        {
            using (var db = new HotelDBContext())
            {
                var list = db.Reservations.Where(r => r.UserId == _user.Id && r.Status == ReservationStatus.Completed).ToList();
                PastReservations = new ObservableCollection<Reservation>(list);
            }
        }
    }
}
