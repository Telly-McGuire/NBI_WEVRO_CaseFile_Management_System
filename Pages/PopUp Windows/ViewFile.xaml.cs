using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CFMS_WPF
{
	public partial class ViewFile : Window
	{
		private MySqlDataReader rdr;
		private MySqlConnection con;
		private MySqlCommand cmd;
		private int _caseId;

		public ViewFile(int caseId)
		{
			InitializeComponent();
			SetConnection();
			_caseId = caseId;

			LoadCase();
			LoadPDF();
		}

		public void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}

		private void LoadCase()
		{
			try
			{
				con.Open();
				string sql = @"
				SELECT ct.type_name AS case_type,
					   c.case_year,
					   c.case_number,
					   c.case_subject,
					   c.case_nature,
					   c.complainant,
					   a.last_name AS agent_name,
					   s.status_name AS status
				FROM case_data c
				INNER JOIN case_type ct ON c.case_type = ct.type_id
				INNER JOIN agent_info a ON c.agent_id = a.agent_id
				INNER JOIN case_status s ON c.status = s.status_id
				WHERE c.case_id = @caseId";

				cmd = new MySqlCommand(sql, con);
				cmd.Parameters.AddWithValue("@caseId", _caseId);
				rdr = cmd.ExecuteReader();

				if (rdr.Read())
				{
					txt_ViewCaseType.Text = rdr["case_type"].ToString();
					txt_ViewYear.Text = rdr["case_year"].ToString();
					txt_ViewCaseNo.Text = rdr["case_number"].ToString();
					txt_ViewSubject.Text = rdr["case_subject"].ToString();
					txt_ViewNature.Text = rdr["case_nature"].ToString();
					txt_ViewComplaints.Text = rdr["complainant"].ToString();
					txt_ViewAgent.Text = rdr["agent_name"].ToString();
					txt_ViewStatus.Text = rdr["status"].ToString();
				}

				rdr.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading case details:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		private async void LoadPDF()
		{
			try
			{
				con.Open();
				string sql = @"
					SELECT d.file_path, d.file_name, d.uploaded_at, 
						   u.username AS uploaded_by
					FROM case_documents d
					INNER JOIN users u ON d.uploaded_by = u.user_id
					WHERE d.case_id = @caseId
					LIMIT 1";

				cmd = new MySqlCommand(sql, con);
				cmd.Parameters.AddWithValue("@caseId", _caseId);
				rdr = cmd.ExecuteReader();

				if (rdr.Read())
				{
					string serverIP = System.Configuration.ConfigurationManager.AppSettings["DbServer"];

					string filePath = rdr["file_path"].ToString();

					// Normalize all slashes to backslashes
					filePath = filePath.Replace("/", "\\");

					// Remove leading \\ or \\ equivalent so we can rebuild cleanly
					// Handles both \\hostname\share and //hostname/share (after replacement)
					if (filePath.StartsWith("\\\\"))
					{
						// Find where the hostname ends (after \\)
						int hostEnd = filePath.IndexOf("\\", 2);

						if (hostEnd >= 0)
						{
							// Rebuild with the IP from config, preserving the rest of the path
							string sharePath = filePath.Substring(hostEnd); // e.g. \wevro case files 2005-2022\CRIME
							filePath = $@"\\{serverIP}{sharePath}";
						}
						else
						{
							// No share found, just replace the host
							filePath = $@"\\{serverIP}";
						}
					}

					// Fill metadata and close reader BEFORE await
					txt_ViewFileName.Text = rdr["file_name"].ToString();
					UsernameText.Text = rdr["uploaded_by"].ToString();
					UploadDateText.Text = Convert.ToDateTime(rdr["uploaded_at"]).ToString("yyyy-MM-dd");

					rdr.Close();

					if (System.IO.File.Exists(filePath))
					{
						await PdfViewer.EnsureCoreWebView2Async();
						PdfViewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
						PdfViewer.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

						// Manually build the URI to handle spaces and special characters correctly
						string uri = "file:" + filePath.Replace("\\", "/");
						PdfViewer.CoreWebView2.Navigate(uri);
					}
					else
					{
						MessageBox.Show("File not found at:\n" + filePath,
										"Missing File",
										MessageBoxButton.OK,
										MessageBoxImage.Warning);
					}
				}
				else
				{
					rdr.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading PDF:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				if (con.State != System.Data.ConnectionState.Closed)
					con.Close();
			}
		}


		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			// Open the EditFile window for the current case
			try
			{
				EditFile edit = new EditFile(_caseId);
				edit.Owner = this;
				edit.ShowDialog();

				// Ensure connection is fully closed before refresh
				if (con.State != System.Data.ConnectionState.Closed)
					con.Close();

				// Refresh both panels with latest DB data
				LoadCase();
				LoadPDF();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to open editor:\n" + ex.Message,
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}





		private void Metadata_Changed(object sender, RoutedEventArgs e)
		{

		}
		private void PreviousPage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void NextPage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{

		}
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{

		}

		private void FileNameBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
		private void YearBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{

		}
		private void YearBox_Pasting(object sender, DataObjectPastingEventArgs e)
		{

		}


	}
}