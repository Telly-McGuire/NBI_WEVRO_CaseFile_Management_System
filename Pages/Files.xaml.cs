
using CFMS_WPF.Data;
using CFMS_WPF.Pages.PopUp_Windows;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CFMS_WPF.FilterPopup;

namespace CFMS_WPF
{

	public partial class Files : Page
	{
		private MySqlConnection con;
		private MySqlCommand cmd;
		private MySqlDataReader rdr;

		public Files()
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

		private void LoadFiles(string searchText = null, FilterData filter = null)
		{
			try
			{
				dataFiles.ItemsSource = null;

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

				//filter function
				if (filter != null)
				{
					if (filter.filter_year != 0)
					{
						cmd.CommandText += " AND cd.case_year = @year";
						cmd.Parameters.AddWithValue("@year", filter.filter_year);
					}

					if (!string.IsNullOrEmpty(filter.filter_casetype))
					{
						cmd.CommandText += " AND ct.type_name = @caseType";
						cmd.Parameters.AddWithValue("@caseType", filter.filter_casetype);
					}

					if (filter.filter_caseno != 0)
					{
						cmd.CommandText += " AND cd.case_number = @caseNo";
						cmd.Parameters.AddWithValue("@caseNo", filter.filter_caseno);
					}

					if (!string.IsNullOrEmpty(filter.filter_agent_name))
					{
						cmd.CommandText += " AND ai.last_name = @agentName";
						cmd.Parameters.AddWithValue("@agentName", filter.filter_agent_name);
					}
				}

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
				dataFiles.ItemsSource = list;

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
		private List<string> GetSearchSuggestions(string searchText)
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

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string searchText = SearchBox.Text.Trim();

			if (string.IsNullOrEmpty(searchText))
			{
				SearchPopup.IsOpen = false;
				LoadFiles();
				return;
			}

			// Load filtered data in DataGrid
			LoadFiles(searchText);

			// Get suggestions
			var suggestions = GetSearchSuggestions(searchText);

			if (suggestions.Count > 0)
			{
				SearchSuggestions.ItemsSource = suggestions;
				SearchPopup.IsOpen = true;
			}
			else
			{
				SearchPopup.IsOpen = false;
			}
		}

		private void SearchSuggestions_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (SearchSuggestions.SelectedItem != null)
			{
				string selected = SearchSuggestions.SelectedItem.ToString();

				SearchBox.Text = selected;
				SearchPopup.IsOpen = false;

				LoadFiles(selected);
			}
		}

		private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
		{
			SearchPopup.IsOpen = false;
		}

		private void FilterButton_Click(object sender, RoutedEventArgs e)
		{
			// Create and show the filter popup
			FilterPopup popup = new FilterPopup
			{
				Owner = Window.GetWindow(this)
			};

			// Positioning
			if (popup.Owner != null)
			{
				popup.WindowStartupLocation = WindowStartupLocation.Manual;
				popup.Left = popup.Owner.Left + popup.Owner.Width - popup.Width - 20;
				popup.Top = popup.Owner.Top + 150;
			}

			// Show popup as dialog
			bool? result = popup.ShowDialog();

			// If user clicked "Apply" or "OK"
			if (result == true)
			{
				FilterData filter = popup.SelectedFilter;

				if (filter != null)
				{
					// Call LoadFiles with filter only
					LoadFiles(filter: filter);
				}
			}
		}


		private void File_UploadClick(object sender, RoutedEventArgs e)
		{
			UploadFile popup = new UploadFile();
			popup.ShowDialog();
		}
		private void ViewFile_Click(object sender, RoutedEventArgs e)
		{
			var caseFile = (sender as FrameworkElement)?.DataContext as CaseFile;
			if (caseFile == null) return;

			// Pass the caseId into the popup
			ViewFile popup = new ViewFile(caseFile.caseId);
			popup.ShowDialog();
		}

		private void EditFile_Click(object sender, RoutedEventArgs e)
		{
			var caseFile = (sender as FrameworkElement)?.DataContext as CaseFile;
			if (caseFile == null) return;

			string role = CurrentUser.CurrentRole?.Trim().ToLower();

			// Allow admin/developer directly
			if (role == "admin" || role == "developer")
			{
				EditFile popup = new EditFile(caseFile.caseId);
				popup.ShowDialog();
				return;
			}

			// Non-admin → require confirmation
			Admin_Confirmation confirm = new Admin_Confirmation();

			bool? result = confirm.ShowDialog();

			if (result == true && confirm.IsAuthenticated)
			{
				EditFile popup = new EditFile(caseFile.caseId);
				popup.ShowDialog();
			}
			else
			{
				MessageBox.Show("Access denied. Admin approval required.");
			}
		}


		private void Dashboard_Click(object sender, RoutedEventArgs e)
		{

		}

		private void FilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			LoadFiles();
		}


	}
}
