using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CFMS_WPF
{



	public partial class FilterPopup : Window
	{

		private MySqlConnection con;
		private MySqlCommand cmd;


		public FilterPopup()
		{
			InitializeComponent();
			SetConnection();
			LoadAgents();
			LoadFiles();

		}

		private void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}

		//AGENTS SECTION CODE :3
		private void LoadAgents()
		{
			try
			{

				con.Open();
				cmd = new MySqlCommand("SELECT agent_id, last_name, first_name, middle_name FROM agent_info", con);
				var reader = cmd.ExecuteReader();

				var list = new List<AgentData>();

				while (reader.Read())
				{
					list.Add(new AgentData
					{
						agentID = Convert.ToInt32(reader["agent_id"]),
						agentlastName = reader["last_name"].ToString()
					});
				}
				reader.Close();

				AgentNameComboBox.ItemsSource = list;
				AgentNameComboBox.DisplayMemberPath = "agentlastName";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading agents:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}

		//YEAR AND CASE TYPE SECTION CODE :3
		private void LoadFiles()
		{
			try
			{
				con.Open();
				cmd = new MySqlCommand(@"
							SELECT cd.case_year,
								   ct.type_name AS case_type,
								   cd.case_number
							FROM case_data cd
							JOIN case_type ct ON cd.case_type = ct.type_id", con);
				var rdr = cmd.ExecuteReader();

				var list = new List<CaseFile>();

				while (rdr.Read())
				{
					list.Add(new CaseFile
					{
						caseType = rdr["case_type"].ToString(),
						caseYear = rdr["case_year"].ToString(),
						caseNo = rdr["case_number"].ToString(),
					});
				}
				rdr.Close();

				var distinctYears = list
					.Select(cf => cf.caseYear)
					.Distinct()
					.OrderBy(y => y)
					.ToList();

				YearComboBox.ItemsSource = distinctYears;

				var distinctTypes = list
					.Select(cf => cf.caseType)
					.Distinct()
					.OrderBy(t => t)
					.ToList();

				CaseTypeComboBox.ItemsSource = distinctTypes;

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

		//Agent 
		private void AgentNameComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
		{
			if (AgentNameComboBox.SelectedItem is AgentData selectedAgent)
			{
				currentFilter.filter_agent_name = selectedAgent.agentlastName;
			}
		}

		//Year
		private void YearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (YearComboBox.SelectedItem is CaseFile selectedYear)
			{
				if (int.TryParse(selectedYear.caseYear, out int year))
					currentFilter.filter_year = year;
				else
					currentFilter.filter_year = 0;
			}
		}

		//CaseType
		private void CaseTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (CaseTypeComboBox.SelectedItem is CaseFile selectedCaseType)
			{
				currentFilter.filter_casetype = selectedCaseType.caseType;
			}
		}


		//CAE NUMBER SECTION CODE :3
		private void txt_FilterCaseNo_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox txtBox = sender as TextBox;
			if (txtBox != null)
			{
				if (int.TryParse(txtBox.Text, out int caseNo))
					currentFilter.filter_caseno = caseNo;
			}
		}

		private FilterData currentFilter = new FilterData();
		public FilterData SelectedFilter { get; private set; }

		public class FilterData
		{
			public int filter_year { get; set; }
			public string filter_casetype { get; set; }
			public int filter_caseno { get; set; }
			public string filter_agent_name { get; set; }
		}

		private void Apply_Click(object sender, RoutedEventArgs e)

		{
			SelectedFilter = new FilterData
			{
				filter_year = currentFilter.filter_year,
				filter_casetype = currentFilter.filter_casetype,
				filter_caseno = currentFilter.filter_caseno,
				filter_agent_name = currentFilter.filter_agent_name
			};

			// Close the popup
			this.DialogResult = true;
			this.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{

		}




	}
}
