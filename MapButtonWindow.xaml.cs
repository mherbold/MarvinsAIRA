
using System.Windows;

namespace MarvinsAIRA
{
	public partial class MapButtonWindow : Window
	{
		private readonly System.Timers.Timer _timer = new( 200 );

		public Guid deviceInstanceGuid = Guid.Empty;
		public string deviceProductName = string.Empty;
		public int buttonNumber = 0;

		public bool canceled = true;

		public MapButtonWindow()
		{
			InitializeComponent();
		}

		private void Window_Activated( object sender, EventArgs e )
		{
			if ( deviceInstanceGuid != Guid.Empty )
			{
				ButtonNameLabel.Content = $"Currently set to button {buttonNumber + 1} on {deviceProductName}.";
			}
			else
			{
				ButtonNameLabel.Content = "Currently not set.";
			}

			_timer.Elapsed += OnTimer;
			_timer.Start();
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			_timer.Stop();
			_timer.Dispose();
		}

		private void UpdateButton_Click( object sender, RoutedEventArgs e )
		{
			canceled = false;

			Close();
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}

		private void OnTimer( object? sender, EventArgs e )
		{
			var app = (App) Application.Current;

			var pressedButton = app.GetAnyPressedButton();

			if ( pressedButton != null )
			{
				Dispatcher.BeginInvoke( () =>
				{
					deviceInstanceGuid = pressedButton.deviceInstanceGuid;
					deviceProductName = pressedButton.deviceProductName;
					buttonNumber = pressedButton.buttonNumber;

					ButtonNameLabel.Content = $"Change to button {buttonNumber + 1} on {deviceProductName}.";
				} );
			}
		}
	}
}
