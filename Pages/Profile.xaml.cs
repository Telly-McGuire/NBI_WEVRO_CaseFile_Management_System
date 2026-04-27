using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CFMS_WPF
{
	public partial class ChangePassword : Page
	{
		private MySqlConnection con;
		private MySqlCommand cmd;

		public ChangePassword()
		{
			InitializeComponent();
			SetConnection();

			LoadUser();
		}

		private void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}
		private void LoadUser()
		{
			txt_ProfileName.Text = CurrentUser.CurrentFullName;
			txt_ProfileUser.Text = CurrentUser.CurrentUsername;
			txt_Role.Text = CurrentUser.CurrentRole;
		}
		private void SetPassword_Click(object sender, RoutedEventArgs e)
		{
			string newPassword = NewPasswordBox.Password;
			string confirmPassword = ConfirmPasswordBox.Password;

			//This is just to Check if there entries are field
			if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
			{
				MessageBox.Show("Please fill in both fields.",
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Warning);
				return;
			}

			if (newPassword != confirmPassword)
			{
				MessageBox.Show("Passwords do not match.",
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
				return;
			}

			//The actual logic :3
			string currentuser = CurrentUser.CurrentUsername;
			try
			{

				con.Open();
				cmd = new MySqlCommand($@"UPDATE users SET password= '" + confirmPassword + "' WHERE username = '" + currentuser + "' ; ", con);
				int rowsAffected = cmd.ExecuteNonQuery();
				con.Close();
				if (rowsAffected > 0)
				{
					MessageBox.Show("Password successfully changed!", "Success",
									MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					MessageBox.Show("No matching user found.", "Error",
									MessageBoxButton.OK, MessageBoxImage.Error);
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error updating password: " + ex.Message, "Error",
						MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void txt_ProfileName_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
	}
}