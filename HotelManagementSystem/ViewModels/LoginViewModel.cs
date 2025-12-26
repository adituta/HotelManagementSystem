using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;
using HotelManagementSystem.ViewModels;


namespace HotelManagementSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

        private string _username;
        private readonly MainViewModel _mainVM;


        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }



        public RelayCommand LoginCommand { get; }
        public RelayCommand RegisterCommand { get; }

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(o => ExecuteGoToRegister());

        }

        private  void ExecuteGoToRegister()
        {
            _mainVM.CurrentView = new RegisterViewModel(_mainVM);
        }

        private void ExecuteLogin(object parameter)
        {
            //Iau parola din PasswordBox transmisa ca parametru in xaml
            var passwordContainer = parameter as PasswordBox;
            string password = passwordContainer?.Password;


            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Te rugam introdu utilizatorul si parola.");
                return;
            }

            using (var db = new HotelDBContext())
            {
                //Cautam userul in baza de date
                var user = db.Users.FirstOrDefault(u => u.Username == Username && u.Password == password);

                if (user != null)
                {
                    //Logare reusita!
                    MessageBox.Show($"Bine ai venit, {user.FullName}!");

                    //Aici voi schimba pagina in functie de rolul userului
                    SwitchToRoleView(user);
                }
                else
                {
                    MessageBox.Show("Utilizator sau parola incorecta.");
                }
            }
        }

        private void SwitchToRoleView(User user)
        {
            switch (user.Role)
            {
                case Enums.UserRole.Administrator:
                    _mainVM.CurrentView = new AdminViewModel(_mainVM);
                    break;
                case Enums.UserRole.Receptionist:
                    _mainVM.CurrentView = new ReceptionViewModel();
                    break;
                case Enums.UserRole.Cleaning:
                    _mainVM.CurrentView = new MaidViewModel(user); 
                    break;
                case Enums.UserRole.Client:
                    _mainVM.CurrentView = new ClientDashboardViewModel(_mainVM, user);
                    break;
                default:
                    MessageBox.Show("Rol de utilizator necunoscut.");
                    break;
            }
        }
    }
}
