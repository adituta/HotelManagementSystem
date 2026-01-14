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
    public class MyReservationsViewModel : BaseViewModel
    {
        public ObservableCollection<Reservation> ReservationsList { get; set; }
        
        public bool HasReservations { get { return ReservationsList != null && ReservationsList.Count > 0; } }

        public Visibility UserHasReservationsVisibility 
        { 
            get { return HasReservations ? Visibility.Visible : Visibility.Collapsed; } 
        }

        public Visibility EmptyStateVisibility
        {
            get { return !HasReservations ? Visibility.Visible : Visibility.Collapsed; }
        }

        private MainViewModel _main;
        private User _user;

        public MyReservationsViewModel(MainViewModel main, User user)
        {
            _main = main;
            _user = user;
            LoadReservations();
        }

        private void LoadReservations()
        {
            using (var db = new HotelDBContext())
            {
                // Incarcam Active si Pending. Cele Completed sunt la "Chitante".
                var list = db.Reservations.Include("Rooms")
                    .Where(r => r.UserId == _user.Id && r.Status != ReservationStatus.Completed)
                    .OrderByDescending(r => r.CreationDate)
                    .ToList();
                
                ReservationsList = new ObservableCollection<Reservation>(list);
                OnPropertyChanged("ReservationsList");
                OnPropertyChanged("HasReservations");
                OnPropertyChanged("UserHasReservationsVisibility");
                OnPropertyChanged("EmptyStateVisibility");
            }
        }
    }
}
