using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views
{
    public partial class ReceptionMapView : UserControl
    {
        public ReceptionMapView()
        {
            InitializeComponent();
            this.DataContextChanged += ReceptionMapView_DataContextChanged;
        }

        private void ReceptionMapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is ReceptionMapViewModel vm)
            {
                // Ne abonăm la evenimentul din ViewModel
                vm.OnScheduleLoaded += UpdateCalendarBlackouts;
            }
        }

        private void UpdateCalendarBlackouts(System.Collections.Generic.List<HotelManagementSystem.Models.Reservation> reservations)
        {
            RoomCalendar.BlackoutDates.Clear();

            foreach (var res in reservations)
            {
                // Adăugăm intervalul ca indisponibil pe calendar
                // BlackoutDates nu acceptă range-uri invalide, deci verificăm
                if (res.CheckOutDate > res.CheckInDate)
                {
                    // Blocăm de la checkin până la checkout
                    RoomCalendar.BlackoutDates.Add(new CalendarDateRange(res.CheckInDate, res.CheckOutDate.AddDays(-1)));
                    // AddDays(-1) pentru că în ziua de checkout camera se eliberează la prânz, 
                    // dar pentru simplitate o marcăm ocupată sau liberă în funcție de politica hotelului.
                    // De obicei o blocăm complet pentru vizualizare clară.
                }
            }
        }
    }
}