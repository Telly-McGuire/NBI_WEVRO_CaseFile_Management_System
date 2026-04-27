using CFMS_WPF;
using CFMS_WPF.Data;
using CFMS_WPF.Pages.PopUp_Windows;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Media;
using YourNamespace.ViewModels;

namespace NBI_Login
{
	public partial class Login : Window
	{
		MySqlConnection con;
		MySqlCommand cmd;
		MySqlDataReader rdr;

		private bool _isPasswordVisible = false;
		public string AppVersion { get; }


		public Login()
		{
			InitializeComponent();
			SetConnection();

			AppVersion = AppVersionHelper.Version;
			//clock
			DataContext = new MainViewModel();
		}
		private void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}

		private void TogglePassword_Click(object sender, RoutedEventArgs e)
		{
			if (_isPasswordVisible)
			{
				// Hide password
				login_PasswordBox.Password = VisiblePasswordBox.Text;
				login_PasswordBox.Visibility = Visibility.Visible;
				VisiblePasswordBox.Visibility = Visibility.Collapsed;

				// Change icon to eye-slash
				EyeIcon.Data = Geometry.Parse("M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7 7 0 0 0-2.79.588l.77.771A6 6 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13 13 0 0 1 14.828 8q-.086.13-.195.288c-.335.48-.83 1.12-1.465 1.755q-.247.248-.517.486zM11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829M3.35 5.47q-.27.24-.518.487A13 13 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7 7 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12z");

				_isPasswordVisible = false;
			}
			else
			{
				// Show password
				VisiblePasswordBox.Text = login_PasswordBox.Password;
				VisiblePasswordBox.Visibility = Visibility.Visible;
				login_PasswordBox.Visibility = Visibility.Collapsed;

				// Change icon to open eye
				EyeIcon.Data = Geometry.Parse("M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM8 12.5A4.5 4.5 0 1 1 8 3.5a4.5 4.5 0 0 1 0 9zM8 6a2 2 0 1 0 0 4 2 2 0 0 0 0-4z");

				_isPasswordVisible = true;
			}
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			CurrentUser.CurrentFullName = "";
			CurrentUser.CurrentUsername = login_UsernameBox.Text;
			CurrentUser.CurrentPassword = login_PasswordBox.Password;
			CurrentUser.CurrentRole = "";

			try
			{
				con.Open();
				using (var cmd = new MySqlCommand(@"SELECT full_name, role 
													FROM users 
													WHERE username = @user AND password = @pass", con))
				{
					cmd.Parameters.AddWithValue("@user", login_UsernameBox.Text);
					cmd.Parameters.AddWithValue("@pass", login_PasswordBox.Password);

					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.Read())
						{
							CurrentUser.CurrentFullName = rdr["full_name"].ToString();
							CurrentUser.CurrentRole = rdr["role"].ToString();

							MessageBox.Show("Login Successful");
							var MainWindow = new MainWindow();
							MainWindow.Show();
							this.Close();

						}
						else
						{
							MessageBox.Show("Wrong Password");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message);
			}
			finally
			{
				if (con != null && con.State == System.Data.ConnectionState.Open)
					con.Close();
			}
		}

		private void Login_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				Login_Click(sender, new RoutedEventArgs());
			}
		}

		private void btn_AddProfile_Click(object sender, RoutedEventArgs e)
		{
			Admin_Confirmation pop = new Admin_Confirmation();

			bool? result = pop.ShowDialog();

			if (result == true && pop.IsAuthenticated)
			{
				AddProfile addProfileWindow = new AddProfile();
				addProfileWindow.ShowDialog();
			}
			else
			{
				MessageBox.Show("Access denied.");
			}
		}

		private void login_UsernameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}
	}
}
