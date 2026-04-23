using System.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CFMS_WPF.Data
{
	internal class SetConnection
	{
		private string connectionString;
		public SetConnection()
		{
			connectionString = ConfigurationManager.ConnectionStrings["case_file_management_system"].ConnectionString;
		}

		public MySqlConnection GetConnection()
		{
			return new MySqlConnection(connectionString);
		}
	}
}
