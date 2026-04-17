using CFMS_WPF.Data;
using NBI_Login;
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

namespace CFMS_WPF.Pages
{
	public partial class Logout : Page
	{
		public Logout()
		{
			InitializeComponent();
			ShowLogoutDialog();
		}

        private void ShowLogoutDialog()
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear user session
                CurrentUser.CurrentUsername = string.Empty;
                CurrentUser.CurrentFullName = string.Empty;
                CurrentUser.CurrentPassword = string.Empty;

                // Open Login Window
                var loginWindow = new NBI_Login.Login();
                loginWindow.Show();

				// Close MainWindow
				Window mainWindow = Application.Current.Windows
					.OfType<MainWindow>()
					.FirstOrDefault();

				mainWindow?.Close();
			}
            else
            {
				if (this.NavigationService != null && this.NavigationService.CanGoBack)
				{
					this.NavigationService.GoBack();
				}
			}
        }
	}
}
