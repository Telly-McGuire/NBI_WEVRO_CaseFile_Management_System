using CFMS_WPF.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
	public partial class AddAgentPopup : Window
	{
		private MySqlConnection con;
		private MySqlCommand cmd;

		public AddAgentPopup()
		{
			InitializeComponent();
			SetConnection();
			LoadAgents();

		}

		// ================= DATABASE CONNECTION =================
		private void SetConnection()
		{
			SetConnection db = new SetConnection();
			con = db.GetConnection();
		}

		// ================= LOAD AGENTS INTO DATAGRID =================
		private void LoadAgents()
		{
			try
			{
				data_AgentInfo.ItemsSource = null;

				con.Open();
				cmd = new MySqlCommand("SELECT agent_id, last_name, first_name, middle_name FROM agent_info", con);
				var reader = cmd.ExecuteReader();

				var list = new List<AgentData>();

				while (reader.Read())
				{
					list.Add(new AgentData
					{
						agentID = Convert.ToInt32(reader["agent_id"]),
						agentlastName = reader["last_name"].ToString(),
						agentfirstName = reader["first_name"].ToString(),
						agentMiddleInitial = reader["middle_name"].ToString()
					});
				}
				reader.Close();
				data_AgentInfo.ItemsSource = list;
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

		// ================= ADD AGENT BUTTON =================
		private void AddAgent_Click(object sender, RoutedEventArgs e)
		{
			string last = txt_AgentLastName.Text.Trim();
			string first = txt_AgentFirstName.Text.Trim();
			string middle = txt_AgentMI.Text.Trim();

			// 🔹 VALIDATION
			if (string.IsNullOrWhiteSpace(last) ||
				string.IsNullOrWhiteSpace(first))
			{
				MessageBox.Show("Please fill in required fields.",
								"Validation Error",
								MessageBoxButton.OK,
								MessageBoxImage.Warning);
				return;
			}

			// 🔹 CONFIRMATION
			var confirm = MessageBox.Show(
				"This agent will be permanently added to the system. Continue?",
				"Confirm Add",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (confirm != MessageBoxResult.Yes)
				return;

			try
			{
				con.Open();

				cmd = new MySqlCommand(
					"INSERT INTO agent_info (last_name, first_name, middle_name) VALUES ('" + txt_AgentLastName.Text + "','" + txt_AgentFirstName.Text + "','" + txt_AgentMI.Text + "');", con);
				cmd.ExecuteNonQuery();

				MessageBox.Show("Agent added successfully!",
								"Success",
								MessageBoxButton.OK,
								MessageBoxImage.Information);
				ClearFields();
				LoadAgents();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error adding agent:\n" + ex.Message,
								"Database Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
			finally
			{
				con.Close();
			}
		}
		//Select Agent
		private AgentData SelectedAgent;
		private void data_AgentInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (data_AgentInfo.SelectedItem is AgentData agent)
			{
				this.SelectedAgent = agent;

				txt_AgentId.Text = agent.agentID.ToString();
				txt_AgentFirstName.Text = agent.agentfirstName;
				txt_AgentLastName.Text = agent.agentlastName;
				txt_AgentMI.Text = agent.agentMiddleInitial;
			}
		}



		// ================= CLEAR INPUTS =================


		private void btn_SaveAgent_Click(object sender, RoutedEventArgs e)
		{
			if (data_AgentInfo.SelectedItems.Count > 0)
			{
				con.Open();
				cmd = new MySqlCommand(
						   $@"UPDATE agent_info SET " +
						   "first_name = '" + txt_AgentFirstName.Text + "', " +
						   "last_name = '" + txt_AgentLastName.Text + "', " +
						   "middle_name = '" + txt_AgentMI.Text + "' " +
						   "WHERE agent_id = " + SelectedAgent.agentID, con);
				cmd.ExecuteNonQuery();
				con.Close();
				MessageBox.Show("Your Agents Are Updated Succsessfully!", "Yaay!");
				ClearFields();
				LoadAgents();
			}
		}

		private void LastNameBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void txt_AgentFirstName_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void MiddleInitialBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
		private void ClearFields()
		{
			txt_AgentFirstName.Clear();
			txt_AgentLastName.Clear();
			txt_AgentMI.Clear();
		}

		private void btn_ClearAgent_Click(object sender, RoutedEventArgs e)
		{
			ClearFields();
		}
	}
}