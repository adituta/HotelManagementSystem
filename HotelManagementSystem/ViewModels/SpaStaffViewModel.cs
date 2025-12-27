using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace HotelManagementSystem.ViewModels
{
    public class SpaStaffViewModel : BaseViewModel
    {
        public ObservableCollection<SpaAppointment> PendingAppointments { get; set; }
        public RelayCommand AcceptCommand { get; }

        public SpaStaffViewModel()
        {
            LoadData();
            AcceptCommand = new RelayCommand(o => ExecuteAccept(o as SpaAppointment));
        }

        private void LoadData()
        {
            using (var db = new HotelDBContext())
            {
                var list = db.SpaAppointments.Include("Reservation.User").Include("SpaService")
                    .Where(a => !a.IsConfirmed && a.AppointmentDate >= DateTime.Today).ToList();
                PendingAppointments = new ObservableCollection<SpaAppointment>(list);
                OnPropertyChanged(nameof(PendingAppointments));
            }
        }

        private void ExecuteAccept(SpaAppointment app)
        {
            if (app == null) return;
            using (var db = new HotelDBContext())
            {
                var dbApp = db.SpaAppointments.Find(app.Id);
                dbApp.IsConfirmed = true;
                db.SaveChanges();
                NotificationService.Send(app.Reservation.UserId, $"SPA: Programarea la {app.SpaService.Name} a fost acceptată!");
                LoadData();
            }
        }
    }
}