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
        private readonly MainViewModel _mainVM;
        private object _currentReceptionView;

        public object CurrentReceptionView
        {
            get { return _currentReceptionView; }
            set { _currentReceptionView = value; OnPropertyChanged("CurrentReceptionView"); }
        }

        // Comenzi pentru meniu
        public RelayCommand ShowMapCommand { get; private set; }
        public RelayCommand ShowRequestsCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand ViewIncomeCommand { get; private set; } // Added

        private readonly User _currentUser; // Added

        public ReceptionViewModel(MainViewModel mainVM, User user) // Updated signature
        {
            _mainVM = mainVM;
            _currentUser = user; // Stored

            // Initializam comenzile
            ShowMapCommand = new RelayCommand(o => CurrentReceptionView = new ReceptionMapViewModel());
            ShowRequestsCommand = new RelayCommand(o => CurrentReceptionView = new ReceptionConfirmationsViewModel());
            ViewIncomeCommand = new RelayCommand(o => _mainVM.CurrentView = new MyIncomeViewModel(_mainVM, _currentUser)); // Added
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));

            // Pagina de start: Harta
            CurrentReceptionView = new ReceptionMapViewModel();
        }
    }
}