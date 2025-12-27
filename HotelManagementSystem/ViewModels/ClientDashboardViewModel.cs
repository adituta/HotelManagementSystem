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
            get => _currentClientSection;
            set { _currentClientSection = value; OnPropertyChanged(nameof(CurrentClientSection)); }
        }

        public RelayCommand LogoutCommand { get; }
        public RelayCommand ShowMakeReservationCommand { get; }
        public RelayCommand ShowMyReservationsCommand { get; }

        public RelayCommand ShowFacilitiesCommand { get; }

        public RelayCommand ShowInvoiceCommand { get; }

        public List<Reservation> AllMyReservations { get; set; } = new List<Reservation>();

        // Rezervări Viitoare/Active
        public List<Reservation> UpcomingReservations => AllMyReservations.Where(r => r.Status != ReservationStatus.Completed).ToList();

        // Istoric (Terminat)
        public List<Reservation> PastReservations => AllMyReservations.Where(r => r.Status == ReservationStatus.Completed).ToList();


        private ObservableCollection<Reservation> _allMyReservations;
        public ObservableCollection<Reservation> AllMyReservationsObservable
        {
            get => _allMyReservations;
            set { _allMyReservations = value; OnPropertyChanged(nameof(AllMyReservations)); }
        }

        public ClientDashboardViewModel(MainViewModel mainVM, User user)
        {
            _mainVM = mainVM;
            _loggedUser = user;

            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));

            // Comenzi pentru schimbarea paginilor interne
            ShowMakeReservationCommand = new RelayCommand(o => CurrentClientSection = new MakeReservationViewModel(_loggedUser));

            ShowFacilitiesCommand = new RelayCommand(o =>
            {
                CurrentClientSection = new FacilitiesViewModel(_loggedUser);
            });

            //Deschide factura
            ShowInvoiceCommand = new RelayCommand(o =>
            {
                CurrentClientSection = new InvoiceViewModel(_loggedUser);
            });

            // Pagina implicită
            CurrentClientSection = new MakeReservationViewModel(_loggedUser);
        }
    }
}
