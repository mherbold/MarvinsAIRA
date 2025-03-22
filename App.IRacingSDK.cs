
using System.Windows;
using System.Windows.Media;

using IRSDKSharper;

#if DEBUG

using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endif

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const int IRSDK_TICK_RATE = 60;
		private const int IRSDK_360HZ_SAMPLES_PER_FRAME = 6;
		private const float IRSDK_ONE_G = 9.80665f; // in meters per second squared

		private IRacingSdk _irsdk = new();

		private IRacingSdkDatum? _irsdk_brakeDatum = null;
		private IRacingSdkDatum? _irsdk_brakeABSactiveDatum = null;
		private IRacingSdkDatum? _irsdk_carLeftRightDatum = null;
		private IRacingSdkDatum? _irsdk_clutchDatum = null;
		private IRacingSdkDatum? _irsdk_displayUnitsDatum = null;
		private IRacingSdkDatum? _irsdk_gearDatum = null;
		private IRacingSdkDatum? _irsdk_isOnTrackDatum = null;
		private IRacingSdkDatum? _irsdk_lapDistPctDatum = null;
		private IRacingSdkDatum? _irsdk_latAccelDatum = null;
		private IRacingSdkDatum? _irsdk_longAccelDatum = null;
		private IRacingSdkDatum? _irsdk_onPitRoadDatum = null;
		private IRacingSdkDatum? _irsdk_playerCarIdxDatum = null;
		private IRacingSdkDatum? _irsdk_playerTrackSurfaceDatum = null;
		private IRacingSdkDatum? _irsdk_rpmDatum = null;
		private IRacingSdkDatum? _irsdk_sessionFlagsDatum = null;
		private IRacingSdkDatum? _irsdk_speedDatum = null;
		private IRacingSdkDatum? _irsdk_steeringFFBEnabled_Datum = null;
		private IRacingSdkDatum? _irsdk_steeringWheelAngleDatum = null;
		private IRacingSdkDatum? _irsdk_steeringWheelAngleMaxDatum = null;
		private IRacingSdkDatum? _irsdk_steeringWheelTorque_Datum = null;
		private IRacingSdkDatum? _irsdk_steeringWheelTorque_STDatum = null;
		private IRacingSdkDatum? _irsdk_throttleDatum = null;
		private IRacingSdkDatum? _irsdk_velocityXDatum = null;
		private IRacingSdkDatum? _irsdk_velocityYDatum = null;
		private IRacingSdkDatum? _irsdk_weatherDeclaredWetDatum = null;
		private IRacingSdkDatum? _irsdk_yawRateDatum = null;

		private bool _irsdk_telemetryDataInitialized = false;
		private bool _irsdk_sessionInfoReceived = false;

		public bool _irsdk_connected = false;

		public int _irsdk_tickRate = 0;
		public int _irsdk_tickCount = 0;

		public float _irsdk_brake = 0f;
		public bool _irsdk_brakeABSactive = false;
		public IRacingSdkEnum.CarLeftRight _irsdk_carLeftRight = 0;
		public float _irsdk_clutch = 1f;
		public int _irsdk_displayUnits = 0;
		public int _irsdk_gear = 0;
		public bool _irsdk_isOnTrack = false;
		public float _irsdk_lapDistPct = 0f;
		public float _irsdk_latAccel = 0f;
		public float _irsdk_longAccel = 0f;
		public bool _irsdk_onPitRoad = false;
		public int _irsdk_playerCarIdx = 0;
		public IRacingSdkEnum.TrkLoc _irsdk_playerTrackSurface = IRacingSdkEnum.TrkLoc.NotInWorld;
		public float _irsdk_rpm = 0f;
		public IRacingSdkEnum.Flags _irsdk_sessionFlags = 0;
		public float _irsdk_speed = 0f;
		public bool _irsdk_steeringFFBEnabled = false;
		public float _irsdk_steeringWheelAngle = 0f;
		public float _irsdk_steeringWheelAngleMax = 0f;
		public float _irsdk_steeringWheelTorque = 0f;
		public float[] _irsdk_steeringWheelTorque_ST = new float[ IRSDK_360HZ_SAMPLES_PER_FRAME ];
		public float _irsdk_throttle = 0f;
		public float _irsdk_velocityX = 0f;
		public float _irsdk_velocityY = 0f;
		public bool _irsdk_weatherDeclaredWet = false;
		public float _irsdk_yawRate = 0f;

		public string _irsdk_simMode = string.Empty;

		public float _irsdk_shiftLightsFirstRPM = 0f;
		public float _irsdk_shiftLightsShiftRPM = 0f;
		public float _irsdk_shiftLightsBlinkRPM = 0f;

		public string _irsdk_playerCarNumber = string.Empty;

		public int _irsdk_tickCountLastFrame = 0;

		public bool _irsdk_brakeABSactiveLastFrame = false;
		public bool _irsdk_isOnTrackLastFrame = false;
		public float _irsdk_lapDistPctLastFrame = 0f;
		public bool _irsdk_steeringFFBEnabledLastFrame = false;
		public bool _irsdk_weatherDeclaredWetLastFrame = false;

		public float _irsdk_velocity = 0f;
		public float _irsdk_velocityLastFrame = 0f;
		public float _irsdk_gForce = 0f;

		private int _irsdk_updateLoopTickCount = 0;

		private IntPtr? _irsdk_windowHandle = null;

		private void InitializeIRacingSDK()
		{
			WriteLine( "InitializeIRacingSDK called.", true );
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
			WriteLine( "StopIRacingSDK called.", true );

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
			WriteLine( $"OnException called.", true );

			WriteLine( "The following exception was thrown from inside the iRacing SDK:" );
			WriteLine( exception.Message.Trim() );

			throw exception;
		}

		private void OnConnected()
		{
			WriteLine( "OnConnected called.", true );

			Say( Settings.SayConnected, null, false, false );

			_irsdk_connected = true;

			_irsdk_windowHandle = WinApi.FindWindow( null, "iRacing.com Simulator" );

			_ffb_reinitializeNeeded = true;

			ResetAutoOverallScaleMetrics();

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = MarvinsAIRA.MainWindow.Instance;

				if ( mainWindow != null )
				{
					mainWindow.Connection_StatusBarItem.Content = "Connected";
					mainWindow.Connection_StatusBarItem.Foreground = Brushes.ForestGreen;

					mainWindow.SimulatorNotRunning_Label.Visibility = Visibility.Hidden;
				}
			} );
		}

		private void OnDisconnected()
		{
			WriteLine( "OnDisconnected called.", true );

			_irsdk_connected = false;
			_irsdk_sessionInfoReceived = false;

			_irsdk_windowHandle = null;

			_irsdk_tickRate = 0;
			_irsdk_tickCount = 0;

			_irsdk_brake = 0f;
			_irsdk_brakeABSactive = false;
			_irsdk_carLeftRight = IRacingSdkEnum.CarLeftRight.Off;
			_irsdk_clutch = 1f;
			_irsdk_displayUnits = 0;
			_irsdk_gear = 0;
			_irsdk_isOnTrack = false;
			_irsdk_lapDistPct = 0f;
			_irsdk_latAccel = 0f;
			_irsdk_longAccel = 0f;
			_irsdk_onPitRoad = false;
			_irsdk_playerCarIdx = 0;
			_irsdk_playerTrackSurface = IRacingSdkEnum.TrkLoc.NotInWorld;
			_irsdk_rpm = 0f;
			_irsdk_sessionFlags = 0;
			_irsdk_speed = 0f;
			_irsdk_steeringFFBEnabled = false;
			_irsdk_steeringWheelAngle = 0f;
			_irsdk_steeringWheelAngleMax = 0f;
			_irsdk_steeringWheelTorque = 0f;
			_irsdk_steeringWheelTorque_ST[ 0 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 1 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 2 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 3 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 4 ] = 0;
			_irsdk_steeringWheelTorque_ST[ 5 ] = 0;
			_irsdk_throttle = 0f;
			_irsdk_velocityX = 0f;
			_irsdk_velocityY = 0f;
			_irsdk_weatherDeclaredWet = false;
			_irsdk_yawRate = 0f;

			_irsdk_simMode = string.Empty;

			_irsdk_shiftLightsFirstRPM = 0f;
			_irsdk_shiftLightsShiftRPM = 0f;
			_irsdk_shiftLightsBlinkRPM = 0f;

			_irsdk_playerCarNumber = string.Empty;

			_irsdk_tickCountLastFrame = 0;

			_irsdk_brakeABSactiveLastFrame = false;
			_irsdk_isOnTrackLastFrame = false;
			_irsdk_lapDistPctLastFrame = 0f;
			_irsdk_steeringFFBEnabledLastFrame = false;
			_irsdk_weatherDeclaredWetLastFrame = false;

			_irsdk_velocity = 0f;
			_irsdk_velocityLastFrame = 0f;
			_irsdk_gForce = 0f;

			_irsdk_updateLoopTickCount = 0;

			_irsdk.PauseSessionInfoUpdates = false;

			_ffb_recordingNow = false;
			_ffb_playingBackNow = false;
			_ffb_startCooldownNow = true;

			StopABS();

			UpdateCurrentCar();
			UpdateCurrentTrack();
			UpdateCurrentWetDryCondition();

			Say( Settings.SayDisconnected, null, false, false );

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = MarvinsAIRA.MainWindow.Instance;

				if ( mainWindow != null )
				{
					mainWindow.Connection_StatusBarItem.Content = "Disconnected";
					mainWindow.Connection_StatusBarItem.Foreground = Brushes.Red;

					mainWindow.SimulatorNotRunning_Label.Visibility = Visibility.Visible;
				}
			} );
		}

		private void OnSessionInfo()
		{
			_irsdk_sessionInfoReceived = true;

			UpdateCurrentCar();
			UpdateCurrentTrack();
			UpdateCurrentWetDryCondition();

			var sessionInfo = _irsdk.Data.SessionInfo;

			_irsdk_simMode = sessionInfo.WeekendInfo.SimMode;

			WriteLine( $"Sim mode = {_irsdk_simMode}", true );

			_irsdk_shiftLightsFirstRPM = sessionInfo.DriverInfo.DriverCarSLFirstRPM;
			_irsdk_shiftLightsShiftRPM = sessionInfo.DriverInfo.DriverCarSLShiftRPM;
			_irsdk_shiftLightsBlinkRPM = sessionInfo.DriverInfo.DriverCarSLBlinkRPM;

			WriteLine( $"Shift lights RPMs = {_irsdk_shiftLightsFirstRPM}, {_irsdk_shiftLightsShiftRPM}, {_irsdk_shiftLightsBlinkRPM}" );

			var driver = sessionInfo.DriverInfo.Drivers.Find( driver => driver.CarIdx == _irsdk_playerCarIdx );

			_irsdk_playerCarNumber = driver?.CarNumber ?? string.Empty;

			WriteLine( $"Player's car number = {_irsdk_playerCarNumber}" );

#if DEBUG

			var jsonString = JsonConvert.SerializeObject( sessionInfo, Formatting.Indented, [ new StringEnumConverter() ] );

			Debug.WriteLine( jsonString );

#endif
		}

		private void OnTelemetryData()
		{
			if ( !_irsdk_telemetryDataInitialized )
			{
				_irsdk_brakeDatum = _irsdk.Data.TelemetryDataProperties[ "Brake" ];
				_irsdk_brakeABSactiveDatum = _irsdk.Data.TelemetryDataProperties[ "BrakeABSactive" ];
				_irsdk_carLeftRightDatum = _irsdk.Data.TelemetryDataProperties[ "CarLeftRight" ];
				_irsdk_clutchDatum = _irsdk.Data.TelemetryDataProperties[ "Clutch" ];
				_irsdk_displayUnitsDatum = _irsdk.Data.TelemetryDataProperties[ "DisplayUnits" ];
				_irsdk_gearDatum = _irsdk.Data.TelemetryDataProperties[ "Gear" ];
				_irsdk_isOnTrackDatum = _irsdk.Data.TelemetryDataProperties[ "IsOnTrack" ];
				_irsdk_lapDistPctDatum = _irsdk.Data.TelemetryDataProperties[ "LapDistPct" ];
				_irsdk_latAccelDatum = _irsdk.Data.TelemetryDataProperties[ "LatAccel" ];
				_irsdk_longAccelDatum = _irsdk.Data.TelemetryDataProperties[ "LongAccel" ];
				_irsdk_onPitRoadDatum = _irsdk.Data.TelemetryDataProperties[ "OnPitRoad" ];
				_irsdk_playerCarIdxDatum = _irsdk.Data.TelemetryDataProperties[ "PlayerCarIdx" ];
				_irsdk_playerTrackSurfaceDatum = _irsdk.Data.TelemetryDataProperties[ "PlayerTrackSurface" ];
				_irsdk_rpmDatum = _irsdk.Data.TelemetryDataProperties[ "RPM" ];
				_irsdk_sessionFlagsDatum = _irsdk.Data.TelemetryDataProperties[ "SessionFlags" ];
				_irsdk_speedDatum = _irsdk.Data.TelemetryDataProperties[ "Speed" ];
				_irsdk_steeringFFBEnabled_Datum = _irsdk.Data.TelemetryDataProperties[ "SteeringFFBEnabled" ];
				_irsdk_steeringWheelAngleDatum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelAngle" ];
				_irsdk_steeringWheelAngleMaxDatum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelAngleMax" ];
				_irsdk_steeringWheelTorque_Datum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelTorque" ];
				_irsdk_steeringWheelTorque_STDatum = _irsdk.Data.TelemetryDataProperties[ "SteeringWheelTorque_ST" ];
				_irsdk_throttleDatum = _irsdk.Data.TelemetryDataProperties[ "Throttle" ];
				_irsdk_velocityXDatum = _irsdk.Data.TelemetryDataProperties[ "VelocityX" ];
				_irsdk_velocityYDatum = _irsdk.Data.TelemetryDataProperties[ "VelocityY" ];
				_irsdk_weatherDeclaredWetDatum = _irsdk.Data.TelemetryDataProperties[ "WeatherDeclaredWet" ];
				_irsdk_yawRateDatum = _irsdk.Data.TelemetryDataProperties[ "YawRate" ];

				_irsdk_telemetryDataInitialized = true;

#if DEBUG
				foreach ( var telemetryDataProperty in _irsdk.Data.TelemetryDataProperties )
				{
					Debug.WriteLine( $"{telemetryDataProperty.Value.Name}, {telemetryDataProperty.Value.VarType}, {telemetryDataProperty.Value.Desc}, {telemetryDataProperty.Value.Unit}, {telemetryDataProperty.Value.Count}" );
				}
#endif
			}

			_irsdk_tickCountLastFrame = _irsdk_tickCount;

			_irsdk_brakeABSactiveLastFrame = _irsdk_brakeABSactive;
			_irsdk_isOnTrackLastFrame = _irsdk_isOnTrack;
			_irsdk_lapDistPctLastFrame = _irsdk_lapDistPct;
			_irsdk_steeringFFBEnabledLastFrame = _irsdk_steeringFFBEnabled;
			_irsdk_weatherDeclaredWetLastFrame = _irsdk_weatherDeclaredWet;

			_irsdk_tickRate = _irsdk.Data.TickRate;
			_irsdk_tickCount = _irsdk.Data.TickCount;

			_irsdk_brake = _irsdk.Data.GetFloat( _irsdk_brakeDatum );
			_irsdk_brakeABSactive = _irsdk.Data.GetBool( _irsdk_brakeABSactiveDatum );
			_irsdk_carLeftRight = (IRacingSdkEnum.CarLeftRight) _irsdk.Data.GetInt( _irsdk_carLeftRightDatum );
			_irsdk_clutch = _irsdk.Data.GetFloat( _irsdk_clutchDatum );
			_irsdk_displayUnits = _irsdk.Data.GetInt( _irsdk_displayUnitsDatum );
			_irsdk_gear = _irsdk.Data.GetInt( _irsdk_gearDatum );
			_irsdk_isOnTrack = _irsdk.Data.GetBool( _irsdk_isOnTrackDatum );
			_irsdk_lapDistPct = _irsdk.Data.GetFloat( _irsdk_lapDistPctDatum );
			_irsdk_latAccel = _irsdk.Data.GetFloat( _irsdk_latAccelDatum );
			_irsdk_longAccel = _irsdk.Data.GetFloat( _irsdk_longAccelDatum );
			_irsdk_onPitRoad = _irsdk.Data.GetBool( _irsdk_onPitRoadDatum );
			_irsdk_playerCarIdx = _irsdk.Data.GetInt( _irsdk_playerCarIdxDatum );
			_irsdk_playerTrackSurface = (IRacingSdkEnum.TrkLoc) _irsdk.Data.GetInt( _irsdk_playerTrackSurfaceDatum );
			_irsdk_rpm = _irsdk.Data.GetFloat( _irsdk_rpmDatum );
			_irsdk_sessionFlags = (IRacingSdkEnum.Flags) _irsdk.Data.GetBitField( _irsdk_sessionFlagsDatum );
			_irsdk_steeringFFBEnabled = _irsdk.Data.GetBool( _irsdk_steeringFFBEnabled_Datum );
			_irsdk_speed = _irsdk.Data.GetFloat( _irsdk_speedDatum );
			_irsdk_steeringWheelAngle = _irsdk.Data.GetFloat( _irsdk_steeringWheelAngleDatum );
			_irsdk_steeringWheelAngleMax = _irsdk.Data.GetFloat( _irsdk_steeringWheelAngleMaxDatum );
			_irsdk_steeringWheelTorque = _irsdk.Data.GetFloat( _irsdk_steeringWheelTorque_Datum );
			_irsdk_throttle = _irsdk.Data.GetFloat( _irsdk_throttleDatum );
			_irsdk.Data.GetFloatArray( _irsdk_steeringWheelTorque_STDatum, _irsdk_steeringWheelTorque_ST, 0, _irsdk_steeringWheelTorque_ST.Length );
			_irsdk_velocityX = _irsdk.Data.GetFloat( _irsdk_velocityXDatum );
			_irsdk_velocityY = _irsdk.Data.GetFloat( _irsdk_velocityYDatum );
			_irsdk_weatherDeclaredWet = _irsdk.Data.GetBool( _irsdk_weatherDeclaredWetDatum );
			_irsdk_yawRate = _irsdk.Data.GetFloat( _irsdk_yawRateDatum );

			// calculate exact delta time

			var deltaTime = ( _irsdk_tickCount - _irsdk_tickCountLastFrame ) / (float) _irsdk_tickRate;

			// calculate velocity

			_irsdk_velocityLastFrame = _irsdk_velocity;

			_irsdk_velocity = MathF.Sqrt( ( _irsdk_velocityX * _irsdk_velocityX ) + ( _irsdk_velocityY * _irsdk_velocityY ) );

			// calculate g force

			if ( deltaTime > 0 )
			{
				_irsdk_gForce = MathF.Abs( _irsdk_velocity - _irsdk_velocityLastFrame ) / deltaTime / IRSDK_ONE_G;
			}
			else
			{
				_irsdk_gForce = 0;
			}

			// pause session info updates if we are in a replay or off the track

			_irsdk.PauseSessionInfoUpdates = _irsdk_isOnTrack || ( _irsdk_simMode == "replay" );

			// ABS tone

			if ( _irsdk_brakeABSactive != _irsdk_brakeABSactiveLastFrame )
			{
				if ( _irsdk_brakeABSactive )
				{
					PlayABS();
				}
				else
				{
					StopABS();
				}
			}

			// update wet dry condition

			if ( _irsdk_sessionInfoReceived )
			{
				if ( _irsdk_weatherDeclaredWet != _irsdk_weatherDeclaredWetLastFrame )
				{
					UpdateCurrentWetDryCondition();
				}
			}

			// reset auto overall scale if we've moved on or off track

			if ( _irsdk_isOnTrack != _irsdk_isOnTrackLastFrame )
			{
				ResetAutoOverallScaleMetrics();
			}

			// update the force feedback magnitudes

			UpdateForceFeedback();

			// run other stuff at 20 FPS instead of 60

			if ( _irsdk_tickCount - _irsdk_updateLoopTickCount >= 3 )
			{
				_irsdk_updateLoopTickCount = _irsdk_tickCount;

				var mainWindow = MarvinsAIRA.MainWindow.Instance;

				mainWindow?._win_autoResetEvent.Set();
			}
		}

		private void OnDebugLog( string message )
		{
			WriteLine( $"[IRSDK] {message}" );
		}
	}
}
