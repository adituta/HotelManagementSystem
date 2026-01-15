using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace HotelManagementSystem.Helpers
{
    /// <summary>
    /// A custom Calendar that disables AutomationPeers to prevent crashes 
    /// when using custom ControlTemplates in some environments.
    /// </summary>
    public class CrashSafeCalendar : Calendar
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // Returning null prevents the creation of the standard CalendarAutomationPeer,
            // which handles the crash reported in the 'Receptie' view.
            return new FrameworkElementAutomationPeer(this);
        }
    }
}
