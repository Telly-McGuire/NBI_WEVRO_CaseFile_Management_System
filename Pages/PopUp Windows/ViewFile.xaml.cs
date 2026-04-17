using CFMS_WPF.Data;
using CFMS_WPF.Pages.PopUp_Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X500;
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

		private void LoadPDF()
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
					string filePath = rdr["file_path"].ToString();

					// Ensure absolute path
					if (!System.IO.Path.IsPathRooted(filePath))
					{
						filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
					}

					// PDF preview in WebView2
					if (System.IO.File.Exists(filePath))
					{
						PdfViewer.Source = new Uri(filePath, UriKind.Absolute);
					}
					else
					{
						MessageBox.Show("File not found at: " + filePath,
										"Missing File",
										MessageBoxButton.OK,
										MessageBoxImage.Warning);
					}

					// Fill document metadata
					txt_ViewFileName.Text = rdr["file_name"].ToString();
					UsernameText.Text = rdr["uploaded_by"].ToString();
					UploadDateText.Text = Convert.ToDateTime(rdr["uploaded_at"]).ToString("yyyy-MM-dd");
				}

				rdr.Close();
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
				con.Close();
			}
		}


		private void Edit_Click(object sender, RoutedEventArgs e)
		{
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