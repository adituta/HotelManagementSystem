using HotelManagementSystem.Views;

namespace HotelManagementSystem.Helpers
{
    public static class MessageBoxHelper
    {
        public static void Show(string message, string title)
        {
            var msgBox = new CustomMessageBoxWindow(message, title, showCancelButton: false);
            msgBox.ShowDialog();
        }

        public static bool ShowYesNo(string message, string title)
        {
            var msgBox = new CustomMessageBoxWindow(message, title, showCancelButton: true);
            msgBox.ShowDialog();
            return msgBox.IsConfirmed;
        }
    }
}
