using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Windows;
using HotelManagementSystem.Enums;
using System.Windows.Controls;


namespace HotelManagementSystem.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _fullName;
        private string _username;
        private readonly MainViewModel _mainVM;

        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                OnPropertyChanged("FullName");
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public RelayCommand RegisterCommand { get; private set; }
        public RelayCommand BackCommand { get; private set; }

        public RegisterViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            RegisterCommand = new RelayCommand(ExecuteRegister);
            BackCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));
        }

        private void ExecuteRegister(object parameter)
        {
            var passBox = parameter as PasswordBox;
            string password = (passBox != null) ? passBox.Password : null;

            // 1. Validări de bază
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(FullName))
            {
                MessageBoxHelper.Show("Toate câmpurile sunt obligatorii.", "Eroare");
                return;
            }

            using (var db = new HotelDBContext())
            {
                // 2. Verificăm dacă utilizatorul există deja
                if (db.Users.Any(u => u.Username == Username))
                {
                    MessageBoxHelper.Show("Acest nume de utilizator este deja folosit.", "Eroare");
                    return;
                }

                // 3. Creăm noul utilizator cu rolul de Client
                var newUser = new User
                {
                    FullName = FullName,
                    Username = Username,
                    Password = password, // În producție se folosește hashing
                    Role = UserRole.Client
                };

                db.Users.Add(newUser);
                db.SaveChanges();
            }

            MessageBoxHelper.Show("Cont creat cu succes! Te poți loga acum.", "Succes");
            _mainVM.CurrentView = new LoginViewModel(_mainVM);
        }
    }
}
