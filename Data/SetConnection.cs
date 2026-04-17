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
			connectionString = @"
                server=192.168.254.205;
                port=3307;
                database=case_file_management_system;
                uid=cfms;
                pwd=wevrocfms2026;";
		}
		public MySqlConnection GetConnection()
		{
			return new MySqlConnection(connectionString);
		}
	}
}
