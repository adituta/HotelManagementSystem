using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagementSystem.Enums;
using System.Collections.ObjectModel;


namespace HotelManagementSystem.ViewModels
{
    public class ClientDashboardViewModel : BaseViewModel
    {
        private object _currentClientSection;
        private MainViewModel _mainVM;
        private User _loggedUser;





        public object CurrentClientSection
        {
            get { return _currentClientSection; }
            set { _currentClientSection = value; OnPropertyChanged("CurrentClientSection"); }
        }

        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand ShowMakeReservationCommand { get; private set; }
        public RelayCommand ShowMyReservationsCommand { get; private set; }

        public RelayCommand ShowFacilitiesCommand { get; private set; }

        public RelayCommand ShowInvoiceCommand { get; private set; }
        public RelayCommand ShowMyReceiptsCommand { get; private set; } // Adaugat

        public List<Reservation> AllMyReservations { get; set; }

        // Rezervări Viitoare/Active
        public List<Reservation> UpcomingReservations { get { return AllMyReservations.Where(r => r.Status != ReservationStatus.Completed).ToList(); } }

        // Istoric (Terminat)
        public List<Reservation> PastReservations { get { return AllMyReservations.Where(r => r.Status == ReservationStatus.Completed).ToList(); } }


        private ObservableCollection<Reservation> _allMyReservations;
        public ObservableCollection<Reservation> AllMyReservationsObservable
        {
            get { return _allMyReservations; }
            set { _allMyReservations = value; OnPropertyChanged("AllMyReservations"); }
        }

        public RelayCommand ShowNotificationsCommand { get; private set; }
        public ClientDashboardViewModel(MainViewModel mainVM, User user)
        {
            AllMyReservations = new List<Reservation>();
            _mainVM = mainVM;
            _loggedUser = user;

            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));

            // Comenzi pentru schimbarea paginilor interne
            ShowMakeReservationCommand = new RelayCommand(o => CurrentClientSection = new MakeReservationViewModel(_loggedUser));
            
            ShowInvoiceCommand = new RelayCommand(o => 
            {
                CurrentClientSection = new InvoiceViewModel(_loggedUser);
            });

            ShowFacilitiesCommand = new RelayCommand(o =>
            {
                CurrentClientSection = new FacilitiesViewModel(_loggedUser);
            });

            //afisare notificari
            ShowNotificationsCommand = new RelayCommand(o => 
            {
                CurrentClientSection = new NotificationsViewModel(_loggedUser);
            });

            // Afișare istoric facturi
            ShowMyReceiptsCommand = new RelayCommand(o =>
            {
                CurrentClientSection = new MyReceiptsViewModel(_loggedUser, this);
            });

            // Afisare rezervari
            ShowMyReservationsCommand = new RelayCommand(o =>
            {
                CurrentClientSection = new MyReservationsViewModel(_mainVM, _loggedUser);
            });

            // Pagina implicită
            CurrentClientSection = new MakeReservationViewModel(_loggedUser);
        }
    }
}
