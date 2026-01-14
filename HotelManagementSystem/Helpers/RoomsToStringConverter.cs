using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Helpers
{
    public class RoomsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rooms = value as ICollection<Room>;
            if (rooms != null && rooms.Count > 0)
            {
                return "Cam: " + string.Join(", ", rooms.Select(r => r.RoomNumber));
            }
            return "Fără Cameră";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
