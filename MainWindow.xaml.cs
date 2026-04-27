using System.Windows;
using System.Windows.Controls;
using YourNamespace.ViewModels;

namespace CFMS_WPF
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			//this is for the navigation, it will navigate to the dashboard page when the app is opened
			navframe.Navigate(new CFMS_WPF.Dashboard.Dashboard());

			//this is for the clock
			DataContext = new MainViewModel();
		}
		private void sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selected = sidebar.SelectedItem as NavButton;
			navframe.Navigate(selected.Navlink);
		}
		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var selected = sidebar.SelectedItem as NavButton;
			navframe.Navigate(selected.Navlink);
		}

		private void NavButton_Selected_1(object sender, RoutedEventArgs e)
		{
			string caseFileDirectory = @"C:\CaseFiles"; // your folder path
			if (System.IO.Directory.Exists(caseFileDirectory))
			{
				System.Diagnostics.Process.Start("explorer.exe", caseFileDirectory);
			}
			else
			{
				MessageBox.Show("Directory not found: " + caseFileDirectory,
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
		}


		//DO NOT TOUCH IDK WHY IT BUGS OUT WHEN ITS NOT EVEN NEEDED
		private void NavButton_Dashboard(object sender, RoutedEventArgs e)
		{

		}

		private void NavButton_Files(object sender, RoutedEventArgs e)
		{

		}

		private void NavButton_Explore(object sender, RoutedEventArgs e)
		{

		}

		private void NavButton_Agents(object sender, RoutedEventArgs e)
		{

		}

		private void NavButton_Profile(object sender, RoutedEventArgs e)
		{

		}
	}
}

