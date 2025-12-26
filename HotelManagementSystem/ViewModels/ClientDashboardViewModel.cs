using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;

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

        public ClientDashboardViewModel(MainViewModel mainVM, User user)
        {
            _mainVM = mainVM;
            _loggedUser = user;

            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));

            // Comenzi pentru schimbarea paginilor interne
            ShowMakeReservationCommand = new RelayCommand(o => CurrentClientSection = new MakeReservationViewModel(_loggedUser));
            // ShowMyReservationsCommand = ... (vom crea ulterior)

            // Pagina implicită
            CurrentClientSection = new MakeReservationViewModel(_loggedUser);
        }
    }
}
