
using System.Windows;

namespace MarvinsAIRA
{
	public partial class MapButtonWindow : Window
	{
		private readonly System.Timers.Timer _timer = new( 100 );

		public required Settings.MappedButtons MappedButtons { get; set; }

		public bool canceled = true;

		public MapButtonWindow()
		{
			InitializeComponent();
		}

		private void UpdateButtonInformation()
		{
			if ( MappedButtons.Button1.DeviceInstanceGuid != Guid.Empty )
			{
				Button1NameLabel.Content = $"Button {MappedButtons.Button1.ButtonNumber + 1} on {MappedButtons.Button1.DeviceProductName}";
			}
			else
			{
				Button1NameLabel.Content = "Not set.";
			}

			if ( MappedButtons.Button2.DeviceInstanceGuid != Guid.Empty )
			{
				Button1NameLabel.Content += " (HOLD)";

				Button2NameLabel.Content = $"Button {MappedButtons.Button2.ButtonNumber + 1} on {MappedButtons.Button2.DeviceProductName} (CLICK)";

				PlusLabel.Visibility = Visibility.Visible;
				Button2NameLabel.Visibility = Visibility.Visible;
			}
			else
			{
				PlusLabel.Visibility = Visibility.Collapsed;
				Button2NameLabel.Visibility = Visibility.Collapsed;
			}

		}

		private void Window_Activated( object sender, EventArgs e )
		{
			UpdateButtonInformation();

			_timer.Elapsed += OnTimer;
			_timer.Start();
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			_timer.Stop();
			_timer.Dispose();
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}

		private void ClearButton_Click( object sender, RoutedEventArgs e )
		{
			MappedButtons.Button1 = new();
			MappedButtons.Button2 = new();

			UpdateButtonInformation();
		}

		private void UpdateButton_Click( object sender, RoutedEventArgs e )
		{
			canceled = false;

			Close();
		}

		private void OnTimer( object? sender, EventArgs e )
		{
			var app = (App) Application.Current;

			app.UpdateInputs();

			if ( app.AnyPressedButton.DeviceInstanceGuid != Guid.Empty )
			{
				var buttonAlreadyCaptured = false;

				if ( app.AnyPressedButton.DeviceInstanceGuid == MappedButtons.Button1.DeviceInstanceGuid )
				{
					if ( app.AnyPressedButton.ButtonNumber == MappedButtons.Button1.ButtonNumber )
					{
						buttonAlreadyCaptured = true;
					}
				}

				if ( app.AnyPressedButton.DeviceInstanceGuid == MappedButtons.Button2.DeviceInstanceGuid )
				{
					if ( app.AnyPressedButton.ButtonNumber == MappedButtons.Button2.ButtonNumber )
					{
						buttonAlreadyCaptured = true;
					}
				}

				if ( !buttonAlreadyCaptured )
				{
					var mappedButton = new Settings.MappedButton
					{
						DeviceInstanceGuid = app.AnyPressedButton.DeviceInstanceGuid,
						DeviceProductName = app.AnyPressedButton.DeviceProductName,
						ButtonNumber = app.AnyPressedButton.ButtonNumber
					};

					if ( MappedButtons.Button1.DeviceInstanceGuid == Guid.Empty )
					{
						MappedButtons.Button1 = mappedButton;
					}
					else if ( MappedButtons.Button2.DeviceInstanceGuid == Guid.Empty )
					{
						MappedButtons.Button2 = mappedButton;
					}
					else
					{
						MappedButtons.Button1 = MappedButtons.Button2;
						MappedButtons.Button2 = mappedButton;
					}
				}

				Dispatcher.BeginInvoke( () =>
				{
					UpdateButtonInformation();
				} );
			}
		}
	}
}
