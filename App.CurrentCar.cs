
using System.Windows;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const string NO_CAR_SCREEN_NAME = "No Car";
		private const string ALL_CARS_SAVE_NAME = "All";

		private string _car_currentCarScreenName = NO_CAR_SCREEN_NAME;
		private string _car_carSaveName = ALL_CARS_SAVE_NAME;

		private bool _car_carChanged = false;

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

			if ( _car_currentCarScreenName != carScreenName )
			{
				if ( carScreenName == NO_CAR_SCREEN_NAME )
				{
					WriteLine( "" );
					WriteLine( $"You are no longer driving a car." );
				}
				else
				{
					WriteLine( "" );
					WriteLine( $"You are driving a {carScreenName}." );

					Say( Settings.SayCarName, carScreenName );

					_car_carChanged = true;
				}

				_car_currentCarScreenName = carScreenName;

				UpdateCarSaveName();

				Dispatcher.BeginInvoke( () =>
				{
					var mainWindow = MarvinsAIRA.MainWindow.Instance;

					if ( mainWindow != null )
					{
						mainWindow.CurrentCarStatusBarItem.Content = _car_currentCarScreenName;
						mainWindow.CurrentCarStatusBarItem.Foreground = ( _car_currentCarScreenName == NO_CAR_SCREEN_NAME ) ? Brushes.Gray : Brushes.ForestGreen;
					}
				} );
			}

		}

		public void UpdateCarSaveName()
		{
			_car_carSaveName = Settings.SaveSettingsPerCar ? _car_currentCarScreenName : ALL_CARS_SAVE_NAME;
		}
	}
}
