using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
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
using System.Windows.Shapes;

namespace CFMS_WPF.Pages.PopUp_Windows
{
	/// <summary>
	/// Interaction logic for AddProfile.xaml
	/// </summary>
	public partial class AddProfile : Window
	{
		private MySqlConnection con;
		private MySqlCommand cmd;
		public AddProfile()
		{
			InitializeComponent();
			SetConnection();
		}

		private void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}
		private void AddProfile_Click(object sender, RoutedEventArgs e)
		{
			string profileName = txt_AddProfileName.Text.Trim();
			string profileUser = txt_AddProfileUser.Text.Trim();
			string profilePass = Add_NewPasswordBox.Password.Trim();
			string profileConfirmPass = Add_ConfirmPasswordBox.Password.Trim();

			if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(profileUser) ||
				string.IsNullOrEmpty(profilePass) || string.IsNullOrEmpty(profileConfirmPass))
			{
				MessageBox.Show("Please fill in all fields.",
								"Input Error",
								MessageBoxButton.OK,
								MessageBoxImage.Warning);
				return;
			}

			// ✅ Added password match check
			if (profilePass != profileConfirmPass)
			{
				MessageBox.Show("Passwords do not match.",
								"Validation Error",
								MessageBoxButton.OK,
								MessageBoxImage.Warning);
				return;
			}

			var confirmResult = MessageBox.Show("Are you sure you want to add this profile?",
											   "Confirm Add",
											   MessageBoxButton.YesNo,
											   MessageBoxImage.Question);

			// ✅ Fixed logic (cancel if NOT Yes)
			if (confirmResult != MessageBoxResult.Yes)
				return;

			try
			{
				con.Open();

				// ✅ Fixed SQL (parameterized, but same structure)
				cmd = new MySqlCommand(
					"INSERT INTO users " +
					"(full_name, username, password, role) " +
					"VALUES (@name, @user, @pass, @role)", con);

				cmd.Parameters.AddWithValue("@name", profileName);
				cmd.Parameters.AddWithValue("@user", profileUser);
				cmd.Parameters.AddWithValue("@pass", profilePass);
				cmd.Parameters.AddWithValue("@role", txt_AddProfileRole.Text);

				cmd.ExecuteNonQuery();

				MessageBox.Show("Profile added successfully!",
								"Success",
								MessageBoxButton.OK,
								MessageBoxImage.Information);

				clearfields();
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error adding profile:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally { con.Close(); }
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			txt_AddProfileName.Clear();
			txt_AddProfileUser.Clear();
			Add_NewPasswordBox.Clear();
			Add_ConfirmPasswordBox.Clear();

			this.Close();

		}

		private void clearfields()
		{
			txt_AddProfileName.Clear();
			txt_AddProfileUser.Clear();
			Add_NewPasswordBox.Clear();
			Add_ConfirmPasswordBox.Clear();

		}
	}
}
