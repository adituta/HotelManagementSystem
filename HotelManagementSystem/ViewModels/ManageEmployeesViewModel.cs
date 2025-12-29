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
    public class ManageEmployeesViewModel : BaseViewModel
    {
        private ObservableCollection<User> _employeesList;
        private User _newUser;

        public ObservableCollection<User> EmployeesList
        {
            get { return _employeesList; }
            set { _employeesList = value; OnPropertyChanged("EmployeesList"); }
        }

        public User NewUser
        {
            get { return _newUser; }
            set { _newUser = value; OnPropertyChanged("NewUser"); }
        }

        public List<UserRole> RoleTypes { get { return Enum.GetValues(typeof(UserRole)).Cast<UserRole>().ToList(); } }

        public RelayCommand SaveEmployeeCommand { get; private set; }

        public ManageEmployeesViewModel()
        {
            NewUser = new User();
            LoadEmployees();
            SaveEmployeeCommand = new RelayCommand(o => ExecuteSave());
        }

        private void LoadEmployees()
        {
            using (var db = new HotelDBContext())
            {
                // Luăm toți utilizatorii care NU sunt clienți
                var list = db.Users.Where(u => u.Role != UserRole.Client).ToList();
                EmployeesList = new ObservableCollection<User>(list);
            }
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrEmpty(NewUser.Username) || string.IsNullOrEmpty(NewUser.Password))
            {
                MessageBox.Show("Username-ul și Parola sunt obligatorii!");
                return;
            }

            using (var db = new HotelDBContext())
            {
                db.Users.Add(NewUser);
                db.SaveChanges();
            }

            MessageBox.Show("Angajat adăugat cu succes!");
            LoadEmployees(); // Refresh listă
            NewUser = new User(); // Reset formular
        }
    }
}
