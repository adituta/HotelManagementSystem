using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using System.Data.Entity; // Necessary for Lambda Includes

namespace HotelManagementSystem.ViewModels
{
    public class SpaStaffViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private readonly User _currentUser;

        // Folosim o listă generală pentru a afișa și cele acceptate, nu doar cele pending
        public ObservableCollection<SpaAppointment> Appointments { get; set; }
        public ObservableCollection<SpaAppointment> HistoryAppointments { get; set; }

        private bool _isActiveView = true;
        public bool IsActiveView
        {
            get => _isActiveView;
            set
            {
                _isActiveView = value;
                OnPropertyChanged(nameof(IsActiveView));
                OnPropertyChanged(nameof(IsHistoryView));
            }
        }

        public bool IsHistoryView => !IsActiveView;
        
        public RelayCommand AcceptCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand ShowActiveCommand { get; private set; }
        public RelayCommand ShowHistoryCommand { get; private set; }
        public RelayCommand ViewIncomeCommand { get; private set; }

        public SpaStaffViewModel(MainViewModel mainVM, User user)
        {
            _mainVM = mainVM;
            _currentUser = user;

            ShowActiveCommand = new RelayCommand(o => IsActiveView = true);
            ShowHistoryCommand = new RelayCommand(o => IsActiveView = false);

            LoadData();
            LoadHistory();
            
            AcceptCommand = new RelayCommand(o => ExecuteAccept(o as SpaAppointment));
            ViewIncomeCommand = new RelayCommand(o => _mainVM.CurrentView = new MyIncomeViewModel(_mainVM, _currentUser));
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));
            RefreshCommand = new RelayCommand(o => { LoadData(); LoadHistory(); });
        }

        private void LoadData()
        {
            try
            {
                using (var db = new HotelDBContext())
                {
                    // Încărcăm cererile de azi (Pending și Confirmed) ca să avem o vizibilitate completă
                    var list = db.SpaAppointments
                        .Include(a => a.Reservation.User)
                        .Include(a => a.SpaService)
                        .Where(a => a.AppointmentDate >= DateTime.Today)
                        .OrderBy(a => a.StartTime)
                        .ToList();

                    Appointments = new ObservableCollection<SpaAppointment>(list);
                    OnPropertyChanged("Appointments");
                    OnPropertyChanged("ActiveCount");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la încărcarea programărilor active: " + ex.Message, "Eroare");
            }
        }

        private void LoadHistory()
        {
            try
            {
                using (var db = new HotelDBContext())
                {
                    // Încărcăm istoricul: programări din trecut (data < azi)
                    var history = db.SpaAppointments
                        .Include(a => a.Reservation.User)
                        .Include(a => a.Reservation.Rooms) // Include Rooms for display
                        .Include(a => a.SpaService)
                        .Where(a => a.AppointmentDate < DateTime.Today)
                        .OrderByDescending(a => a.AppointmentDate)
                        .ThenByDescending(a => a.StartTime)
                        .ToList();

                    HistoryAppointments = new ObservableCollection<SpaAppointment>(history);
                    OnPropertyChanged("HistoryAppointments");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la încărcarea istoricului: " + ex.Message, "Eroare");
            }
        }
        
        public int ActiveCount => Appointments != null ? Appointments.Count(a => !a.IsConfirmed) : 0;

        private void ExecuteAccept(SpaAppointment app)
        {
            if (app == null) return;
            try
            {
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
                    MessageBoxHelper.Show($"Nu se poate accepta cererea! Slotul de la ora {app.StartTime} are deja {existingCount} persoane programate. Capacitate maximă: 6.", 
                                    "Slot Indisponibil");
                    return;
                }

                // 2. Acceptăm rezervarea
                var dbApp = db.SpaAppointments.Find(app.Id);
                if (dbApp != null)
                {
                    dbApp.IsConfirmed = true;
                    db.SaveChanges();
                    
                    // Pentru notificări, avem nevoie de userId din rezervare
                    var userId = db.SpaAppointments.Include(a => a.Reservation).Where(a => a.Id == app.Id).Select(a => a.Reservation.UserId).FirstOrDefault();
                    if (userId > 0)
                    {
                        NotificationService.Send(userId, string.Format("SPA: Programarea la {0} a fost acceptată pentru ora {1}!", app.SpaService.Name, app.StartTime));
                    }
                    
                    LoadData(); // Reîncărcăm lista
                }
            }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la acceptarea programării: " + ex.Message, "Eroare");
            }
        }
    }
}