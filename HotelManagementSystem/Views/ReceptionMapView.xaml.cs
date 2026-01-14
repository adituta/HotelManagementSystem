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
            var vm = this.DataContext as ReceptionMapViewModel;
            if (vm != null)
            {
                // Ne abonăm la evenimentul din ViewModel
                vm.OnScheduleLoaded += UpdateCalendarBlackouts;
            }
        }

        private void UpdateCalendarBlackouts(System.Collections.Generic.List<HotelManagementSystem.Models.Reservation> reservations)
        {
            // Executăm asincron pe UI thread pentru a lăsa randarea să se termine
            // Asta previne crash-ul de AutomationPeer (System.ArgumentNullException)
            Dispatcher.InvokeAsync(() =>
            {
                try 
                {
                    RoomCalendar.BlackoutDates.Clear();
                    
                    if (reservations == null) return;

                    DateTime minDate = RoomCalendar.DisplayDateStart ?? DateTime.MinValue;

                    foreach (var res in reservations)
                    {
                        DateTime rawStart = res.CheckInDate.Date;
                        DateTime rawEnd = res.CheckOutDate.AddDays(-1).Date;

                        DateTime effectiveStart = rawStart < minDate ? minDate : rawStart;
                        DateTime effectiveEnd = rawEnd;

                        if (effectiveEnd < effectiveStart) continue;

                        try
                        {
                            RoomCalendar.BlackoutDates.Add(new CalendarDateRange(effectiveStart, effectiveEnd));
                        }
                        catch (ArgumentOutOfRangeException) { }
                    }
                }
                catch (Exception ex)
                {
                    // Ignorăm erorile de UI minore pentru a nu bloca aplicația
                    System.Diagnostics.Debug.WriteLine("Calendar Update Error: " + ex.Message);
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}