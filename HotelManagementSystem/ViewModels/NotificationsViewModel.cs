using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;

namespace HotelManagementSystem.ViewModels
{
    public class NotificationsViewModel : BaseViewModel
    {
        private User _currentUser;
        public ObservableCollection<Notification> MyNotifications { get; set; }

        public NotificationsViewModel(User user)
        {
            _currentUser = user;
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            using (var db = new HotelDBContext())
            {
                var list = db.Notifications
                    .Where(n => n.UserId == _currentUser.Id)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                // Daca nu gasim notificari, cream o lista goala
                if (list == null) list = new System.Collections.Generic.List<Notification>();

                MyNotifications = new ObservableCollection<Notification>(list);
                OnPropertyChanged("MyNotifications");
            }
        }
    }
}
