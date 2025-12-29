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
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }



        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand RegisterCommand { get; private set; }

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
            string password = (passwordContainer != null) ? passwordContainer.Password : null;


            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Te rugam introdu utilizatorul si parola.");
                return;
            }

            using (var db = new HotelDBContext())
            {
                // --- AUTO-CHECKOUT LOGIC ---
                // Verificăm dacă există rezervări expirate (CheckOutDate < Acum) și le încheiem automat
                // Setăm camerele pe "CleaningRequired"
                var expiredReservations = db.Reservations
                    .Include("Rooms") // Avem nevoie de camere pentru a le schimba statusul
                    .Where(r => r.Status == Enums.ReservationStatus.Active && r.CheckOutDate < DateTime.Now)
                    .ToList();

                if (expiredReservations.Count > 0)
                {
                    foreach (var res in expiredReservations)
                    {
                        res.Status = Enums.ReservationStatus.Completed;
                        foreach (var room in res.Rooms)
                        {
                            room.Status = Enums.RoomStatus.CleaningRequired;
                        }
                    }
                    db.SaveChanges(); // Salvăm modificările globale
                }
                // ---------------------------

                //Cautam userul in baza de date
                var user = db.Users.FirstOrDefault(u => u.Username == Username && u.Password == password);

                if (user != null)
                {
                    //Logare reusita!
                    MessageBox.Show(string.Format("Bine ai venit, {0}!", user.FullName));

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
                    _mainVM.CurrentView = new ReceptionViewModel(_mainVM);
                    break;
                case Enums.UserRole.Cleaning:
                    _mainVM.CurrentView = new MaidViewModel(_mainVM, user); 
                    break;
                case Enums.UserRole.Client:
                    _mainVM.CurrentView = new ClientDashboardViewModel(_mainVM, user);
                    break;
                case Enums.UserRole.Cook:
                    _mainVM.CurrentView = new KitchenViewModel(_mainVM, user);
                    break;
                case Enums.UserRole.SpaStaff:
                    _mainVM.CurrentView = new SpaStaffViewModel(_mainVM, user);
                    break;
                default:
                    MessageBox.Show("Rol de utilizator necunoscut.");
                    break;
            }
        }
    }
}
