using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace HotelManagementSystem.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private object _currentAdminSection;
        private MainViewModel _mainVM;

        public object CurrentAdminSection
        {
            get { return _currentAdminSection; }
            set { _currentAdminSection = value; OnPropertyChanged("CurrentAdminSection"); }
        }

        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand ShowEmployeesCommand { get; private set; }
        public RelayCommand ShowFinanceCommand { get; private set; }

        public RelayCommand ShowRoomsCommand { get; private set; }
        public RelayCommand ShowPendingReceiptsCommand { get; private set; } // Adaugat
        public ICommand GenerateReportCommand { get; private set; } // Added
        public ICommand ProcessSalariesCommand { get; private set; } // Added

        public AdminViewModel(MainViewModel mainVM) // Kept original parameter name
        {
            _mainVM = mainVM;
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));

            // Secțiunea implicită la deschidere
            // CurrentAdminSection = new EmployeeManagementViewModel(); 

            //Comanda pentru a schimba sectiunea curenta
            ShowEmployeesCommand = new RelayCommand(o => CurrentAdminSection = new ManageEmployeesViewModel());

            //Optional, deschide automat angajatii la inceput
            CurrentAdminSection = new ManageEmployeesViewModel();


            //Punem sa afisam meniul pentru Finante
            ShowFinanceCommand = new RelayCommand(o => CurrentAdminSection = new FinanceViewModel());


            ///Afisare camere
            ShowRoomsCommand = new RelayCommand(o => CurrentAdminSection = new ReceptionMapViewModel());

            // Aprobare facturi
            ShowPendingReceiptsCommand = new RelayCommand(o => CurrentAdminSection = new PendingReceiptsViewModel(this));
        }
    }
}
