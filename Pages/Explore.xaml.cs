using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CFMS_WPF
{
	public partial class Explore : Page
	{
		public Explore()
		{
			InitializeComponent();
			OpenCaseFileDirectory();
		}

		private void OpenCaseFileDirectory()
		{
			string caseFileDirectory = @"C:\WEVRO CASE FILES 2005-2022"; // adjust path

			if (Directory.Exists(caseFileDirectory))
			{
				Process.Start("explorer.exe", caseFileDirectory);
			}
			else
			{
				System.Windows.MessageBox.Show("Directory not found: " + caseFileDirectory,
											   "Error",
											   System.Windows.MessageBoxButton.OK,
											   System.Windows.MessageBoxImage.Error);
			}
		}
	}
}
