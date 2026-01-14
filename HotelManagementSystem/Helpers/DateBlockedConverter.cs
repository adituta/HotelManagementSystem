using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Helpers
{
    public class DateBlockedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Value 0: The date of the calendar button
                // Value 1: The list of reservations
                if (values == null || values.Length < 2)
                    return false;

                if (!(values[0] is DateTime date))
                    return false;

                if (!(values[1] is List<Reservation> reservations) || reservations == null)
                    return false;

                // Check if the date is inside any reservation range
                foreach (var res in reservations)
                {
                    // CheckIn inclusive, CheckOut exclusive usually for hotel logic, 
                    // but visual calendar often blocks the night.
                    // Let's assume standard logic: Occupied from CheckIn date up to (but not including) CheckOut date.
                    // Wait, existing logic was: r.CheckInDate.Date <= date < r.CheckOutDate.Date
                    
                    if (date.Date >= res.CheckInDate.Date && date.Date < res.CheckOutDate.Date)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
