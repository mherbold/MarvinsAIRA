
using System.Windows;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const string NO_CAR_SCREEN_NAME = "No Car";
		private const string ALL_CARS_SAVE_NAME = "All";

		private string _currentCarScreenName = NO_CAR_SCREEN_NAME;
		private string _carSaveName = ALL_CARS_SAVE_NAME;

		private bool _carChanged = false;

		private void InitializeCurrentCar()
		{
			WriteLine( "" );
			WriteLine( "InitializeCurrentCar called." );

			UpdateCurrentCar();
		}

		private void UpdateCurrentCar()
		{
			var carScreenName = NO_CAR_SCREEN_NAME;

			if ( _irsdk.IsConnected && ( _irsdk.Data.SessionInfo != null ) )
			{
				foreach ( var driver in _irsdk.Data.SessionInfo.DriverInfo.Drivers )
				{
					if ( driver.CarIdx == _irsdk.Data.SessionInfo.DriverInfo.DriverCarIdx )
					{
						carScreenName = driver.CarScreenName;
						break;
					}
				}
			}

			if ( _currentCarScreenName != carScreenName )
			{
				if ( carScreenName == NO_CAR_SCREEN_NAME )
				{
					WriteLine( "" );
					WriteLine( $"You are no longer driving a car." );
				}
				else
				{
					WriteLine( "" );
					WriteLine( $"You are now driving a {carScreenName}." );

					Say( $"You are now driving a {carScreenName}." );

					_carChanged = true;
				}

				_currentCarScreenName = carScreenName;
				_carSaveName = Settings.SaveSettingsPerCar ? _currentCarScreenName : ALL_CARS_SAVE_NAME;

				Dispatcher.BeginInvoke( () =>
				{
					var mainWindow = (MainWindow) MainWindow;

					if ( mainWindow != null )
					{
						mainWindow.CurrentCarStatusBarItem.Content = _currentCarScreenName;
						mainWindow.CurrentCarStatusBarItem.Foreground = ( _currentCarScreenName == NO_CAR_SCREEN_NAME ) ? Brushes.Gray : Brushes.ForestGreen;
					}
				} );
			}

		}
	}
}
