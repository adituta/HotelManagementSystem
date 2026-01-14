using HotelManagementSystem.Enums;
using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.ViewModels
{
    public class MaidViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private User _currentMaid;
        private ObservableCollection<Room> _assignedRooms;

        public ObservableCollection<Room> AssignedRooms
        {
            get { return _assignedRooms; }
            set { _assignedRooms = value; OnPropertyChanged("AssignedRooms"); }
        }

        public int? MyFloor { get { return _currentMaid.AssignedFloor; } }

        public RelayCommand StartCleaningCommand { get; private set; }
        public RelayCommand FinishCleaningCommand { get; private set; }

        public MaidViewModel(MainViewModel mainVM, User maid)
        {
            _mainVM = mainVM;
            _currentMaid = maid;
            LoadAssignedRooms();

            StartCleaningCommand = new RelayCommand(room => ExecuteUpdateStatus(room as Room, RoomStatus.CleaningInProgress));
            FinishCleaningCommand = new RelayCommand(room => ExecuteFinishCleaning(room as Room));
            ViewIncomeCommand = new RelayCommand(o => _mainVM.CurrentView = new MyIncomeViewModel(_mainVM, _currentMaid));
            LogoutCommand = new RelayCommand(o => _mainVM.CurrentView = new LoginViewModel(_mainVM));
        }

        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand ViewIncomeCommand { get; private set; }

        private void LoadAssignedRooms()
        {
            using (var db = new HotelDBContext())
            {
                // FILTRARE: Doar camerele de pe etajul asignat cameristei
                var rooms = db.Rooms
                    .Where(r => r.Floor == _currentMaid.AssignedFloor)
                    .OrderBy(r => r.RoomNumber)
                    .ToList();

                AssignedRooms = new ObservableCollection<Room>(rooms);
            }
        }

        private void ExecuteUpdateStatus(Room room, RoomStatus newStatus)
        {
            if (room == null) return;

            using (var db = new HotelDBContext())
            {
                var dbRoom = db.Rooms.Find(room.Id);
                if (dbRoom != null)
                {
                    dbRoom.Status = newStatus;
                    db.SaveChanges();
                    LoadAssignedRooms(); // Refresh vizual
                }
            }
        }

        private void ExecuteFinishCleaning(Room room)
        {
            if (room == null) return;

            using (var db = new HotelDBContext())
            {
                var dbRoom = db.Rooms.Find(room.Id);
                if (dbRoom != null)
                {
                    // VERIFICARE CRITICA:
                    // Daca exista o rezervare ACTIVA care nu expira azi (CheckOutDate > terminarea curateniei),
                    // atunci camera revine la statusul OCCUPIED (Ocupata), nu FREE.
                    
                    DateTime now = DateTime.Now;
                    
                    // Cautam o rezervare activa care include aceasta camera
                    var activeRes = db.Reservations
                        .Where(r => r.Status == ReservationStatus.Active &&
                                    r.Rooms.Any(rm => rm.Id == dbRoom.Id) &&
                                    r.CheckOutDate > now) // Mai are timp de stat
                        .FirstOrDefault();

                    if (activeRes != null)
                    {
                        // E curatenie "de zi cu zi"
                        dbRoom.Status = RoomStatus.Occupied;
                    }
                    else
                    {
                        // E curatenie "de check-out" (nu mai e nimeni sau expira azi)
                        dbRoom.Status = RoomStatus.Free;
                    }

                    db.SaveChanges();
                    
                    // NOTIFICARE CLIENT CURATENIE GATA
                    int userIdToNotify = 0;
                     if (activeRes != null) userIdToNotify = activeRes.UserId;
                     // Daca nu e activeRes, poate luam din history, dar momentan notificam doar daca e cineva cazat.
                    
                     if (userIdToNotify > 0)
                     {
                         NotificationService.Send(userIdToNotify, $"Camera {dbRoom.RoomNumber} a fost curățată. Mulțumim!");
                     }
                    
                    LoadAssignedRooms();
                }
            }
        }
}
}
