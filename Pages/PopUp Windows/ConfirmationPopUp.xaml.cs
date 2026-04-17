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

namespace CFMS_WPF
{
	/// <summary>
	/// Interaction logic for ConfirmationPopUp.xaml
	/// </summary>
	public partial class ConfirmationPopUp : Window
	{
		private CaseFile _caseFile;

		private MySqlConnection con;
		private MySqlCommand cmd;
		private MySqlDataReader rdr;
		public ConfirmationPopUp(CaseFile caseFile)
		{
			InitializeComponent();
			SetConnection();

			_caseFile = caseFile;

			txt_UploadFinalFileName.Text = caseFile.Document?.CaseFileName;
			txt_UploadFinalCaseType.Text = caseFile.caseType;
			txt_UploadFinalYear.Text = caseFile.caseYear;
			txt_UploadFinalCaseNo.Text = caseFile.caseNo;
			txt_UploadFinalCaseComplainant.Text = caseFile.caseComplainant;
			txt_UploadFinalAgentName.Text = caseFile.caseAgent;
			txt_UploadFinalStatus.Text = caseFile.caseStatus;
			txt_UploadFinalUsername.Text = caseFile.Document?.CaseUploadedBy;
			txt_UploadFinalDate.Text = caseFile.Document?.CaseUploadAt.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}
		private void Upload_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SetConnection();
				con.Open();

				// Check if file path already exists in case_documents
				string comparePath = _caseFile.Document.CaseFilePath; // already normalized with "/"
				using (var checkCmd = new MySqlCommand(
					@"SELECT COUNT(*) 
					  FROM case_documents 
					  WHERE file_path = @path", con))
				{
					checkCmd.Parameters.AddWithValue("@path", comparePath);

					int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

					if (existingCount > 0)
					{
						MessageBox.Show("This file has already been uploaded.\nDuplicate file paths are not allowed.",
										"Duplicate File",
										MessageBoxButton.OK,
										MessageBoxImage.Warning);
						return;
					}
				}


				// Map caseType string to ID
				int caseTypeId = 0;
				switch (_caseFile.caseType)
				{
					case "Service": caseTypeId = 1; break;
					case "Crime": caseTypeId = 2; break;
					case "Misc": caseTypeId = 3; break;
				}

				// Map status string to ID
				int statusId = 0;
				switch (_caseFile.caseStatus)
				{
					case "Closed": statusId = 1; break;
					case "Prosecuted": statusId = 2; break;
					case "Reported": statusId = 3; break;
				}

				// Lookup agent_id
				int agentId = 0;
				cmd = new MySqlCommand(@"
						SELECT agent_id FROM agent_info 
						WHERE last_name='" + _caseFile.caseAgent + "' LIMIT 1", con);
				var agentResult = cmd.ExecuteScalar();
				if (agentResult != null) agentId = Convert.ToInt32(agentResult);

				// Lookup uploaded_by user id
				int uploadedById = 0;
				cmd = new MySqlCommand(@"
						SELECT user_id FROM users 
						WHERE username='" + _caseFile.Document.CaseUploadedBy + "' LIMIT 1", con);
				var userResult = cmd.ExecuteScalar();
				if (userResult != null) uploadedById = Convert.ToInt32(userResult);

				// Insert into case_data
				cmd = new MySqlCommand(
					"INSERT INTO case_data (case_type, case_year, case_number, case_subject, case_nature, complainant, agent_id, status) " +
					"VALUES (" + caseTypeId + ", '" + _caseFile.caseYear + "', '" + _caseFile.caseNo + "', '" + _caseFile.caseSubject + "', '" + _caseFile.caseNature + "', '" + _caseFile.caseComplainant + "', " + agentId + ", " + statusId + "); " +
					"SELECT LAST_INSERT_ID();", con);

				int caseId = Convert.ToInt32(cmd.ExecuteScalar());

				// Insert into case_documents
				cmd = new MySqlCommand(
					"INSERT INTO case_documents (case_id, file_name, file_path, uploaded_by, uploaded_at) " +
					"VALUES (" + caseId + ", '" + _caseFile.Document.CaseFileName + "', '" + _caseFile.Document.CaseFilePath + "', " + uploadedById + ", '" + _caseFile.Document.CaseUploadAt.ToString("yyyy-MM-dd HH:mm:ss") + "');", con);

				cmd.ExecuteNonQuery();

				MessageBox.Show("Case and document uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error uploading case:\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				if (con != null && con.State == System.Data.ConnectionState.Open)
					con.Close();
			}

		}



		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}



	}
}
