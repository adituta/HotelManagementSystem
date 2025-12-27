using HotelManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HotelManagementSystem.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private object _currentAdminSection;
        private MainViewModel _mainVM;

        public object CurrentAdminSection
        {
            get => _currentAdminSection;
            set { _currentAdminSection = value; OnPropertyChanged(nameof(CurrentAdminSection)); }
        }

        public RelayCommand LogoutCommand { get; }
        public RelayCommand ShowEmployeesCommand { get; }
        public RelayCommand ShowFinanceCommand { get; }

        public AdminViewModel(MainViewModel mainVM)
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
        }
}
}
