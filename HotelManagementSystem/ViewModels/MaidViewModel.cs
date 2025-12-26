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
        private User _currentMaid;
        private ObservableCollection<Room> _assignedRooms;

        public ObservableCollection<Room> AssignedRooms
        {
            get => _assignedRooms;
            set { _assignedRooms = value; OnPropertyChanged(nameof(AssignedRooms)); }
        }

        public int? MyFloor => _currentMaid.AssignedFloor;

        public RelayCommand StartCleaningCommand { get; }
        public RelayCommand FinishCleaningCommand { get; }

        public MaidViewModel(User maid)
        {
            _currentMaid = maid;
            LoadAssignedRooms();

            StartCleaningCommand = new RelayCommand(room => ExecuteUpdateStatus(room as Room, RoomStatus.CleaningInProgress));
            FinishCleaningCommand = new RelayCommand(room => ExecuteUpdateStatus(room as Room, RoomStatus.Free));
        }

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
}
}
