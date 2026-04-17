using CFMS_WPF.Data;
using CFMS_WPF.Pages.PopUp_Windows;
using MySql.Data.MySqlClient;
using NBI_Login;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CFMS_WPF.Dashboard
{
	public partial class Dashboard : Page
	{
		private MySqlConnection con;
		private MySqlCommand cmd;
		private MySqlDataReader rdr;

		public Dashboard()
		{
			InitializeComponent();
			SetConnection();
			LoadFiles();
		}

		public void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}
		private void LoadFiles(string searchText = null)
		{
			try
			{
				data_DashboardAllFiles.ItemsSource = null;

				con.Open();
				cmd = new MySqlCommand(@"
						SELECT cd.case_id,
							   cd.case_year,
							   ct.type_name AS case_type,
							   cd.case_number,
							   cd.case_subject,
							   cd.case_nature,
							   cd.complainant,
							   ai.last_name AS agent_id,
							   cs.status_name AS status
						FROM case_data cd
						JOIN case_type ct ON cd.case_type = ct.type_id
						JOIN case_status cs ON cd.status = cs.status_id
						JOIN agent_info ai ON cd.agent_id = ai.agent_id
						WHERE 1=1", con);

				if (!string.IsNullOrEmpty(searchText))
				{
					cmd.CommandText += @"
						AND (
							cd.case_subject LIKE @search
							OR cd.complainant LIKE @search
							OR CONCAT(ai.first_name, ' ', ai.middle_name, ' ', ai.last_name) LIKE @search
							OR CAST(cd.case_number AS CHAR) LIKE @search
						)";

					cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");
				}
				var rdr = cmd.ExecuteReader();

				var list = new List<CaseFile>();

				while (rdr.Read())
				{
					list.Add(new CaseFile
					{
						caseId = Convert.ToInt32(rdr["case_id"]),
						caseType = rdr["case_type"].ToString(),
						caseYear = rdr["case_year"].ToString(),
						caseNo = rdr["case_number"].ToString(),
						caseSubject = rdr["case_subject"].ToString(),
						caseNature = rdr["case_nature"].ToString(),
						caseComplainant = rdr["complainant"].ToString(),
						caseAgent = rdr["agent_id"].ToString(),
						caseStatus = rdr["status"].ToString()
					});
				}
				rdr.Close();
				data_DashboardAllFiles.ItemsSource = list;

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading cases:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		private void ViewFile_Click(object sender, RoutedEventArgs e)
		{
			var caseFile = (sender as FrameworkElement)?.DataContext as CaseFile;
			if (caseFile == null) return;
			ViewFile popup = new ViewFile(caseFile.caseId);
			popup.ShowDialog();
		}

		private List<string> GetDashboardSuggestions(string searchText)
		{
			var suggestions = new List<string>();

			try
			{
				con.Open();

				MySqlCommand cmd = new MySqlCommand(@"
			SELECT DISTINCT suggestion FROM (
				SELECT CAST(cd.case_number AS CHAR) AS suggestion
				FROM case_data cd
				WHERE CAST(cd.case_number AS CHAR) LIKE @search

				UNION

				SELECT cd.case_subject
				FROM case_data cd
				WHERE cd.case_subject LIKE @search

				UNION

				SELECT cd.complainant
				FROM case_data cd
				WHERE cd.complainant LIKE @search
			) AS combined
			LIMIT 10", con);

				cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

				var rdr = cmd.ExecuteReader();

				while (rdr.Read())
				{
					suggestions.Add(rdr["suggestion"].ToString());
				}

				rdr.Close();
			}
			catch { }
			finally
			{
				con.Close();
			}

			return suggestions;
		}

		private void DashboardSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			string searchText = txt_DashboardSearch.Text.Trim();

			if (string.IsNullOrEmpty(searchText))
			{
				DashboardSearchPopup.IsOpen = false;
				LoadFiles();
				return;
			}

			// Filter grid
			LoadFiles(searchText);

			// Suggestions
			var suggestions = GetDashboardSuggestions(searchText);

			if (suggestions.Count > 0)
			{
				DashboardSearchSuggestions.ItemsSource = suggestions;
				DashboardSearchPopup.IsOpen = true;
			}
			else
			{
				DashboardSearchPopup.IsOpen = false;
			}
		}
		private void DashboardSuggestions_Click(object sender, MouseButtonEventArgs e)
		{
			if (DashboardSearchSuggestions.SelectedItem != null)
			{
				string selected = DashboardSearchSuggestions.SelectedItem.ToString();

				txt_DashboardSearch.Text = selected;
				DashboardSearchPopup.IsOpen = false;

				LoadFiles(selected);
			}
		}
		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string searchText = txt_DashboardSearch.Text.Trim();

			// If empty → reset
			if (string.IsNullOrEmpty(searchText))
			{
				DashboardSearchPopup.IsOpen = false;
				LoadFiles(); // load all
				return;
			}

			// 🔍 Filter DataGrid
			LoadFiles(searchText);

			// 📋 Get suggestions
			var suggestions = GetDashboardSuggestions(searchText);

			if (suggestions.Count > 0)
			{
				DashboardSearchSuggestions.ItemsSource = suggestions;
				DashboardSearchPopup.IsOpen = true;
			}
			else
			{
				DashboardSearchPopup.IsOpen = false;
			}
		}

		private void LoadServiceFiles()
		{
			try
			{
				data_DashboardAllFiles.ItemsSource = null;

				con.Open();
				cmd = new MySqlCommand(@"
                SELECT cd.case_year,
                       ct.type_name AS case_type,
                       cd.case_number,
                       cd.case_subject,
                       cd.case_nature,
                       cd.complainant,
                       ai.last_name AS agent_id,
                       cs.status_name AS status
                FROM case_data cd
                JOIN case_type ct ON cd.case_type = ct.type_id
                JOIN case_status cs ON cd.status = cs.status_id
                JOIN agent_info ai ON cd.agent_id = ai.agent_id
                WHERE ct.type_name = 'Service';", con);

				var rdr = cmd.ExecuteReader();
				var list = new List<CaseFile>();

				while (rdr.Read())
				{
					list.Add(new CaseFile
					{
						caseType = rdr["case_type"].ToString(),
						caseYear = rdr["case_year"].ToString(),
						caseNo = rdr["case_number"].ToString(),
						caseSubject = rdr["case_subject"].ToString(),
						caseNature = rdr["case_nature"].ToString(),
						caseComplainant = rdr["complainant"].ToString(),
						caseAgent = rdr["agent_id"].ToString(),
						caseStatus = rdr["status"].ToString()
					});
				}
				rdr.Close();
				data_DashboardAllFiles.ItemsSource = list;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading service cases:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		private void Services_Click(object sender, RoutedEventArgs e)
		{
			LoadServiceFiles();
		}

		private void LoadCrimeFiles()
		{
			try
			{
				data_DashboardAllFiles.ItemsSource = null;
				con.Open();
				cmd = new MySqlCommand(@"
				SELECT cd.case_year,
					   ct.type_name AS case_type,
					   cd.case_number,
					   cd.case_subject,
					   cd.case_nature,
					   cd.complainant,
					   ai.last_name AS agent_id,
					   cs.status_name AS status
				FROM case_data cd
				JOIN case_type ct ON cd.case_type = ct.type_id
				JOIN case_status cs ON cd.status = cs.status_id
				JOIN agent_info ai ON cd.agent_id = ai.agent_id
				WHERE ct.type_name = 'Crime';", con);
				var rdr = cmd.ExecuteReader();
				var list = new List<CaseFile>();
				while (rdr.Read())
				{
					list.Add(new CaseFile
					{
						caseType = rdr["case_type"].ToString(),
						caseYear = rdr["case_year"].ToString(),
						caseNo = rdr["case_number"].ToString(),
						caseSubject = rdr["case_subject"].ToString(),
						caseNature = rdr["case_nature"].ToString(),
						caseComplainant = rdr["complainant"].ToString(),
						caseAgent = rdr["agent_id"].ToString(),
						caseStatus = rdr["status"].ToString()
					});
				}
				rdr.Close();
				data_DashboardAllFiles.ItemsSource = list;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading crime cases:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		private void Crime_Click(object sender, RoutedEventArgs e)
		{
			LoadCrimeFiles();
		}

		private void LoadMiscFiles()
		{
			try
			{
				data_DashboardAllFiles.ItemsSource = null;
				con.Open();
				cmd = new MySqlCommand(@"
				SELECT cd.case_year,
					   ct.type_name AS case_type,
					   cd.case_number,
					   cd.case_subject,
					   cd.case_nature,
					   cd.complainant,
					   ai.last_name AS agent_id,
					   cs.status_name AS status
				FROM case_data cd
				JOIN case_type ct ON cd.case_type = ct.type_id
				JOIN case_status cs ON cd.status = cs.status_id
				JOIN agent_info ai ON cd.agent_id = ai.agent_id
				WHERE ct.type_name NOT IN ('Service', 'Crime');", con);
				var rdr = cmd.ExecuteReader();
				var list = new List<CaseFile>();
				while (rdr.Read())
				{
					list.Add(new CaseFile
					{
						caseType = rdr["case_type"].ToString(),
						caseYear = rdr["case_year"].ToString(),
						caseNo = rdr["case_number"].ToString(),
						caseSubject = rdr["case_subject"].ToString(),
						caseNature = rdr["case_nature"].ToString(),
						caseComplainant = rdr["complainant"].ToString(),
						caseAgent = rdr["agent_id"].ToString(),
						caseStatus = rdr["status"].ToString()
					});
				}
				rdr.Close();
				data_DashboardAllFiles.ItemsSource = list;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading miscellaneous cases:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		private void Misc_Click(object sender, RoutedEventArgs e)
		{
			LoadMiscFiles();
		}


		private void Logout_Click(object sender, RoutedEventArgs e)
		{
			new Login().Show();
			//this.Close();
		}
	}
}