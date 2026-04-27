using NBI_Login;
using System.Windows;

namespace CFMS_WPF
{

	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Login login = new Login();
			login.Show();
		}
	}
}
