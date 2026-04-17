using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMS_WPF.Data
{
	internal class CurrentUser
	{
		public static string CurrentFullName { get; set; }
		public static string CurrentUsername { get; set; }
		public static string CurrentPassword { get; set; }
		public static string CurrentRole { get; set; }
	}
}
