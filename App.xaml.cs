using NBI_Login;
using System;
using System.Diagnostics;
using System.Windows;

namespace CFMS_WPF
{

	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			NetworkShare.MapShare();

			Login login = new Login();
			login.Show();
		}
	}

	public static class NetworkShare
	{
		public static void MapShare()
		{
			try
			{
				string serverIP = System.Configuration.ConfigurationManager.AppSettings["DbServer"];
				string shareName = System.Configuration.ConfigurationManager.AppSettings["ShareName"];
				string shareUser = System.Configuration.ConfigurationManager.AppSettings["ShareUser"];
				string sharePass = System.Configuration.ConfigurationManager.AppSettings["SharePass"];

				string uncPath = $@"\\{serverIP}\{shareName}";

				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "net",
					Arguments = $@"use ""{uncPath}"" /user:{shareUser} {sharePass} /persistent:yes",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};

				using (Process p = Process.Start(psi))
				{
					p.WaitForExit();
				}
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show("Network share error:\n" + ex.Message,
					"Network Error",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Warning);
			}
		}
	}
}
