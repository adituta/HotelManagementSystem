using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System; // Added

namespace HotelManagementSystem.ViewModels
{
    public class MyIncomeViewModel : BaseViewModel
    {
        public ObservableCollection<SalaryPayment> SalaryList { get; set; }
        public decimal TotalEarned { get; set; }

        private MainViewModel _main;
        private User _user;

        public RelayCommand BackCommand { get; private set; }

        public MyIncomeViewModel(MainViewModel main, User user)
        {
            _main = main;
            _user = user;
            LoadIncome();
            BackCommand = new RelayCommand(GoBack);
        }

        private void GoBack(object parameter)
        {
            switch (_user.Role)
            {
                case Enums.UserRole.Cleaning:
                    _main.CurrentView = new MaidViewModel(_main, _user);
                    break;
                case Enums.UserRole.Cook:
                    _main.CurrentView = new KitchenViewModel(_main, _user);
                    break;
                case Enums.UserRole.Receptionist:
                    _main.CurrentView = new ReceptionViewModel(_main, _user);
                    break;
                case Enums.UserRole.SpaStaff:
                    _main.CurrentView = new SpaStaffViewModel(_main, _user);
                    break;
                case Enums.UserRole.Administrator:
                    _main.CurrentView = new AdminViewModel(_main);
                    break;
                default:
                    // Fallback to login if something weird happens
                    _main.CurrentView = new LoginViewModel(_main);
                    break;
            }
        }

        private void LoadIncome()
        {
            if (_user == null)
            {
                MessageBoxHelper.Show("Eroare: Utilizatorul nu este setat.", "Eroare Internă");
                return;
            }

            try
            {
                using (var db = new HotelDBContext())
                {
                    var payments = db.SalaryPayments
                        .Where(p => p.UserId == _user.Id)
                        .OrderByDescending(p => p.PaymentDate)
                        .ToList();

                    SalaryList = new ObservableCollection<SalaryPayment>(payments);
                    TotalEarned = payments.Sum(p => p.Amount);
                    
                    OnPropertyChanged("SalaryList");
                    OnPropertyChanged("TotalEarned");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la încărcarea veniturilor: " + ex.Message, "Eroare");
                SalaryList = new ObservableCollection<SalaryPayment>(); // Prevent UI crash on null binding
            }
        }
    }
}
