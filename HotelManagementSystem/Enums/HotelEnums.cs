using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Enums
{
    public enum UserRole
    {
        Administrator =1,
        Receptionist = 2,
        Cleaning = 3,
        Cook = 4,
        SpaStaff = 5,
        Client=6
    }

    public enum RoomType
    {
        Double = 1,
        Triple = 2,
    }

    public enum RoomStatus
    {
        Free=1,
        Occupied=2,
        CleaningRequired=3,
        CleaningInProgress=4
    }

    public enum ReservationStatus
    {
        Pending=1,
        Active = 2,
        Completed = 3,
        Cancelled = 4
    }


    public enum ServiceType
    {
        Sauna=1,
        Massage=2
    }

    public enum OrderStatus
    {
        Pending = 1,
        Accepted = 2,
        Cooking = 3,
        Served = 4,
        Cancelled = 5
    }
}
