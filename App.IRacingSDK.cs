
using System.Windows;
using System.Windows.Media;

using IRSDKSharper;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private IRacingSdk _irsdk = new();

		private IRacingSdkDatum? _steeringWheelTorque_STDatum = null;
		private IRacingSdkDatum? _isOnTrackDatum = null;
		private IRacingSdkDatum? _speedDatum = null;
		private IRacingSdkDatum? _velocityXDatum = null;
		private IRacingSdkDatum? _velocityYDatum = null;
		private IRacingSdkDatum? _displayUnitsDatum = null;

		private bool _telemetryDataInitialized = false;

		private void InitializeIRacingSDK()
		{
			WriteLine( "" );
			WriteLine( "InitializeIRacingSDK called." );
			WriteLine( "Initializing the iRacing SDK..." );

			_irsdk.OnException += OnException;
			_irsdk.OnConnected += OnConnected;
			_irsdk.OnDisconnected += OnDisconnected;
			_irsdk.OnSessionInfo += OnSessionInfo;
			_irsdk.OnTelemetryData += OnTelemetryData;
			_irsdk.OnDebugLog += OnDebugLog;

			_irsdk.Start();

			WriteLine( "...the iRacing SDK is now waiting for the iRacing Simulator." );
		}

		public void StopIRacingSDK()
		{
			WriteLine( "" );
			WriteLine( "StopIRacingSDK called." );

			WriteLine( "Stopping the iRacing SDK..." );

			_irsdk.Stop();

			while ( _irsdk.IsStarted )
			{
				Thread.Sleep( 0 );
			}

			WriteLine( "...the iRacing SDK has been stopped." );
		}

		private void OnException( Exception exception )
		{
			WriteLine( "" );
			WriteLine( $"OnException called." );

			WriteLine( "The following exception was thrown from inside the iRacing SDK:" );
			WriteLine( exception.Message.Trim() );
		}

		private void OnConnected()
		{
			WriteLine( "" );
			WriteLine( "OnConnected called." );

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = (MainWindow) MainWindow;

				if ( mainWindow != null )
				{
					mainWindow.ConnectionStatusBarItem.Content = "Connected";
					mainWindow.ConnectionStatusBarItem.Foreground = Brushes.White;
				}
			} );
		}

		private void OnDisconnected()
		{
			WriteLine( "" );
			WriteLine( "OnDisconnected called." );

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = (MainWindow) MainWindow;

				if ( mainWindow != null )
				{
					mainWindow.ConnectionStatusBarItem.Content = "Disconnected";
					mainWindow.ConnectionStatusBarItem.Foreground = Brushes.Red;

					UpdateCurrentCar();
					UpdateCurrentTrack();
				}
			} );
		}

		private void OnSessionInfo()
		{
			UpdateCurrentCar();
			UpdateCurrentTrack();
		}

		private void OnTelemetryData()
		{
			if ( !_telemetryDataInitialized )
			{
				_steeringWheelTorque_STDatum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelTorque_ST" ];
				_isOnTrackDatum = _irsdk.Data.TelemetryDataProperties[ "IsOnTrack" ];
				_speedDatum = _irsdk.Data.TelemetryDataProperties[ "Speed" ];
				_velocityXDatum = _irsdk.Data.TelemetryDataProperties[ "VelocityX" ];
				_velocityYDatum = _irsdk.Data.TelemetryDataProperties[ "VelocityY" ];
				_displayUnitsDatum = _irsdk.Data.TelemetryDataProperties[ "DisplayUnits" ];

				_telemetryDataInitialized = true;
			}

			UpdateForceFeedback();
		}

		private void OnDebugLog( string message )
		{
			WriteLine( "" );
			WriteLine( message );
		}
	}
}