
using System.Windows;
using System.Windows.Media;

using IRSDKSharper;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const int IRSDK_TICK_RATE = 60;
		private const int IRSDK_360HZ_SAMPLES_PER_FRAME = 6;

		private IRacingSdk _irsdk = new();

		private IRacingSdkDatum? _irsdk_steeringWheelTorque_STDatum = null;
		private IRacingSdkDatum? _irsdk_steeringWheelTorque_Datum = null;
		private IRacingSdkDatum? _irsdk_isOnTrackDatum = null;
		private IRacingSdkDatum? _irsdk_speedDatum = null;
		private IRacingSdkDatum? _irsdk_velocityXDatum = null;
		private IRacingSdkDatum? _irsdk_velocityYDatum = null;
		private IRacingSdkDatum? _irsdk_yawRateDatum = null;
		private IRacingSdkDatum? _irsdk_latAccelDatum = null;
		private IRacingSdkDatum? _irsdk_steeringWheelAngleDatum = null;
		private IRacingSdkDatum? _irsdk_displayUnitsDatum = null;

		private bool _irsdk_telemetryDataInitialized = false;

		public bool _irsdk_connected = false;

		public int _irsdk_tickRate = 0;
		public int _irsdk_tickCount = 0;

		public float[] _irsdk_steeringWheelTorque_ST = new float[ IRSDK_360HZ_SAMPLES_PER_FRAME ];
		public float _irsdk_steeringWheelTorque;
		public bool _irsdk_isOnTrack = false;
		public float _irsdk_speed = 0;
		public float _irsdk_velocityX = 0;
		public float _irsdk_velocityY = 0;
		public float _irsdk_yawRate = 0;
		public float _irsdk_latAccel = 0;
		public float _irsdk_steeringWheelAngle = 0;
		public int _irsdk_displayUnits = 0;

		private int _irsdk_lastTickCount = 0;

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

			Say( Settings.SayConnected );

			_irsdk_connected = true;

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = MarvinsAIRA.MainWindow.Instance;

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

			_irsdk_connected = false;

			_irsdk_tickRate = 0;
			_irsdk_tickCount = 0;

			_irsdk_steeringWheelTorque_ST[ 0 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 1 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 2 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 3 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 4 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 5 ] = 0;

			_irsdk_steeringWheelTorque = 0;
			_irsdk_isOnTrack = false;
			_irsdk_speed = 0;
			_irsdk_velocityX = 0;
			_irsdk_velocityY = 0;
			_irsdk_displayUnits = 0;

			_irsdk_lastTickCount = 0;

			_irsdk.PauseSessionInfoUpdates = false;

			UpdateConstantForce( [ 0, 0, 0, 0, 0, 0 ] );

			Say( Settings.SayDisconnected );

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = MarvinsAIRA.MainWindow.Instance;

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
			if ( !_irsdk_telemetryDataInitialized )
			{
				_irsdk_steeringWheelTorque_STDatum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelTorque_ST" ];
				_irsdk_steeringWheelTorque_Datum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelTorque" ];
				_irsdk_isOnTrackDatum = _irsdk.Data.TelemetryDataProperties[ "IsOnTrack" ];
				_irsdk_speedDatum = _irsdk.Data.TelemetryDataProperties[ "Speed" ];
				_irsdk_velocityXDatum = _irsdk.Data.TelemetryDataProperties[ "VelocityX" ];
				_irsdk_velocityYDatum = _irsdk.Data.TelemetryDataProperties[ "VelocityY" ];
				_irsdk_yawRateDatum = _irsdk.Data.TelemetryDataProperties[ "YawRate" ];
				_irsdk_latAccelDatum = _irsdk.Data.TelemetryDataProperties[ "LatAccel" ];
				_irsdk_steeringWheelAngleDatum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelAngle" ];
				_irsdk_displayUnitsDatum = _irsdk.Data.TelemetryDataProperties[ "DisplayUnits" ];

				_irsdk_telemetryDataInitialized = true;
			}

			_irsdk_tickRate = _irsdk.Data.TickRate;
			_irsdk_tickCount = _irsdk.Data.TickCount;

			_irsdk.Data.GetFloatArray( _irsdk_steeringWheelTorque_STDatum, _irsdk_steeringWheelTorque_ST, 0, _irsdk_steeringWheelTorque_ST.Length );

			_irsdk_steeringWheelTorque = _irsdk.Data.GetFloat( _irsdk_steeringWheelTorque_Datum );
			_irsdk_isOnTrack = _irsdk.Data.GetBool( _irsdk_isOnTrackDatum );
			_irsdk_speed = _irsdk.Data.GetFloat( _irsdk_speedDatum );
			_irsdk_velocityX = _irsdk.Data.GetFloat( _irsdk_velocityXDatum );
			_irsdk_velocityY = _irsdk.Data.GetFloat( _irsdk_velocityYDatum );
			_irsdk_yawRate = _irsdk.Data.GetFloat( _irsdk_yawRateDatum );
			_irsdk_latAccel = _irsdk.Data.GetFloat( _irsdk_latAccelDatum );
			_irsdk_steeringWheelAngle = _irsdk.Data.GetFloat( _irsdk_steeringWheelAngleDatum );
			_irsdk_displayUnits = _irsdk.Data.GetInt( _irsdk_displayUnitsDatum );

			_irsdk.PauseSessionInfoUpdates = _irsdk_isOnTrack;

			UpdateForceFeedback();

			// run main window update loop at 20 FPS

			if ( _irsdk_tickCount - _irsdk_lastTickCount >= 3 )
			{
				_irsdk_lastTickCount = _irsdk_tickCount;

				var mainWindow = MarvinsAIRA.MainWindow.Instance;

				if ( mainWindow != null )
				{
					mainWindow._win_autoResetEvent.Set();
				}
			}
		}

		private void OnDebugLog( string message )
		{
			WriteLine( $"[IRSDK] {message}" );
		}
	}
}
