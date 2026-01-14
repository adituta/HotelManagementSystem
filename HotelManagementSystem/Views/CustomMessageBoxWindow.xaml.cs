using System.Windows;

namespace HotelManagementSystem.Views
{
    public partial class CustomMessageBoxWindow : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        public CustomMessageBoxWindow(string message, string title, bool showCancelButton = false)
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;

            if (showCancelButton)
            {
                CancelButton.Visibility = Visibility.Visible;
                OkButton.Content = "Da"; // Daca avem cancel, OK devine DA de obicei
            }
            else
            {
                CancelButton.Visibility = Visibility.Collapsed;
                OkButton.Content = "OK";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
