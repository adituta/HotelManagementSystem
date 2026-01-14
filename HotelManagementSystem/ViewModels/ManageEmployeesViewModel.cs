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
        public RelayCommand PaySalaryCommand { get; private set; } // Added

        public ManageEmployeesViewModel()
        {
            NewUser = new User();
            LoadEmployees();
            SaveEmployeeCommand = new RelayCommand(o => ExecuteSave());
            PaySalaryCommand = new RelayCommand(ExecutePaySalary); // Added
        }

        private void LoadEmployees()
        {
            using (var db = new HotelDBContext())
            {
                // Luăm toți utilizatorii care NU sunt clienți și nici Administratori
                var list = db.Users.Where(u => u.Role != UserRole.Client && u.Role != UserRole.Administrator).ToList();
                EmployeesList = new ObservableCollection<User>(list);
            }
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrEmpty(NewUser.Username) || string.IsNullOrEmpty(NewUser.Password))
            {
                MessageBoxHelper.Show("Username-ul și Parola sunt obligatorii!", "Eroare");
                return;
            }

            using (var db = new HotelDBContext())
            {
                db.Users.Add(NewUser);
                db.SaveChanges();
            }

            MessageBoxHelper.Show("Angajat adăugat cu succes!", "Succes");
            LoadEmployees(); // Refresh listă
            NewUser = new User(); // Reset formular
        }

        private void ExecutePaySalary(object parameter)
        {
            var employee = parameter as User;
            if (employee == null) return;

            try
            {
                using (var db = new HotelDBContext())
                {
                    // 1. Verificăm dacă a fost plătit luna asta
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;

                    bool alreadyPaid = db.SalaryPayments.Any(p => p.UserId == employee.Id && 
                                                                  p.PaymentDate.Month == currentMonth && 
                                                                  p.PaymentDate.Year == currentYear);

                    if (alreadyPaid)
                    {
                        MessageBoxHelper.Show($"Angajatul {employee.FullName} a încasat deja salariul pe această lună!", "Atenție");
                        return;
                    }

                    // 2. Procesăm plata
                    var payment = new SalaryPayment
                    {
                        UserId = employee.Id,
                        Amount = 2500, // Suma fixă
                        PaymentDate = DateTime.Now,
                        Details = $"Salariu {DateTime.Now:MMMM yyyy} (Individual)"
                    };

                    db.SalaryPayments.Add(payment);
                    
                    // Notificare pentru angajat
                    var notif = new Notification
                    {
                        UserId = employee.Id,
                        Message = $"Ai primit salariul în valoare de {payment.Amount} RON (Plată Individuală).",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    db.Notifications.Add(notif);

                    db.SaveChanges();
                    MessageBoxHelper.Show($"Salariu plătit cu succes lui {employee.FullName}!", "Succes");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la procesarea plății: " + ex.Message, "Eroare");
            }
        }
    }
}
