using System;
using System.ComponentModel;

namespace YourNamespace.ViewModels
{
	//THIS IS FOR THE BUILT IN CLOCK
	public class MainViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public MainViewModel()
		{
			RefreshDate();
		}

		private DateTime _nowPlusTen;
		public DateTime NowPlusTen
		{
			get => _nowPlusTen;
			set
			{
				_nowPlusTen = value;
				OnPropertyChanged(nameof(NowPlusTen));
			}
		}

		public void RefreshDate()
		{
			NowPlusTen = DateTime.Now;
		}

		private void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}