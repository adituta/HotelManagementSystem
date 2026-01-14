using HotelManagementSystem.Enums;
using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using System;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class KitchenViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private readonly User _currentUser;

        public ObservableCollection<FoodOrder> ActiveOrders { get; set; }
        public ObservableCollection<FoodOrder> HistoryOrders { get; set; }

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

        public RelayCommand AdvanceStatusCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand ShowActiveCommand { get; private set; }
        public RelayCommand ShowHistoryCommand { get; private set; }
        public RelayCommand ViewIncomeCommand { get; private set; }

        public KitchenViewModel(MainViewModel mainVM, User user)
        {
            _mainVM = mainVM;
            _currentUser = user; // Confirmed assignment
            
            ActiveOrders = new ObservableCollection<FoodOrder>();
            HistoryOrders = new ObservableCollection<FoodOrder>();

            AdvanceStatusCommand = new RelayCommand(ExecuteAdvanceStatus);
            ViewIncomeCommand = new RelayCommand(o => _mainVM.CurrentView = new MyIncomeViewModel(_mainVM, _currentUser));
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));
            RefreshCommand = new RelayCommand(o => { LoadOrders(); LoadHistory(); });
            
            ShowActiveCommand = new RelayCommand(o => IsActiveView = true);
            ShowHistoryCommand = new RelayCommand(o => IsActiveView = false);

            LoadOrders();
            LoadHistory();
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new HotelDBContext())
                {
                    // Ne intereseaza doar comenzile care NU sunt Served/Cancelled
                    var orders = db.FoodOrders
                        .Include(f => f.Reservation.User)
                        .Include(f => f.MenuItem)
                        .Where(f => f.Status != OrderStatus.Served && f.Status != OrderStatus.Cancelled)
                        .OrderBy(f => f.OrderDate)
                        .ToList();

                    ActiveOrders = new ObservableCollection<FoodOrder>(orders);
                    OnPropertyChanged("ActiveOrders"); // Notificăm UI
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la încărcarea comenzilor active: " + ex.Message, "Eroare");
            }
        }

        private void LoadHistory()
        {
            try
            {
                using (var db = new HotelDBContext())
                {
                    // Încărcăm comenzile FINALIZATE
                    var history = db.FoodOrders
                        .Include(f => f.Reservation.User)
                        .Include(f => f.Reservation.Rooms) // Include Rooms for display
                        .Include(f => f.MenuItem)
                        .Where(f => f.Status == OrderStatus.Served || f.Status == OrderStatus.Cancelled)
                        .OrderByDescending(f => f.OrderDate)
                        .ToList();

                    HistoryOrders = new ObservableCollection<FoodOrder>();
                    foreach (var order in history)
                    {
                        HistoryOrders.Add(order);
                    }
                    OnPropertyChanged("HistoryOrders");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show("Eroare la încărcarea istoricului: " + ex.Message, "Eroare");
            }
        }

        private void ExecuteAdvanceStatus(object parameter)
        {
            var order = parameter as FoodOrder;
            if (order != null)
            {
                try
                {
                    using (var db = new HotelDBContext())
                    {
                        var dbOrder = db.FoodOrders.Include(f => f.Reservation).FirstOrDefault(x => x.Id == order.Id);

                        if (dbOrder != null)
                        {
                            // Logica de avansare status
                            switch (dbOrder.Status)
                            {
                                case OrderStatus.Pending:
                                    dbOrder.Status = OrderStatus.Cooking;
                                    break;
                                case OrderStatus.Cooking:
                                    dbOrder.Status = OrderStatus.Served;
                                    
                                    // NOTIFICARE CLIENT
                                    var notif = new Notification
                                    {
                                        UserId = dbOrder.Reservation.UserId,
                                        Message = $"Comanda dvs. ({dbOrder.MenuItem.Name}) este gata și a fost servită! Poftă bună!",
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
                            LoadOrders(); // Reîncărcăm lista activă
                            LoadHistory(); // Reîncărcăm istoricul
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.Show("Eroare la actualizarea statusului: " + ex.Message, "Eroare");
                }
            }
        }
    }
}
