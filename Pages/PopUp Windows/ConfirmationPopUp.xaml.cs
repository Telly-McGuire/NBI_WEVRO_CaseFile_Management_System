using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

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


                // Map caseType string to ID (ensure IDs match case_type table)
				int caseTypeId = 0;
				switch ((_caseFile.caseType ?? string.Empty).Trim())
				{
					// Crime should be type_id = 1, Service = 2, Misc = 3
					case "Crime": caseTypeId = 1; break;
					case "Service": caseTypeId = 2; break;
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

                // Insert into case_data using parameters to avoid SQL injection/escaping issues
				cmd = new MySqlCommand(
					"INSERT INTO case_data (case_type, case_year, case_number, case_subject, case_nature, complainant, agent_id, status) " +
					"VALUES (@caseType, @caseYear, @caseNumber, @caseSubject, @caseNature, @complainant, @agentId, @status); SELECT LAST_INSERT_ID();",
					con);

				cmd.Parameters.AddWithValue("@caseType", caseTypeId);
				cmd.Parameters.AddWithValue("@caseYear", _caseFile.caseYear ?? string.Empty);
				cmd.Parameters.AddWithValue("@caseNumber", _caseFile.caseNo ?? string.Empty);
				cmd.Parameters.AddWithValue("@caseSubject", _caseFile.caseSubject ?? string.Empty);
				cmd.Parameters.AddWithValue("@caseNature", _caseFile.caseNature ?? string.Empty);
				cmd.Parameters.AddWithValue("@complainant", _caseFile.caseComplainant ?? string.Empty);
				cmd.Parameters.AddWithValue("@agentId", agentId);
				cmd.Parameters.AddWithValue("@status", statusId);

				int caseId = Convert.ToInt32(cmd.ExecuteScalar());

				// Insert into case_documents using parameters
				cmd = new MySqlCommand(
					"INSERT INTO case_documents (case_id, file_name, file_path, uploaded_by, uploaded_at) " +
					"VALUES (@caseId, @fileName, @filePath, @uploadedBy, @uploadedAt);",
					con);

				cmd.Parameters.AddWithValue("@caseId", caseId);
				cmd.Parameters.AddWithValue("@fileName", _caseFile.Document?.CaseFileName ?? string.Empty);
				cmd.Parameters.AddWithValue("@filePath", _caseFile.Document?.CaseFilePath ?? string.Empty);
				cmd.Parameters.AddWithValue("@uploadedBy", uploadedById);
				cmd.Parameters.AddWithValue("@uploadedAt", _caseFile.Document?.CaseUploadAt.ToString("yyyy-MM-dd HH:mm:ss"));

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
