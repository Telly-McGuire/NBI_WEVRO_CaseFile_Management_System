using CFMS_WPF.Data;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CFMS_WPF
{
	public partial class UploadFile : Window
	{
		private MySqlConnection con;
		private MySqlCommand cmd;
		private MySqlDataReader rdr;

		public UploadFile()
		{
			InitializeComponent();
			SetConnection();

			LoadAgents();
			LoadCaseTypes();
			LoadCaseStatus();
			LoadUser();
			LoadTime();
		}

		public void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}

		//case types
		private void LoadCaseTypes()
		{
			try
			{
				con.Open();
				cmd = new MySqlCommand(@"
				SELECT type_id, type_name FROM case_type
				", con);
				rdr = cmd.ExecuteReader();

				var TypeList = new List<CaseType>();

				while (rdr.Read())
				{
					TypeList.Add( new CaseType 
					{
						type_ID = Convert.ToInt32(rdr["type_id"]),
						type_name = rdr["type_name"].ToString()
					});
				}
				rdr.Close();

				Upload_CaseTypeComboBox.ItemsSource = TypeList;
				Upload_CaseTypeComboBox.DisplayMemberPath = "type_name";

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Case Types:\n" + ex.Message,
					"Database Error Whoopsie :3",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally { con.Close(); }
		}


		//AGENTS SECTION CODE :3
		private void LoadAgents()
		{
			try
			{

				con.Open();
				cmd = new MySqlCommand("SELECT agent_id, last_name, first_name, middle_name FROM agent_info", con);
				rdr = cmd.ExecuteReader();

				var Agentlist = new List<AgentData>();

				while (rdr.Read())
				{
					Agentlist.Add(new AgentData
					{
						agentID = Convert.ToInt32(rdr["agent_id"]),
						agentlastName = rdr["last_name"].ToString()
					});
				}
				rdr.Close();

				Upload_AgentNameComboBox.ItemsSource = Agentlist;
				Upload_AgentNameComboBox.DisplayMemberPath = "agentlastName";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Loading Agents:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		private void LoadCaseStatus()
		{
			try
			{
				con.Open();
				cmd = new MySqlCommand(@"SELECT status_id, status_name FROM case_status",con);
				rdr = cmd.ExecuteReader();

				var StatusList = new List<CaseStatus>();

				while (rdr.Read())
				{
					StatusList.Add(new CaseStatus
					{
						status_id = Convert.ToInt32(rdr["status_id"]),
						status_name = rdr["status_name"].ToString()
					});
				}
				rdr.Close() ;

				Upload_StatusComboBox.ItemsSource = StatusList;
				Upload_StatusComboBox.DisplayMemberPath = "status_name";

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Loading Status:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally { con.Close(); }
		}

		private void LoadUser()
		{
			txt_UploadUsername.Text = CurrentUser.CurrentUsername;
		}

		private void LoadTime()
		{
			DateTime now = DateTime.Now;
			txt_UploadDate.Text = now.ToString("yyyy-MM-dd HH:mm:ss");
		}


		private CaseMetaData SelectedDocument;
		private List<CaseMetaData> DocumentList = new List<CaseMetaData>();
		private async void OpenPdf_Click(object sender, RoutedEventArgs e)
		{
			// Ensure WebView2 is initialized
			await PdfViewer.EnsureCoreWebView2Async();
			string serverIP = System.Configuration.ConfigurationManager.AppSettings["DbServer"];
			// Open File Explorer dialog
			var dialog = new Microsoft.Win32.OpenFileDialog
			{
				Filter = "PDF files (*.pdf)|*.pdf",
				Title = "Select a PDF file",
				InitialDirectory = @"\\" + serverIP + @"\wevro case files 2005-2022"
			};

			if (dialog.ShowDialog() == true)
			{
				// The File Path and Putting the file name
				string filePath = dialog.FileName;
				txt_UploadFilePath.Text = filePath;

				string fileName = System.IO.Path.GetFileName(filePath);
				txt_UploadFileName.Text = fileName;

				// Convert to URI for WebView2
				string uri = new Uri(filePath).AbsoluteUri;

				PdfViewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
				PdfViewer.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

				// Preview the PDF in WebView2
				PdfViewer.CoreWebView2.Navigate(uri);

				SelectedDocument = new CaseMetaData
				{
					CaseFileName = fileName,
					CaseFilePath = filePath,
					CaseUploadedBy = Environment.UserName,   
					CaseUploadAt = DateTime.Now,
				};

				DocumentList.Add(SelectedDocument);
			}
		}

		private void FileNameBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
		
		private void BuildFileName()
		{
			string prefix = string.Empty;

			// Case type prefix
			if (Upload_CaseTypeComboBox.SelectedItem is CaseType selectedType)
			{
				switch (selectedType.type_name)
				{
					case "Crime": prefix = "C"; break;
					case "Service": prefix = "S"; break;
					case "Misc": prefix = "M"; break;
				}
			}

			// Gather other parts
			string year = string.Empty;
			string yearInput = txt_UploadYear.Text.Trim();
			if (int.TryParse(yearInput, out int yearValue))
			{
				year = (yearValue % 100).ToString("D2"); 
			}

			string caseNo = txt_UploadCaseNo.Text.Trim();
			string complainant = txt_UploadCaseComplainant.Text.Trim().Replace(" ", "_");
			string agent = Upload_AgentNameComboBox.SelectedItem is AgentData a ? a.agentlastName : string.Empty;
			string status = Upload_StatusComboBox.SelectedItem is CaseStatus s ? s.status_name : string.Empty;

			// Build filename parts list
			var parts = new List<string>();
			if (!string.IsNullOrEmpty(prefix)) parts.Add(prefix);
			if (!string.IsNullOrEmpty(year)) parts.Add(year);
			if (!string.IsNullOrEmpty(caseNo)) parts.Add(caseNo);
			if (!string.IsNullOrEmpty(complainant)) parts.Add(complainant);
			if (MultipleComplainantsCheckBox.IsChecked == true)
			{
				if (!parts.Contains("et.al."))
					parts.Add("et.al.");
			}
			else
			{
				parts.Remove("et.al.");
			}
			if (!string.IsNullOrEmpty(agent)) parts.Add(agent);
			if (!string.IsNullOrEmpty(status)) parts.Add(status);

			// Join with dashes
			txt_UploadFileName.Text = string.Join("-", parts);
		}

		private void Upload_Click(object sender, RoutedEventArgs e)
		{
			try 
			{
				SetConnection();
				con.Open();

				// Validate file path input
				string rawPath = txt_UploadFilePath.Text.Trim();
				if (string.IsNullOrWhiteSpace(rawPath))
				{
					MessageBox.Show("Please select a file path.",
									"Missing File",
									MessageBoxButton.OK,
									MessageBoxImage.Warning);
					return;
				}

				// Normalize path consistently (forward slashes)
				string normalizedPath = System.IO.Path.GetFullPath(rawPath).Replace("\\", "/");

				// Ensure file exists on disk
				if (!System.IO.File.Exists(normalizedPath))
				{
					MessageBox.Show("The selected file does not exist.",
									"Invalid File",
									MessageBoxButton.OK,
									MessageBoxImage.Warning);
					return;
				}


				var caseFile = new CaseFile
				{
					caseYear = txt_UploadYear.Text.Trim(),
					caseType = (Upload_CaseTypeComboBox.SelectedItem as CaseType)?.type_name,
					caseNo = txt_UploadCaseNo.Text.Trim(),
					caseSubject = txt_UploadSubject.Text.Trim(),
					caseNature = txt_UploadNature.Text.Trim(),
					caseComplainant = txt_UploadCaseComplainant.Text.Trim(),
					caseAgent = (Upload_AgentNameComboBox.SelectedItem as AgentData)?.agentlastName,
					caseStatus = (Upload_StatusComboBox.SelectedItem as CaseStatus)?.status_name,
					Document = new CaseMetaData
					{
						CaseFileName = txt_UploadFileName.Text.Trim(),
						CaseFilePath = normalizedPath,
						CaseUploadedBy = CurrentUser.CurrentUsername,
						CaseUploadAt = DateTime.Now
					}
				};

				ConfirmationPopUp popUp = new ConfirmationPopUp(caseFile);
				popUp.ShowDialog();
				ClearUploadFields();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error processing file:\n" + ex.Message,
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
		}
		private void Upload_CaseTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BuildFileName();
		}

		 //Year Handlers
		private void txt_UploadYear_Changed(object sender, TextChangedEventArgs e)
		{
			BuildFileName();
		}

		private void txt_UploadYear_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !int.TryParse(e.Text, out _);
		}

		private void txt_UploadCaseNo_Changed(object sender, TextChangedEventArgs e)
		{
			BuildFileName();
		}
		private void txt_UploadCaseComplainant_TextChanged(object sender, TextChangedEventArgs e)
		{
			BuildFileName();
		}
		private void MultipleComplainantsCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			BuildFileName();
		}
		private void MultipleComplainantsCheckBox_UnChecked(object sender, RoutedEventArgs e)
		{
			BuildFileName();
		}
		private void Upload_AgentNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BuildFileName();
		}
		private void Upload_StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BuildFileName();
		}

		private void txt_UploadCaseSubject_Changed(object sender, TextChangedEventArgs e)
		{
			BuildFileName();
		}

		private void txt_UploadNature_Changed(object sender, TextChangedEventArgs e)
		{
			BuildFileName();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			ClearUploadFields();
			this.Close();
		}

		public void ClearUploadFields()
		{
			txt_UploadYear.Clear();
			txt_UploadCaseNo.Clear();
			txt_UploadCaseComplainant.Clear();
			txt_UploadFileName.Clear();
			txt_UploadFilePath.Clear();
			txt_UploadSubject.Clear();
			txt_UploadNature.Clear();

			Upload_CaseTypeComboBox.SelectedIndex = -1;
			Upload_AgentNameComboBox.SelectedIndex = -1;
			Upload_StatusComboBox.SelectedIndex = -1;
			MultipleComplainantsCheckBox.IsChecked = false;

			if (PdfViewer?.CoreWebView2 != null)
			{
				PdfViewer.CoreWebView2.Navigate("about:blank");
			}
		}


		private void AddAgent_Click(object sender, RoutedEventArgs e)
		{
			AddAgentPopup popup = new AddAgentPopup();
			popup.ShowDialog();
			LoadAgents();
		}

		private bool _isUpdatingText = false;

		private void txt_UploadNature_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (_isUpdatingText) return;

			var textBox = sender as TextBox;
			if (textBox == null) return;

			string upper = textBox.Text.ToUpper();

			if (textBox.Text != upper)
			{
				_isUpdatingText = true;

				int caretIndex = textBox.CaretIndex;
				textBox.Text = upper;
				textBox.CaretIndex = caretIndex;

				_isUpdatingText = false;
			}
		}
	}
}