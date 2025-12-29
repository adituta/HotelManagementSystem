using HotelManagementSystem.Enums;
using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System;

namespace HotelManagementSystem.ViewModels
{
    public class KitchenViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private readonly User _currentUser;

        public ObservableCollection<FoodOrder> ActiveOrders { get; set; }

        public RelayCommand AdvanceStatusCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        public KitchenViewModel(MainViewModel mainVM, User user)
        {
            _mainVM = mainVM;
            _currentUser = user;
            
            ActiveOrders = new ObservableCollection<FoodOrder>();

            AdvanceStatusCommand = new RelayCommand(ExecuteAdvanceStatus);
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));
            RefreshCommand = new RelayCommand(o => LoadOrders());

            LoadOrders();
        }

        private void LoadOrders()
        {
            using (var db = new HotelDBContext())
            {
                // Încărcăm comenzile care NU sunt complet finalizate (Served) sau Anulate
                var orders = db.FoodOrders
                    .Include(f => f.Reservation.Rooms) // Pentru a vedea camera
                    .Include(f => f.MenuItem)
                    .Where(f => f.Status != OrderStatus.Served && f.Status != OrderStatus.Cancelled)
                    .OrderBy(f => f.OrderDate)
                    .ToList();

                ActiveOrders.Clear();
                foreach (var order in orders)
                {
                    // Hack pentru vechile comenzi cu Status=0 -> Pending
                    if ((int)order.Status == 0) order.Status = OrderStatus.Pending;
                    ActiveOrders.Add(order);
                }
            }
        }

        private void ExecuteAdvanceStatus(object parameter)
        {
            var order = parameter as FoodOrder;
            if (order != null)
            {
                using (var db = new HotelDBContext())
                {
                    // Folosim Include pentru a avea acces la Reservation si UserId pentru notificare
                    var dbOrder = db.FoodOrders.Include("Reservation").FirstOrDefault(x => x.Id == order.Id);

                    if (dbOrder != null)
                    {
                        // Logica de avansare status
                        switch (dbOrder.Status)
                        {
                            case OrderStatus.Pending: // 0 sau 1
                                dbOrder.Status = OrderStatus.Cooking; // Sărim direct la Cooking/Accepted
                                break;
                            case OrderStatus.Cooking: // 3
                                dbOrder.Status = OrderStatus.Served; // Finalizat
                                
                                // NOTIFICARE CLIENT
                                var notif = new Notification
                                {
                                    UserId = dbOrder.Reservation.UserId,
                                    Message = $"Comanda dvs. ({dbOrder.FoodDetails}) este gata și a fost servită! Poftă bună!",
                                    CreatedAt = DateTime.Now,
                                    IsRead = false
                                };
                                db.Notifications.Add(notif);

                                break;
                            default:
                                if ((int)dbOrder.Status == 0) dbOrder.Status = OrderStatus.Cooking;
                                break;
                        }
                        
                        db.SaveChanges();
                        LoadOrders(); // Reîncărcăm lista
                    }
                }
            }
        }
    }
}
