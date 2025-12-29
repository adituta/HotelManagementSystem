using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class SpaStaffViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private readonly User _currentUser;

        // Folosim o listă generală pentru a afișa și cele acceptate, nu doar cele pending
        public ObservableCollection<SpaAppointment> Appointments { get; set; }
        
        public RelayCommand AcceptCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        public SpaStaffViewModel(MainViewModel mainVM, User user)
        {
            _mainVM = mainVM;
            _currentUser = user;

            LoadData();
            
            AcceptCommand = new RelayCommand(o => ExecuteAccept(o as SpaAppointment));
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));
            RefreshCommand = new RelayCommand(o => LoadData());
        }

        private void LoadData()
        {
            using (var db = new HotelDBContext())
            {
                // Încărcăm cererile de azi (Pending și Confirmed) ca să avem o vizibilitate completă
                var list = db.SpaAppointments
                    .Include("Reservation.User")
                    .Include("SpaService")
                    .Where(a => a.AppointmentDate >= DateTime.Today)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                Appointments = new ObservableCollection<SpaAppointment>(list);
                OnPropertyChanged("Appointments");
                OnPropertyChanged("ActiveCount");
            }
        }
        
        public int ActiveCount => Appointments != null ? Appointments.Count(a => !a.IsConfirmed) : 0;

        private void ExecuteAccept(SpaAppointment app)
        {
            if (app == null) return;
            using (var db = new HotelDBContext())
            {
                // 1. Verificăm disponibilitatea (Maxim 6 persoane pe slot orar)
                var existingCount = db.SpaAppointments
                    .Where(a => a.IsConfirmed 
                             && a.AppointmentDate == app.AppointmentDate 
                             && a.StartTime == app.StartTime)
                    .Sum(a => (int?)a.PersonsCount) ?? 0;

                int requestedSeats = app.PersonsCount;

                if (existingCount + requestedSeats > 6)
                {
                    MessageBox.Show($"Nu se poate accepta cererea! Slotul de la ora {app.StartTime} are deja {existingCount} persoane programate. Capacitate maximă: 6.", 
                                    "Slot Indisponibil", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Acceptăm rezervarea
                var dbApp = db.SpaAppointments.Find(app.Id);
                if (dbApp != null)
                {
                    dbApp.IsConfirmed = true;
                    db.SaveChanges();
                    
                    NotificationService.Send(app.Reservation.UserId, string.Format("SPA: Programarea la {0} a fost acceptată pentru ora {1}!", app.SpaService.Name, app.StartTime));
                    
                    LoadData(); // Reîncărcăm lista
                }
            }
        }
    }
}