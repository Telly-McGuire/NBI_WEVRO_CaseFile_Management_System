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

		public string GetServerIP()
		{
			var conn = new MySqlConnectionStringBuilder(connectionString);
			return conn.Server; // returns just the IP, e.g. "192.168.254.205"
		}
	}
}
