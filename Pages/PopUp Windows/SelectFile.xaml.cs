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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CFMS_WPF
{
	/// <summary>
	/// Interaction logic for AddFile.xaml
	/// </summary>
	public partial class AddFile : Window
	{
		private string selectedFilePath;

		public AddFile()
		{
			InitializeComponent();
		}

		private void SelectFile_Click(object sender, RoutedEventArgs e)
		{
			string caseFilesPath = @"C:\Users\IFDBS119\Desktop\FINAL SCANNED FILES 2005-2022";

			if (!Directory.Exists(caseFilesPath))
			{
				MessageBox.Show("Case files directory not found.");
				return;
			}

			Process.Start(new ProcessStartInfo()
			{
				FileName = caseFilesPath,
				UseShellExecute = true
			});

		}
		private void DropZone_DragEnter(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.None;
		}
		private void DropZone_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (files.Length > 0)
				{
					selectedFilePath = files[0];
					UploadButton.IsEnabled = true;
				}
			}
		}
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void Upload_Click(object sender, RoutedEventArgs e)
		{
		}
	}
}
