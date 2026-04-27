using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace CFMS_WPF.Pages.PopUp_Windows
{
	public partial class Admin_Confirmation : Window
	{
		MySqlConnection con;
		MySqlCommand cmd;
		MySqlDataReader rdr;

		public Admin_Confirmation()
		{
			InitializeComponent();
			SetConnection();

		}

		private void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}

		public bool IsAuthenticated { get; private set; } = false;
		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				con.Open();
				using (var cmd = new MySqlCommand(@"SELECT * FROM users WHERE username = @user AND password = @pass", con))
				{
					cmd.Parameters.AddWithValue("@user", Confirm_UsernameBox.Text);
					cmd.Parameters.AddWithValue("@pass", Confirm_PasswordBox.Password);

					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.HasRows)
						{
							MessageBox.Show("Password Accepted");
							IsAuthenticated = true;
							this.DialogResult = true;
							this.Close();
						}
						else
						{
							MessageBox.Show("Wrong Password");
							IsAuthenticated = false;
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
	}
}
