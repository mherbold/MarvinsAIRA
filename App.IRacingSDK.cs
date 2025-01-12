
using System.Windows;
using System.Windows.Media;

using IRSDKSharper;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const int IRSDK_360HZ_SAMPLES_PER_FRAME = 6;

		private IRacingSdk _irsdk = new();

		private IRacingSdkDatum? _steeringWheelTorque_STDatum = null;
		private IRacingSdkDatum? _isOnTrackDatum = null;
		private IRacingSdkDatum? _speedDatum = null;
		private IRacingSdkDatum? _velocityXDatum = null;
		private IRacingSdkDatum? _velocityYDatum = null;
		private IRacingSdkDatum? _displayUnitsDatum = null;

		private bool _telemetryDataInitialized = false;

		private float[] _steeringWheelTorque_ST = new float[ IRSDK_360HZ_SAMPLES_PER_FRAME ];
		private bool _isOnTrack = false;
		private float _speed = 0;
		private float _velocityX = 0;
		private float _velocityY = 0;
		private int _displayUnits = 0;

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

			Say( "We are now connected to the iRacing simulator." );

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = (MainWindow) MainWindow;

				if ( mainWindow != null )
				{
					mainWindow.ConnectionStatusBarItem.Content = "Connected";
					mainWindow.ConnectionStatusBarItem.Foreground = Brushes.ForestGreen;
				}
			} );
		}

		private void OnDisconnected()
		{
			WriteLine( "" );
			WriteLine( "OnDisconnected called." );

			Say( "We have been disconnected from the iRacing simulator." );

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

			_irsdk.Data.GetFloatArray( _steeringWheelTorque_STDatum, _steeringWheelTorque_ST, 0, _steeringWheelTorque_ST.Length );

			_isOnTrack = _irsdk.Data.GetBool( _isOnTrackDatum );
			_speed = _irsdk.Data.GetFloat( _speedDatum );
			_velocityX = _irsdk.Data.GetFloat( _velocityXDatum );
			_velocityY = _irsdk.Data.GetFloat( _velocityYDatum );
			_displayUnits = _irsdk.Data.GetInt( _displayUnitsDatum );

			UpdateForceFeedback();
		}

		private void OnDebugLog( string message )
		{
			WriteLine( "" );
			WriteLine( message );
		}
	}
}
