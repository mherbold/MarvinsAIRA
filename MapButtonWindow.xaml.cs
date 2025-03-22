
using System.Windows;

namespace MarvinsAIRA
{
	public partial class MapButtonWindow : Window
	{
		private readonly System.Timers.Timer _timer = new( 100 );

		private readonly Settings.MappedButtons currentMappedButtons;
		private readonly Settings.MappedButtons newMappedButtons = new();

		public bool canceled = true;

		public MapButtonWindow( Settings.MappedButtons mappedButtons )
		{
			InitializeComponent();

			currentMappedButtons = mappedButtons;

			newMappedButtons.Button1.DeviceInstanceGuid = currentMappedButtons.Button1.DeviceInstanceGuid;
			newMappedButtons.Button1.DeviceProductName = currentMappedButtons.Button1.DeviceProductName;
			newMappedButtons.Button1.ButtonNumber = currentMappedButtons.Button1.ButtonNumber;

			newMappedButtons.Button2.DeviceInstanceGuid = currentMappedButtons.Button2.DeviceInstanceGuid;
			newMappedButtons.Button2.DeviceProductName = currentMappedButtons.Button2.DeviceProductName;
			newMappedButtons.Button2.ButtonNumber = currentMappedButtons.Button2.ButtonNumber;
		}

		private void UpdateButtonInformation()
		{
			if ( newMappedButtons.Button1.DeviceInstanceGuid != Guid.Empty )
			{
				Button1NameLabel.Content = $"Button {newMappedButtons.Button1.ButtonNumber + 1} on {newMappedButtons.Button1.DeviceProductName}";
			}
			else
			{
				Button1NameLabel.Content = "Not set.";
			}

			if ( newMappedButtons.Button2.DeviceInstanceGuid != Guid.Empty )
			{
				Button1NameLabel.Content += " (HOLD)";

				Button2NameLabel.Content = $"Button {newMappedButtons.Button2.ButtonNumber + 1} on {newMappedButtons.Button2.DeviceProductName} (CLICK)";

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

			if ( !canceled )
			{
				currentMappedButtons.Button1 = newMappedButtons.Button1;
				currentMappedButtons.Button2 = newMappedButtons.Button2;
			}
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}

		private void ClearButton_Click( object sender, RoutedEventArgs e )
		{
			newMappedButtons.Button1 = new();
			newMappedButtons.Button2 = new();

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

			app.UpdateInputs( 1f / 60f );

			if ( app.Input_AnyPressedButton.DeviceInstanceGuid != Guid.Empty )
			{
				var buttonAlreadyCaptured = false;

				if ( app.Input_AnyPressedButton.DeviceInstanceGuid == newMappedButtons.Button1.DeviceInstanceGuid )
				{
					if ( app.Input_AnyPressedButton.ButtonNumber == newMappedButtons.Button1.ButtonNumber )
					{
						buttonAlreadyCaptured = true;
					}
				}

				if ( app.Input_AnyPressedButton.DeviceInstanceGuid == newMappedButtons.Button2.DeviceInstanceGuid )
				{
					if ( app.Input_AnyPressedButton.ButtonNumber == newMappedButtons.Button2.ButtonNumber )
					{
						buttonAlreadyCaptured = true;
					}
				}

				if ( !buttonAlreadyCaptured )
				{
					var mappedButton = new Settings.MappedButton
					{
						DeviceInstanceGuid = app.Input_AnyPressedButton.DeviceInstanceGuid,
						DeviceProductName = app.Input_AnyPressedButton.DeviceProductName,
						ButtonNumber = app.Input_AnyPressedButton.ButtonNumber
					};

					if ( newMappedButtons.Button1.DeviceInstanceGuid == Guid.Empty )
					{
						newMappedButtons.Button1 = mappedButton;
					}
					else if ( newMappedButtons.Button2.DeviceInstanceGuid == Guid.Empty )
					{
						newMappedButtons.Button2 = mappedButton;
					}
					else
					{
						newMappedButtons.Button1 = newMappedButtons.Button2;
						newMappedButtons.Button2 = mappedButton;
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
