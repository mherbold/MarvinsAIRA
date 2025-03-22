
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SharpDX.DirectInput;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		#region Properties

		public const int DI_FFNOMINALMAX = 10000;
		public const int DIEB_NOTRIGGER = -1;

		private const int EVENTTYPE_SINGLE = 0;
		private const int EVENTTYPE_PERIODIC = 1;

		private const int FFB_SAMPLES_PER_FRAME = IRSDK_360HZ_SAMPLES_PER_FRAME;
		private const int FFB_UPDATE_FREQUENCY = 360;
		private const int FFB_MICROSECONDS_PER_SECOND = 1000000;
		private const int FFB_MICROSECONDS_PER_UPDATE = FFB_MICROSECONDS_PER_SECOND / FFB_UPDATE_FREQUENCY;
		private const int FFB_NANOSECONDS_PER_SECOND = 1000000000;
		private const double FFB_NANOSECONDS_PER_UPDATE = (double) FFB_NANOSECONDS_PER_SECOND / FFB_UPDATE_FREQUENCY;

		public const int FFB_WRITEABLE_BITMAP_WIDTH = 978;
		public const int FFB_WRITEABLE_BITMAP_HEIGHT = 200;
		public const int FFB_WRITEABLE_BITMAP_DPI = 96;

		public const int FFB_PIXELS_BUFFER_WIDTH = FFB_WRITEABLE_BITMAP_WIDTH + 1;
		public const int FFB_PIXELS_BUFFER_HEIGHT = 200;
		public const int FFB_PIXELS_BUFFER_BYTES_PER_PIXEL = 4;
		public const int FFB_PIXELS_BUFFER_STRIDE = FFB_PIXELS_BUFFER_WIDTH * FFB_PIXELS_BUFFER_BYTES_PER_PIXEL;

		public const string ALL_WHEELS_SAVE_NAME = "All";

		private Joystick? _ffb_drivingJoystick = null;

		private EffectInfo? _ffb_constantForceEffectInfo = null;
		private EffectParameters? _ffb_constantForceEffectParameters = null;
		private Effect? _ffb_constantForceEffect = null;
		private float _ffb_sendStartTimer = 0f;

		private bool _ffb_initialized = false;
		private int _ffb_updatesToSkip = 0;
		private bool _ffb_reinitializeNeeded = false;
		private float _ffb_reinitializeTimer = 0f;
		private bool _ffb_forceFeedbackExceptionThrown = false;
		private UInt32 _ffb_multimediaTimerId = 0;

		private bool _ffb_wheelChanged = false;
		private string _ffb_wheelSaveName = ALL_WHEELS_SAVE_NAME;

		private float _ffb_clippedTimer = 0f;
		private float _ffb_announceOverallScaleTimer = 0f;
		private float _ffb_announceDetailScaleTimer = 0f;
		private float _ffb_announceLFEScaleTimer = 0f;

		private readonly float[] _ffb_autoScaleSteeringWheelTorqueBuffer = new float[ 10000 ];
		private int _ffb_autoScaleSteeringWheelTorqueBufferCount = 0;

		public readonly float[] _ffb_recordedSteeringWheelTorqueBuffer = new float[ FFB_SAMPLES_PER_FRAME * IRSDK_TICK_RATE * 60 * 10 ];
		public int _ffb_recordedSteeringWheelTorqueBufferIndex = 0;
		public bool _ffb_recordingNow = false;
		public bool _ffb_playingBackNow = false;

		private readonly int[] _ffb_outputWheelMagnitudeBuffer = new int[ FFB_SAMPLES_PER_FRAME + 1 ];
		private float _ffb_outputWheelMagnitudeBufferTimer = 0f;
		private int _ffb_resetOutputWheelMagnitudeBufferTimerNow = 0;
		private int _ffb_lastMagnitudeSentToWheel = 0;

		private bool _ffb_startCooldownNow = false;
		private float _ffb_magnitudeCoolDownTimer = 0f;
		private int _ffb_lastNonCooldownMagnitudeSentToWheel = 0;

		private float _ffb_previousSteeringWheelTorque = 0f;
		private float _ffb_runningSteeringWheelTorque = 0f;
		private float _ffb_rawSteadyStateWheelTorque = 0f;
		private float _ffb_steadyStateWheelTorque = 0f;

		public bool _ffb_drawPrettyGraph = false;
		public int _ffb_prettyGraphCurrentX = 0;
		public readonly WriteableBitmap _ffb_writeableBitmap = new( FFB_WRITEABLE_BITMAP_WIDTH, FFB_WRITEABLE_BITMAP_HEIGHT, FFB_WRITEABLE_BITMAP_DPI, FFB_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null );
		public readonly byte[] _ffb_pixels = new byte[ FFB_PIXELS_BUFFER_STRIDE * FFB_PIXELS_BUFFER_HEIGHT ];

		private readonly Stopwatch _ffb_stopwatch = new();

		private readonly float[] _ffb_gForceBuffer = new float[ 120 ];
		private int _ffb_gForceBufferIndex = 0;
		private float _ffb_peakGForce = 0f;

		private float _ffb_yawRateFactorInstant = 0f;
		private float _ffb_yawRateFactorAverage = 0f;
		private readonly float[] _ffb_yawRateFactorBuffer = new float[ 120 ];
		private int _ffb_yawRateFactorBufferIndex = 0;

		private float _ffb_understeerAmount = 0f;
		private float _ffb_understeerAmountLinear = 0f;
		private float _ffb_understeerEffectWaveAngle = 0f;

		private float _ffb_oversteerAmount = 0f;
		private float _ffb_oversteerAmountLinear = 0f;
		private float _ffb_oversteerEffectWaveAngle = 0f;

		private float _ffb_crashProtectionTimer = 0f;
		private float _ffb_crashProtectionScale = 1f;

		private int _ffb_centerWheelForce = 0;

		public bool FFB_Initialized { get => _ffb_initialized; }
		public float FFB_ClippedTimer { get => _ffb_clippedTimer; }
		public int FFB_LastMagnitudeSentToWheel { get => _ffb_lastMagnitudeSentToWheel; }
		public float FFB_PeakGForce { get => _ffb_peakGForce; }
		public float FFB_YawRateFactorInstant { get => _ffb_yawRateFactorInstant; }
		public float FFB_YawRateFactorAverage { get => _ffb_yawRateFactorAverage; }
		public float FFB_UndersteerAmount { get => _ffb_understeerAmount; }
		public float FFB_UndersteerAmountLinear { get => _ffb_understeerAmountLinear; }
		public float FFB_OversteerAmount { get => _ffb_oversteerAmount; }
		public float FFB_OversteerAmountLinear { get => _ffb_oversteerAmountLinear; }
		public bool FFB_AutoScaleSteeringWheelTorqueBufferIsReady { get { return _ffb_autoScaleSteeringWheelTorqueBufferCount >= 2000; } }

		#endregion

		public void InitializeForceFeedback( nint windowHandle, bool isFirstInitialization = false )
		{
			var mainWindow = MarvinsAIRA.MainWindow.Instance;

			if ( isFirstInitialization && ( mainWindow != null ) )
			{
				mainWindow.WheelForceFeedback_Image.Source = _ffb_writeableBitmap;
			}

			if ( _ffb_initialized )
			{
				UninitializeForceFeedback();
			}

			WriteLine( "InitializeForceFeedback called.", true );

			_ffb_stopwatch.Restart();

			WriteLine( "...initializing DirectInput (FF devices only)..." );

			var directInput = new DirectInput();

			DeviceInstance? forceFeedbackDeviceInstance = null;

			DeviceType[] deviceTypeArray = [ DeviceType.Driving, DeviceType.FirstPerson, DeviceType.Joystick, DeviceType.Gamepad ];

			if ( Settings.SelectedFFBDeviceGuid == Guid.Empty )
			{
				WriteLine( "...there is not already a selected force feedback device, looking for the first attached force feedback driving device..." );

				foreach ( var deviceType in deviceTypeArray )
				{
					var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.ForceFeedback | DeviceEnumerationFlags.AttachedOnly );

					forceFeedbackDeviceInstance = deviceInstanceList.FirstOrDefault();

					if ( forceFeedbackDeviceInstance != null )
					{
						Settings.SelectedFFBDeviceGuid = forceFeedbackDeviceInstance.InstanceGuid;
						break;
					}
				}
			}
			else
			{
				WriteLine( "...there is already a selected force feedback device, looking for it..." );

				foreach ( var deviceType in deviceTypeArray )
				{
					bool deviceFound = false;

					var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.ForceFeedback | DeviceEnumerationFlags.AttachedOnly );

					foreach ( var deviceInstance in deviceInstanceList )
					{
						if ( deviceInstance.InstanceGuid == Settings.SelectedFFBDeviceGuid )
						{
							forceFeedbackDeviceInstance = deviceInstance;
							deviceFound = true;
							break;
						}
					}

					if ( deviceFound )
					{
						break;
					}
				}

				if ( forceFeedbackDeviceInstance == null )
				{
					WriteLine( "...we could not find this device again - could it be unplugged or turned off?..." );
				}
			}

			if ( forceFeedbackDeviceInstance != null )
			{
				WriteLine( $"...the {forceFeedbackDeviceInstance.ProductName} device has been selected..." );

				_ffb_drivingJoystick = new Joystick( directInput, forceFeedbackDeviceInstance.InstanceGuid );

				WriteLine( "...checking for constant force support..." );

				var effectInfoList = _ffb_drivingJoystick.GetEffects();

				foreach ( var effectInfo in effectInfoList )
				{
					if ( effectInfo.Guid == EffectGuid.ConstantForce )
					{
						_ffb_constantForceEffectInfo = effectInfo;
					}
				}

				if ( _ffb_constantForceEffectInfo == null )
				{
					WriteLine( "...the device does NOT support constant force effects!" );
				}
				else
				{
					WriteLine( "...the device does support constant force effects..." );
					WriteLine( "...we are good to go with this force feedback driving device." );

					_ffb_initialized = true;

					UpdateWheelSaveName();

					ReinitializeForceFeedbackDevice( windowHandle );
				}
			}
			else
			{
				WriteLine( "...no force feedback driving device was selected!" );
			}
		}

		private void UninitializeForceFeedback( bool disposeOfDrivingJoystick = true )
		{
			WriteLine( "UninitializeForceFeedback called.", true );

			if ( _ffb_multimediaTimerId != 0 )
			{
				WriteLine( "...killing the multimedia timer event..." );

				WinApi.TimeKillEvent( _ffb_multimediaTimerId );

				_ffb_multimediaTimerId = 0;

				WriteLine( "...multimedia timer event killed..." );
			}

			if ( _ffb_constantForceEffect != null )
			{
				WriteLine( "...disposing of the old constant force effect..." );

				var constantForceEffect = _ffb_constantForceEffect;

				_ffb_constantForceEffect = null;

				constantForceEffect.Dispose();

				WriteLine( "...the old constant force effect has been disposed of..." );
			}

			if ( _ffb_drivingJoystick != null )
			{
				WriteLine( "...unacquiring the force feedback device..." );

				_ffb_drivingJoystick.Unacquire();

				WriteLine( "...the force feedback device has been unacquired..." );

				if ( disposeOfDrivingJoystick )
				{
					_ffb_constantForceEffectInfo = null;
					_ffb_drivingJoystick = null;
					_ffb_initialized = false;
				}
			}

			WriteLine( "...force feedback uninitialized." );
		}

		public void ReinitializeForceFeedbackDevice( nint windowHandle )
		{
			UninitializeForceFeedback( false );

			WriteLine( "ReinitializeForceFeedbackDevice called.", true );

			if ( !_ffb_initialized )
			{
				WriteLine( "...the force feedback system has faulted, we will not attempt to reinitialize the force feedback device." );
				return;
			}

			if ( !Settings.ForceFeedbackEnabled )
			{
				WriteLine( "...force feedback has been disabled, we will not attempt to reinitialize the force feedback device." );
				return;
			}

			if ( _ffb_drivingJoystick == null )
			{
				throw new Exception( "_ffb_drivingJoystick == null!" );
			}

			if ( _ffb_constantForceEffectInfo == null )
			{
				throw new Exception( "_ffb_constantForceEffectInfo == null!" );
			}

			try
			{
				WriteLine( "...setting the cooperative level (exclusive background) on the force feedback device..." );

				_ffb_drivingJoystick.SetCooperativeLevel( windowHandle, CooperativeLevel.Exclusive | CooperativeLevel.Background );

				WriteLine( "...the cooperative level has been set..." );
				WriteLine( "...acquiring the force feedback device..." );

				_ffb_drivingJoystick.Acquire();

				WriteLine( "...the force feedback device has been acquired..." );
				WriteLine( "...creating the constant force effect..." );

				_ffb_constantForceEffectParameters = new EffectParameters
				{
					Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets,
					Duration = int.MaxValue,
					Gain = DI_FFNOMINALMAX,
					Axes = [ 0 ],
					Directions = [ 0 ],
					SamplePeriod = 0,
					StartDelay = 0,
					TriggerButton = DIEB_NOTRIGGER,
					TriggerRepeatInterval = 0,
					Envelope = new Envelope
					{
						AttackLevel = 0,
						FadeLevel = 0,
						AttackTime = 0,
						FadeTime = 0,
					},
					Parameters = new ConstantForce
					{
						Magnitude = 0
					}
				};

				_ffb_constantForceEffect = new Effect( _ffb_drivingJoystick, _ffb_constantForceEffectInfo.Guid, _ffb_constantForceEffectParameters );

				WriteLine( "...the constant force effect has been created..." );
				WriteLine( "...downloading the constant force effect..." );

				_ffb_constantForceEffect.Download();

				WriteLine( "...the constant force effect has been downloaded..." );

				UInt32 userCtx = 0;

				var periodInMilliseconds = (UInt32) ( 18 - Settings.Frequency );

				WriteLine( $"...starting the multimedia timer event (period = {periodInMilliseconds} ms)..." );

				_ffb_sendStartTimer = 0;
				_ffb_forceFeedbackExceptionThrown = false;

				_ffb_multimediaTimerId = WinApi.TimeSetEvent( periodInMilliseconds, 0, MultimediaTimerEventCallback, ref userCtx, EVENTTYPE_PERIODIC );

				WriteLine( "...the multimedia timer event has been started..." );
				WriteLine( "...the force feedback device has been reinitialized." );
			}
			catch ( Exception exception )
			{
				WriteLine( "...failed to reinitialize the force feedback device:" );
				WriteLine( exception.Message.Trim() );

				_ffb_forceFeedbackExceptionThrown = true;
				_ffb_reinitializeNeeded = true;
			}
		}

		public void StopForceFeedback()
		{
			UpdateConstantForce( [ 0 ] );

			UninitializeForceFeedback();
		}

		public void ScheduleReinitializeForceFeedback()
		{
			_ffb_reinitializeNeeded = true;
		}

		public void AutoOverallScale()
		{
			var calculatedOverallScale = 1f;

			for ( var i = 0; i < _ffb_autoScaleSteeringWheelTorqueBuffer.Length; i++ )
			{
				var rawSteeringWheelTorque = _ffb_autoScaleSteeringWheelTorqueBuffer[ i ];

				if ( rawSteeringWheelTorque > 1f )
				{
					var scale = Settings.WheelMaxForce / ( ( 1f - ( Settings.AutoOverallScaleClipLimit / 100.0f ) ) * rawSteeringWheelTorque );

					if ( scale < calculatedOverallScale )
					{
						calculatedOverallScale = scale;
					}
				}

				_ffb_autoScaleSteeringWheelTorqueBuffer[ i ] = 0f;
			}

			_ffb_autoScaleSteeringWheelTorqueBufferCount = 0;

			Settings.OverallScale = calculatedOverallScale * 100f;

			if ( Settings.OverallScale < 10f )
			{
				Say( Settings.SayOverallScale, $"{Settings.OverallScale:F1}" );
			}
			else
			{
				Say( Settings.SayOverallScale, $"{Settings.OverallScale:F0}" );
			}
		}

		private float UpdateScale( float scale, float direction )
		{
			if ( direction >= 0f )
			{
				if ( scale < 10f )
				{
					scale += 0.1f;
				}
				else if ( scale < 50f )
				{
					scale += 1f;
				}
				else if ( scale < 100f )
				{
					scale += 2f;
				}
				else if ( scale < 150f )
				{
					scale += 5f;
				}
				else
				{
					scale += 10f;
				}
			}
			else
			{
				if ( scale > 150f )
				{
					scale -= 10f;
				}
				else if ( scale > 100f )
				{
					scale -= 5f;
				}
				else if ( scale > 50f )
				{
					scale -= 2f;
				}
				else if ( scale > 10f )
				{
					scale -= 1f;
				}
				else
				{
					scale -= 0.1f;
				}
			}

			if ( scale < 10f )
			{
				scale = MathF.Round( scale, 1 );
			}
			else if ( scale < 50f )
			{
				scale = MathF.Round( scale, 0 );
			}
			else if ( scale < 100f )
			{
				scale = ( (int) scale ) / 2 * 2;
			}
			else if ( scale < 150f )
			{
				scale = ( (int) scale ) / 5 * 5;
			}
			else
			{
				scale = ( (int) scale ) / 10 * 10;
			}

			return scale;
		}

		public void UpdateForceFeedback( float deltaTime, bool pauseInputProcessing, nint windowHandle )
		{
			var mainWindow = MarvinsAIRA.MainWindow.Instance;

			if ( !pauseInputProcessing )
			{
				var playClickSound = false;

				var buttonPresses = Settings.ReinitForceFeedbackButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( "RESET button pressed!", true );

					ReinitializeForceFeedbackDevice( windowHandle );
				}

				buttonPresses = Settings.AutoOverallScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( "AUTO-OVERALL-SCALE button pressed!", true );

					if ( _ffb_autoScaleSteeringWheelTorqueBufferCount >= 2000 )
					{
						AutoOverallScale();
					}
				}

				buttonPresses = Settings.DecreaseOverallScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"DECREASE-OVERALL-SCALE button pressed! (x{buttonPresses})", true );

					for ( var i = 0; i < buttonPresses; i++ )
					{
						Settings.OverallScale = UpdateScale( Settings.OverallScale, -1f );
					}

					_ffb_announceOverallScaleTimer = 1f;

					playClickSound = true;
				}

				buttonPresses = Settings.IncreaseOverallScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"INCREASE-OVERALL-SCALE button pressed! (x{buttonPresses})", true );

					for ( var i = 0; i < buttonPresses; i++ )
					{
						Settings.OverallScale = UpdateScale( Settings.OverallScale, +1f );
					}

					_ffb_announceOverallScaleTimer = 1f;

					playClickSound = true;
				}

				buttonPresses = Settings.DecreaseDetailScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"DECREASE-DETAIL-SCALE button pressed! (x{buttonPresses})", true );

					for ( var i = 0; i < buttonPresses; i++ )
					{
						Settings.DetailScale = UpdateScale( Settings.DetailScale, -1f );
					}

					_ffb_announceDetailScaleTimer = 1f;

					playClickSound = true;
				}

				buttonPresses = Settings.IncreaseDetailScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"INCREASE-DETAIL-SCALE button pressed! (x{buttonPresses})", true );

					for ( var i = 0; i < buttonPresses; i++ )
					{
						Settings.DetailScale = UpdateScale( Settings.DetailScale, +1f );
					}

					_ffb_announceDetailScaleTimer = 1f;

					playClickSound = true;
				}

				buttonPresses = Settings.UndersteerEffectButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"UNDERSTEER-EFFECT button pressed!", true );

					if ( !Settings.LimitToCentripetalTrack || ( _track_currentTrackDisplayName == "Centripetal Circuit" ) )
					{
						if ( _irsdk_steeringWheelAngle >= 0 )
						{
							Settings.USStartYawRateFactorLeft = (int) _ffb_yawRateFactorInstant;
							Settings.USEndYawRateFactorLeft = (int) _ffb_yawRateFactorInstant + 80;
						}
						else
						{
							Settings.USStartYawRateFactorRight = (int) _ffb_yawRateFactorInstant;
							Settings.USEndYawRateFactorRight = (int) _ffb_yawRateFactorInstant + 80;
						}

						mainWindow?.FixRangeSliders();
					}
				}

				buttonPresses = Settings.DecreaseLFEScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"DECREASE-LFE-SCALE button pressed! (x{buttonPresses})", true );

					for ( var i = 0; i < buttonPresses; i++ )
					{
						Settings.LFEScale = UpdateScale( Settings.LFEScale, -1f );
					}

					_ffb_announceLFEScaleTimer = 1f;

					playClickSound = true;
				}

				buttonPresses = Settings.IncreaseLFEScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( $"INCREASE-LFE-SCALE button pressed! (x{buttonPresses})", true );

					for ( var i = 0; i < buttonPresses; i++ )
					{
						Settings.LFEScale = UpdateScale( Settings.LFEScale, +1f );
					}

					_ffb_announceLFEScaleTimer = 1f;

					playClickSound = true;
				}

				if ( playClickSound )
				{
					PlayClick();
				}
			}

			if ( _ffb_announceOverallScaleTimer > 0f )
			{
				_ffb_announceOverallScaleTimer -= deltaTime;

				if ( _ffb_announceOverallScaleTimer <= 0f )
				{
					if ( Settings.OverallScale < 10f )
					{
						Say( Settings.SayOverallScale, $"{Settings.OverallScale:F1}" );
					}
					else
					{
						Say( Settings.SayOverallScale, $"{Settings.OverallScale:F0}" );
					}
				}
			}

			if ( _ffb_announceDetailScaleTimer > 0f )
			{
				_ffb_announceDetailScaleTimer -= deltaTime;

				if ( _ffb_announceDetailScaleTimer <= 0f )
				{
					if ( Settings.DetailScale < 10f )
					{
						Say( Settings.SayDetailScale, $"{Settings.DetailScale:F1}" );
					}
					else
					{
						Say( Settings.SayDetailScale, $"{Settings.DetailScale:F0}" );
					}
				}
			}

			if ( _ffb_announceLFEScaleTimer > 0f )
			{
				_ffb_announceLFEScaleTimer -= deltaTime;

				if ( _ffb_announceLFEScaleTimer <= 0f )
				{
					if ( Settings.LFEScale < 10f )
					{
						Say( Settings.SayLFEScale, $"{Settings.LFEScale:F1}" );
					}
					else
					{
						Say( Settings.SayLFEScale, $"{Settings.LFEScale:F0}" );
					}
				}
			}

			if ( _ffb_reinitializeNeeded )
			{
				_ffb_reinitializeNeeded = false;

				_ffb_reinitializeTimer = Math.Max( 1f, _ffb_reinitializeTimer );
			}

			if ( _ffb_reinitializeTimer > 0f )
			{
				_ffb_reinitializeTimer -= deltaTime;

				if ( _ffb_reinitializeTimer <= 0f )
				{
					ReinitializeForceFeedbackDevice( windowHandle );
				}
			}

			// load force feedback settings if the wheel, car, track, or track configuration has changed

			if ( _ffb_wheelChanged || _car_carChanged || _track_trackChanged || _track_trackConfigChanged || _wetdry_conditionChanged )
			{
				WriteLine( $"Loading force feedback configuration [{_ffb_wheelSaveName}, {_car_carSaveName}, {_track_trackSaveName}, {_track_trackConfigSaveName}, {_wetdry_conditionSaveName}]" );

				_settings_pauseSerialization = true;

				Settings.ForceFeedbackSettings? bestMatchingForceFeedbackSettings = null;

				var bestMatchRanking = 0;

				foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
				{
					var ranking = 0;

					if ( forceFeedbackSettings.WheelName == _ffb_wheelSaveName ) ranking += 20;
					if ( forceFeedbackSettings.CarName == _car_carSaveName ) ranking += 20;
					if ( forceFeedbackSettings.TrackName == _track_trackSaveName ) ranking += 20;
					if ( forceFeedbackSettings.TrackConfigName == _track_trackConfigSaveName ) ranking += 20;
					if ( forceFeedbackSettings.WetDryConditionName == _wetdry_conditionSaveName ) ranking += 20;

					if ( ranking > bestMatchRanking )
					{
						bestMatchRanking = ranking;
						bestMatchingForceFeedbackSettings = forceFeedbackSettings;
					}
				}

				if ( bestMatchingForceFeedbackSettings == null )
				{
					bestMatchingForceFeedbackSettings = new Settings.ForceFeedbackSettings();

					Say( Settings.SayScalesReset );
				}

				Settings.OverallScale = bestMatchingForceFeedbackSettings.OverallScale;
				Settings.DetailScale = bestMatchingForceFeedbackSettings.DetailScale;

				if ( Settings.OverallScale < 10f )
				{
					Say( Settings.SayOverallScale, $"{Settings.OverallScale:F1}" );
				}
				else
				{
					Say( Settings.SayOverallScale, $"{Settings.OverallScale:F0}" );
				}

				if ( Settings.DetailScale < 10f )
				{
					Say( Settings.SayDetailScale, $"{Settings.DetailScale:F1}" );
				}
				else
				{
					Say( Settings.SayDetailScale, $"{Settings.DetailScale:F0}" );
				}

				_settings_pauseSerialization = false;
			}

			// load yaw rate factors if the car has changed

			if ( _car_carChanged )
			{
				WriteLine( $"Loading steering effects configuration [{_car_currentCarScreenName}]", true );

				_settings_pauseSerialization = true;

				var steeringEffectsSettingsFound = false;

				foreach ( var steeringEffectsSettings in Settings.SteeringEffectsSettingsList )
				{
					if ( steeringEffectsSettings.CarName == _car_currentCarScreenName )
					{
						Settings.SteeringEffectsEnabled = steeringEffectsSettings.SteeringEffectsEnabled;

						Settings.USStartYawRateFactorLeft = steeringEffectsSettings.USStartYawRateFactorLeft;
						Settings.USEndYawRateFactorLeft = steeringEffectsSettings.USEndYawRateFactorLeft;

						Settings.USStartYawRateFactorRight = steeringEffectsSettings.USStartYawRateFactorRight;
						Settings.USEndYawRateFactorRight = steeringEffectsSettings.USEndYawRateFactorRight;

						Settings.OSEndYVelocity = steeringEffectsSettings.OSEndYVelocity;
						Settings.OSStartYVelocity = steeringEffectsSettings.OSStartYVelocity;

						Settings.OSSoftness = steeringEffectsSettings.OSSoftness;

						steeringEffectsSettingsFound = true;

						break;
					}
				}

				if ( !steeringEffectsSettingsFound )
				{
					Settings.SteeringEffectsEnabled = false;

					Settings.USStartYawRateFactorLeft = 120;
					Settings.USEndYawRateFactorLeft = 180;

					Settings.USStartYawRateFactorRight = 120;
					Settings.USEndYawRateFactorRight = 180;

					Settings.OSStartYVelocity = 3f;
					Settings.OSEndYVelocity = 8f;

					Settings.OSSoftness = 10f;
				}

				_settings_pauseSerialization = false;

				mainWindow?.FixRangeSliders();
			}

			// reset changed things

			_ffb_wheelChanged = false;
			_car_carChanged = false;
			_track_trackChanged = false;
			_track_trackConfigChanged = false;
			_wetdry_conditionChanged = false;

			// auto-center wheel feature

			if ( Settings.ForceFeedbackEnabled && _ffb_initialized && !_ffb_forceFeedbackExceptionThrown )
			{
				if ( Settings.AutoCenterWheel )
				{
					if ( ( !_irsdk_isOnTrack || ( _irsdk_simMode == "replay" ) ) && !_ffb_playingBackNow )
					{
						var leftRange = (float) ( Settings.WheelCenterValue - Settings.WheelMinValue );
						var rightRange = (float) ( Settings.WheelMaxValue - Settings.WheelCenterValue );

						if ( ( leftRange > 0f ) && ( rightRange > 0f ) )
						{
							var totalRange = leftRange + rightRange;

							var normalizedWheelVelocity = Input_CurrentWheelVelocity / totalRange; // -1 to 1

							var currentWheelPosition = Math.Clamp( Input_CurrentWheelPosition, Settings.WheelMinValue, Settings.WheelMaxValue );

							var leftDelta = (float) -Math.Min( 0f, currentWheelPosition - Settings.WheelCenterValue );
							var rightDelta = (float) Math.Max( 0f, currentWheelPosition - Settings.WheelCenterValue );

							var leftPercentage = leftDelta / leftRange; // 0 to 1
							var rightPercentage = rightDelta / rightRange; // 0 to 1

							var normalizedPercentage = rightPercentage - leftPercentage; // -1 to 1

							var forceMagnitude = 0;

							if ( !pauseInputProcessing )
							{
								if ( Settings.AutoCenterWheelType == 0 )
								{
									var brake = false;
									var targetNormalizedWheelVelocity = -normalizedPercentage;

									if ( leftPercentage >= 0.015f )
									{
										if ( normalizedWheelVelocity < -0.01f )
										{
											brake = true;
										}
										else if ( normalizedWheelVelocity < targetNormalizedWheelVelocity )
										{
											_ffb_centerWheelForce -= Settings.AutoCenterWheelStrength * 2;
										}
										else
										{
											_ffb_centerWheelForce += Settings.AutoCenterWheelStrength;
										}
									}
									else if ( rightPercentage >= 0.015f )
									{
										if ( normalizedWheelVelocity > 0.01f )
										{
											brake = true;
										}
										else if ( normalizedWheelVelocity > targetNormalizedWheelVelocity )
										{
											_ffb_centerWheelForce += Settings.AutoCenterWheelStrength * 2;
										}
										else
										{
											_ffb_centerWheelForce -= Settings.AutoCenterWheelStrength;
										}
									}
									else
									{
										_ffb_centerWheelForce = 0;

										if ( ( normalizedWheelVelocity < -0.01f ) || ( normalizedWheelVelocity > 0.01f ) )
										{
											brake = true;
										}
									}

									if ( brake )
									{
										_ffb_centerWheelForce = 0;

										forceMagnitude = (int) ( normalizedWheelVelocity * DI_FFNOMINALMAX * 0.25f );
									}
									else
									{
										forceMagnitude = _ffb_centerWheelForce;
									}
								}
								else if ( Settings.AutoCenterWheelType == 1 )
								{
									if ( leftPercentage >= 0.015f )
									{
										forceMagnitude = (int) -( MathF.Pow( leftPercentage, 2f ) * DI_FFNOMINALMAX * Settings.AutoCenterWheelStrength / 100f );
									}
									else if ( rightPercentage >= 0.015f )
									{
										forceMagnitude = (int) ( MathF.Pow( rightPercentage, 2f ) * DI_FFNOMINALMAX * Settings.AutoCenterWheelStrength / 100f );
									}
								}
							}

							UpdateConstantForce( [ forceMagnitude ] );
						}
					}
				}
			}
		}

		public bool TogglePrettyGraph()
		{
			_ffb_drawPrettyGraph = !_ffb_drawPrettyGraph;

			return _ffb_drawPrettyGraph;
		}

		public void SendTestForceFeedbackSignal( bool invert )
		{
			var magnitude = invert ? -500 : 500;

			UpdateConstantForce( [ 0, magnitude * 1, magnitude * 2, magnitude * 3, magnitude * 2, magnitude * 1, 0 ] );
		}

		public void UpdateWheelSaveName()
		{
			var oldWheelSaveName = _ffb_wheelSaveName;

			_ffb_wheelSaveName = ALL_WHEELS_SAVE_NAME;

			if ( Settings.SaveSettingsPerWheel )
			{
				if ( _ffb_drivingJoystick != null )
				{
					_ffb_wheelSaveName = _ffb_drivingJoystick.Information.ProductName;
				}
			}

			if ( _ffb_wheelSaveName != oldWheelSaveName )
			{
				_ffb_wheelChanged = true;
			}
		}

		public void UpdateConstantForce( int[] forceMagnitudeList )
		{
			for ( var i = 0; i < _ffb_outputWheelMagnitudeBuffer.Length; i++ )
			{
				var value = forceMagnitudeList[ i % forceMagnitudeList.Length ];

				_ffb_outputWheelMagnitudeBuffer[ i ] = value;
			}

			_ffb_updatesToSkip = 6;
			_ffb_resetOutputWheelMagnitudeBufferTimerNow = 1;
			_ffb_previousSteeringWheelTorque = 0f;
			_ffb_runningSteeringWheelTorque = 0f;
			_ffb_rawSteadyStateWheelTorque = 0f;
			_ffb_steadyStateWheelTorque = 0f;
		}

		private void UpdateForceFeedback()
		{
			// calculate current instant yaw rate factor

			if ( ( MathF.Abs( _irsdk_yawRate ) >= 5f * 0.0175f ) && ( _irsdk_velocityX > 0f ) )
			{
				_ffb_yawRateFactorInstant = _irsdk_steeringWheelAngle * _irsdk_speed / _irsdk_yawRate;
			}
			else
			{
				_ffb_yawRateFactorInstant = 0f;
			}

			// keep track of average yaw rate factor over the last two seconds

			_ffb_yawRateFactorBuffer[ _ffb_yawRateFactorBufferIndex ] = _ffb_yawRateFactorInstant;

			_ffb_yawRateFactorBufferIndex = ( _ffb_yawRateFactorBufferIndex + 1 ) % _ffb_yawRateFactorBuffer.Length;

			var totalYawRateFactor = 0f;

			for ( var i = 0; i < _ffb_yawRateFactorBuffer.Length; i++ )
			{
				totalYawRateFactor += _ffb_yawRateFactorBuffer[ i ];
			}

			_ffb_yawRateFactorAverage = totalYawRateFactor / _ffb_yawRateFactorBuffer.Length;

			// keep track of peak g force over the last two seconds

			_ffb_gForceBuffer[ _ffb_gForceBufferIndex ] = _irsdk_gForce;

			_ffb_gForceBufferIndex = ( _ffb_gForceBufferIndex + 1 ) % _ffb_gForceBuffer.Length;

			var peakGForce = 0f;

			for ( var i = 0; i < _ffb_gForceBuffer.Length; i++ )
			{
				peakGForce = Math.Max( peakGForce, _ffb_gForceBuffer[ i ] );
			}

			_ffb_peakGForce = peakGForce;

			// stop here if force feedback is not enabled

			if ( !Settings.ForceFeedbackEnabled )
			{
				return;
			}

			// warn if FFB is enabled in iRacing

			if ( _irsdk_steeringFFBEnabled && !_irsdk_steeringFFBEnabledLastFrame )
			{
				Say( Settings.SayFFBWarning );
			}

			// see if we want to process FFB without actually updating the FFB magnitude buffer

			var processThisFrame = ( Interlocked.Decrement( ref _ffb_updatesToSkip ) < 0 );

			// get the FFB from the telemetry data

			float[] steeringWheelTorque_ST = [
				_irsdk_steeringWheelTorque_ST[ 0 ],
				_irsdk_steeringWheelTorque_ST[ 1 ],
				_irsdk_steeringWheelTorque_ST[ 2 ],
				_irsdk_steeringWheelTorque_ST[ 3 ],
				_irsdk_steeringWheelTorque_ST[ 4 ],
				_irsdk_steeringWheelTorque_ST[ 5 ]
			];

			var softLockMagnitude = 0;

			if ( _ffb_playingBackNow )
			{
				for ( var x = 0; x < steeringWheelTorque_ST.Length; x++ )
				{
					steeringWheelTorque_ST[ x ] = _ffb_recordedSteeringWheelTorqueBuffer[ _ffb_recordedSteeringWheelTorqueBufferIndex ];

					_ffb_recordedSteeringWheelTorqueBufferIndex = ( _ffb_recordedSteeringWheelTorqueBufferIndex + 1 ) % _ffb_recordedSteeringWheelTorqueBuffer.Length;
				}
			}
			else if ( !_irsdk_isOnTrack || ( _irsdk_simMode == "replay" ) )
			{
				if ( _irsdk_simMode != "replay" )
				{
					if ( _irsdk_isOnTrackLastFrame )
					{
						_ffb_startCooldownNow = true;
					}
				}

				steeringWheelTorque_ST[ 0 ] = 0f;
				steeringWheelTorque_ST[ 1 ] = 0f;
				steeringWheelTorque_ST[ 2 ] = 0f;
				steeringWheelTorque_ST[ 3 ] = 0f;
				steeringWheelTorque_ST[ 4 ] = 0f;
				steeringWheelTorque_ST[ 5 ] = 0f;
			}
			else if ( Settings.EnableSoftLock )
			{
				var softLockMarginInRadians = Settings.SoftLockMargin * MathF.PI / 180f;

				var softLockPercentage = Math.Clamp( ( MathF.Abs( _irsdk_steeringWheelAngle ) - ( _irsdk_steeringWheelAngleMax * .5f - softLockMarginInRadians ) ) / softLockMarginInRadians, 0f, 1f );

				softLockMagnitude = (int) ( -MathF.Sign( _irsdk_steeringWheelAngle ) * softLockPercentage * Settings.SoftLockStrength / 100f * DI_FFNOMINALMAX );
			}

			// crash protection processing

			if ( Settings.EnableCrashProtection )
			{
				bool crashProtectionWasOff = _ffb_crashProtectionTimer == 0f;

				if ( MathF.Abs( _irsdk_gForce ) >= Settings.GForce )
				{
					_ffb_crashProtectionTimer = Settings.CrashDuration;
				}

				if ( crashProtectionWasOff && ( _ffb_crashProtectionTimer > 0f ) )
				{
					Say( Settings.SayCrashProtectionOn );
				}
			}

			if ( _ffb_crashProtectionTimer > 0f )
			{
				_ffb_crashProtectionScale = 0f;

				_ffb_crashProtectionTimer = Math.Max( 0f, _ffb_crashProtectionTimer - ( 1f / _irsdk_tickRate ) );

				if ( _ffb_crashProtectionTimer == 0f )
				{
					Say( Settings.SayCrashProtectionOff );
				}
			}
			else
			{
				_ffb_crashProtectionScale = ( _ffb_crashProtectionScale * 0.99f ) + ( 1f * 0.01f );
			}

			// understeer effect

			var understeerFrequency = 0f;

			if ( _ffb_yawRateFactorInstant > 0f )
			{
				if ( _irsdk_steeringWheelAngle >= 0f )
				{
					_ffb_understeerAmountLinear = Math.Clamp( ( _ffb_yawRateFactorInstant - Settings.USStartYawRateFactorLeft ) / (float) ( Settings.USEndYawRateFactorLeft - Settings.USStartYawRateFactorLeft ), 0f, 1f );
				}
				else
				{
					_ffb_understeerAmountLinear = Math.Clamp( ( _ffb_yawRateFactorInstant - Settings.USStartYawRateFactorRight ) / (float) ( Settings.USEndYawRateFactorRight - Settings.USStartYawRateFactorRight ), 0f, 1f );
				}

				understeerFrequency = Math.Max( 0.25f, _ffb_understeerAmountLinear );

				_ffb_understeerAmount = MathF.Pow( _ffb_understeerAmountLinear, Settings.USCurve );
			}
			else
			{
				_ffb_understeerAmount = 0f;
				_ffb_understeerAmountLinear = 0f;
			}

			// oversteer effect

			var oversteerFrequency = 0f;

			if ( MathF.Sign( _irsdk_steeringWheelAngle ) != MathF.Sign( _irsdk_velocityY ) )
			{
				_ffb_oversteerAmountLinear = Math.Clamp( ( MathF.Abs( _irsdk_velocityY ) - Settings.OSStartYVelocity ) / ( Settings.OSEndYVelocity - Settings.OSStartYVelocity ), 0f, 1f );

				oversteerFrequency = Math.Max( 0.25f, _ffb_oversteerAmountLinear );

				_ffb_oversteerAmount = MathF.Pow( _ffb_oversteerAmountLinear, Settings.OSCurve );

				if ( Settings.OSSoftness >= 0.1f )
				{
					var softnessInRadians = Settings.OSSoftness * MathF.PI / 180f;

					_ffb_oversteerAmount *= 1f - Math.Clamp( ( softnessInRadians - MathF.Abs( _irsdk_steeringWheelAngle ) ) / softnessInRadians, 0f, 1f ); ;
				}
			}
			else
			{
				_ffb_oversteerAmount = 0f;
				_ffb_oversteerAmountLinear = 0f;
			}

			// we want to reduce forces while the car is moving very slow or parked

			var speedScale = 1f;

			if ( !_ffb_playingBackNow && ( _irsdk_speed < 5f ) )
			{
				var t = _irsdk_speed / 5f;

				speedScale = ( Settings.ParkedScale / 100f ) * ( 1f - t ) + t;
			}

			// calculate the conversion scale from Newton-meters to (-DI_FFNOMINALMAX, DI_FFNOMINALMAX)

			var newtonMetersToDirectInputUnits = DI_FFNOMINALMAX / Settings.WheelMaxForce;

			// normalize the scale settings ("100" slider value = 1.0)

			var normalizedOverallScaleSetting = Settings.OverallScale / 100f;
			var normalizedDetailScaleSetting = Settings.DetailScale / 100f;
			var normalizedLFEScaleSetting = Settings.LFEScale / 100f;

			// apply crash protection to overall and detail scale

			normalizedOverallScaleSetting = ( normalizedOverallScaleSetting * ( 1f - Settings.CrashProtectionOverallScale / 100f ) * ( 1f - _ffb_crashProtectionScale ) ) + ( normalizedOverallScaleSetting * _ffb_crashProtectionScale );
			normalizedDetailScaleSetting *= _ffb_crashProtectionScale;

			// map scales into DI units (detail scale will be the same as the overall scale if detail scale slider is set to 100%)

			var overallScaleToDirectInputUnits = normalizedOverallScaleSetting * newtonMetersToDirectInputUnits;
			var detailScaleToDirectInputUnits = overallScaleToDirectInputUnits + overallScaleToDirectInputUnits * ( normalizedDetailScaleSetting - 1f );
			var understeerEffectScaleToDirectInputUnits = (int) ( ( Settings.USEffectStrength / 100f ) * DI_FFNOMINALMAX );
			var oversteerEffectScaleToDirectInputUnits = (int) ( ( Settings.OSEffectStrength / 100f ) * DI_FFNOMINALMAX );
			var lfeScale = normalizedLFEScaleSetting * DI_FFNOMINALMAX;

			// make a copy of the lfe read index so it doesn't change in the middle of this update (its updated in another thread)

			var lfeMagnitudeIndex = _lfe_magnitudeIndex;

			// save the last magnitude sample from the last frame to make interpolation even better

			if ( processThisFrame )
			{
				_ffb_outputWheelMagnitudeBuffer[ 0 ] = _ffb_outputWheelMagnitudeBuffer[ 6 ];
			}

			// go through each sample

			for ( var x = 0; x < steeringWheelTorque_ST.Length; x++ )
			{
				// output buffer index

				var outputWheelMagnitudeBufferIndex = x + 1;

				// get the next steering wheel torque sample (it is in Newton-meters)

				var currentSteeringWheelTorque = steeringWheelTorque_ST[ x ];

				// save the original steering wheel torque (for playback feature)

				if ( _ffb_recordingNow )
				{
					_ffb_recordedSteeringWheelTorqueBuffer[ _ffb_recordedSteeringWheelTorqueBufferIndex ] = currentSteeringWheelTorque;

					_ffb_recordedSteeringWheelTorqueBufferIndex = ( _ffb_recordedSteeringWheelTorqueBufferIndex + 1 ) % _ffb_recordedSteeringWheelTorqueBuffer.Length;
				}

				// calculate the impulse (change in steering wheel torque compared to the last sample)

				var deltaSteeringWheelTorque = currentSteeringWheelTorque - _ffb_previousSteeringWheelTorque;

				_ffb_previousSteeringWheelTorque = currentSteeringWheelTorque;

				// delta limiter

				var limitedDeltaSteeringWheelTorque = Math.Clamp( deltaSteeringWheelTorque, -0.09f, 0.09f );

				// calculate raw steady state wheel torque

				_ffb_rawSteadyStateWheelTorque += limitedDeltaSteeringWheelTorque;

				_ffb_rawSteadyStateWheelTorque = ( _ffb_rawSteadyStateWheelTorque * 0.9f ) + ( currentSteeringWheelTorque * 0.1f );

				// calculate scaled steady state wheel torque

				_ffb_steadyStateWheelTorque += limitedDeltaSteeringWheelTorque * overallScaleToDirectInputUnits;

				_ffb_steadyStateWheelTorque = ( _ffb_steadyStateWheelTorque * 0.9f ) + ( currentSteeringWheelTorque * overallScaleToDirectInputUnits * 0.1f );

				// algorithm is different for detail scale >= 100 and < 100

				if ( normalizedDetailScaleSetting >= 1 )
				{
					// scale the impulse by our detail scale and add it to our running steering wheel torque

					_ffb_runningSteeringWheelTorque += deltaSteeringWheelTorque * detailScaleToDirectInputUnits;

					// ramp our running scaled magnitude towards the original signal (feed in steady state signal)

					_ffb_runningSteeringWheelTorque = ( _ffb_runningSteeringWheelTorque * 0.9f ) + ( currentSteeringWheelTorque * overallScaleToDirectInputUnits * 0.1f );
				}
				else
				{
					// blend between steady state force and original force using detail scale amount

					_ffb_runningSteeringWheelTorque = ( currentSteeringWheelTorque * overallScaleToDirectInputUnits * normalizedDetailScaleSetting ) + ( _ffb_steadyStateWheelTorque * ( 1f - normalizedDetailScaleSetting ) );
				}

				// apply the speed scale

				if ( processThisFrame )
				{
					_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] = (int) ( _ffb_runningSteeringWheelTorque * speedScale );
				}

				// mix in the low frequency effects

				if ( Settings.LFEToFFBEnabled )
				{
					if ( processThisFrame )
					{
						_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( _lfe_magnitude[ lfeMagnitudeIndex, x ] * lfeScale );
					}
				}

				// add in the steering wheel stops magnitude

				_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += softLockMagnitude;

				// mix in the understeer and oversteer effects

				if ( Settings.SteeringEffectsEnabled )
				{
					_ffb_understeerEffectWaveAngle += understeerFrequency;
					_ffb_oversteerEffectWaveAngle += oversteerFrequency;

					if ( _ffb_understeerEffectWaveAngle > MathF.PI * 2f )
					{
						_ffb_understeerEffectWaveAngle -= MathF.PI * 2f;
					}

					if ( _ffb_oversteerEffectWaveAngle > MathF.PI * 2f )
					{
						_ffb_oversteerEffectWaveAngle -= MathF.PI * 2f;
					}

					if ( processThisFrame )
					{
						// understeer effects

						if ( Settings.USEffectStyle == 0 )
						{
							var waveAmplitude = MathF.Sin( _ffb_understeerEffectWaveAngle );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_understeerAmount * understeerEffectScaleToDirectInputUnits );
						}
						else if ( Settings.USEffectStyle == 1 )
						{
							var waveAmplitude = _ffb_understeerEffectWaveAngle / ( MathF.PI * 2f ) * ( ( _irsdk_steeringWheelAngle >= 0f ) ? 1f : -1f );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_understeerAmount * understeerEffectScaleToDirectInputUnits );
						}
						else if ( Settings.USEffectStyle == 2 )
						{
							var waveAmplitude = ( ( _irsdk_steeringWheelAngle >= 0f ) ? 1f : -1f );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_understeerAmount * understeerEffectScaleToDirectInputUnits );
						}
						else if ( Settings.USEffectStyle == 3 )
						{
							var waveAmplitude = ( ( _irsdk_steeringWheelAngle >= 0f ) ? -1f : 1f );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_understeerAmount * understeerEffectScaleToDirectInputUnits );
						}

						// oversteer effects

						if ( Settings.OSEffectStyle == 0 )
						{
							var waveAmplitude = MathF.Sin( _ffb_oversteerEffectWaveAngle );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_oversteerAmount * oversteerEffectScaleToDirectInputUnits );
						}
						else if ( Settings.OSEffectStyle == 1 )
						{
							var waveAmplitude = _ffb_oversteerEffectWaveAngle / ( MathF.PI * 2f ) * ( ( _irsdk_steeringWheelAngle >= 0f ) ? 1f : -1f );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_oversteerAmount * oversteerEffectScaleToDirectInputUnits );
						}
						else if ( Settings.OSEffectStyle == 2 )
						{
							var waveAmplitude = ( ( _irsdk_steeringWheelAngle >= 0f ) ? 1f : -1f );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_oversteerAmount * oversteerEffectScaleToDirectInputUnits );
						}
						else if ( Settings.OSEffectStyle == 3 )
						{
							var waveAmplitude = ( ( _irsdk_steeringWheelAngle >= 0f ) ? -1f : 1f );

							_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] += (int) ( waveAmplitude * _ffb_oversteerAmount * oversteerEffectScaleToDirectInputUnits );
						}
					}
				}

				// reset the magnitude index now

				_ffb_resetOutputWheelMagnitudeBufferTimerNow = 1;

				// save the original steering wheel torque (for auto-overall-scale feature)

				if ( processThisFrame )
				{
					if ( !_irsdk_onPitRoad && ( _irsdk_playerTrackSurface != IRSDKSharper.IRacingSdkEnum.TrkLoc.OffTrack ) )
					{
						var absOutputWheelMagnitude = MathF.Abs( _ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] );

						if ( absOutputWheelMagnitude > 0f )
						{
							var autoScaleSteeringWheelTorqueBufferIndex = Math.Clamp( (int) ( _irsdk_lapDistPct * _ffb_autoScaleSteeringWheelTorqueBuffer.Length ), 0, _ffb_autoScaleSteeringWheelTorqueBuffer.Length - 1 );

							if ( _ffb_autoScaleSteeringWheelTorqueBuffer[ autoScaleSteeringWheelTorqueBufferIndex ] == 0f )
							{
								_ffb_autoScaleSteeringWheelTorqueBufferCount++;
							}

							var outputWheelMagnitudeInNm = absOutputWheelMagnitude / overallScaleToDirectInputUnits;

							_ffb_autoScaleSteeringWheelTorqueBuffer[ autoScaleSteeringWheelTorqueBufferIndex ] = Math.Max( _ffb_autoScaleSteeringWheelTorqueBuffer[ autoScaleSteeringWheelTorqueBufferIndex ], outputWheelMagnitudeInNm );
						}
					}
				}

				// update the pretty graph

				if ( _ffb_drawPrettyGraph )
				{
					var forceFeedbackMaxToPixelBufferHeightScale = DI_FFNOMINALMAX * 2f / ( FFB_PIXELS_BUFFER_HEIGHT - 40f );
					var halfPixelBufferHeight = FFB_PIXELS_BUFFER_HEIGHT / 2f;

					var centerLine = FFB_PIXELS_BUFFER_HEIGHT / 2;

					var midPoint1 = (int) ( +5000 / forceFeedbackMaxToPixelBufferHeightScale + halfPixelBufferHeight );
					var midPoint2 = (int) ( -5000 / forceFeedbackMaxToPixelBufferHeightScale + halfPixelBufferHeight );

					var clipPoint1 = 19;
					var clipPoint2 = FFB_PIXELS_BUFFER_HEIGHT - 20;

					var iY2 = (int) ( currentSteeringWheelTorque * overallScaleToDirectInputUnits / forceFeedbackMaxToPixelBufferHeightScale + halfPixelBufferHeight ) + 1;
					var iY1 = iY2 - 2;

					var oY2 = (int) ( _ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] / forceFeedbackMaxToPixelBufferHeightScale + halfPixelBufferHeight ) + 1;
					var oY1 = oY2 - 2;

					if ( oY1 < clipPoint1 )
					{
						oY2 = clipPoint1;
					}
					else if ( oY2 > clipPoint2 )
					{
						oY1 = clipPoint2;
					}

					for ( var y = 0; y < FFB_PIXELS_BUFFER_HEIGHT; y++ )
					{
						var offset = y * FFB_PIXELS_BUFFER_STRIDE + _ffb_prettyGraphCurrentX * FFB_PIXELS_BUFFER_BYTES_PER_PIXEL;

						_ffb_pixels[ offset + 0 ] = 0;
						_ffb_pixels[ offset + 1 ] = 0;
						_ffb_pixels[ offset + 2 ] = 0;
						_ffb_pixels[ offset + 3 ] = 255;

						var onInput = ( y >= iY1 ) && ( y <= iY2 );
						var onOutput = ( y >= oY1 ) && ( y <= oY2 );

						if ( onInput )
						{
							_ffb_pixels[ offset + 0 ] = 128;
							_ffb_pixels[ offset + 1 ] = 128;
							_ffb_pixels[ offset + 2 ] = 255;
						}

						if ( onOutput )
						{
							if ( ( y < clipPoint1 ) || ( y > clipPoint2 ) )
							{
								_ffb_pixels[ offset + 2 ] = 255;
							}
							else
							{
								_ffb_pixels[ offset + 0 ] = 255;
								_ffb_pixels[ offset + 1 ] = 255;
							}
						}

						if ( !onInput && !onOutput )
						{
							if ( ( y == clipPoint1 ) || ( y == clipPoint2 ) )
							{
								_ffb_pixels[ offset + 2 ] = 128;
							}
							else if ( ( y == midPoint1 ) || ( y == midPoint2 ) )
							{
								_ffb_pixels[ offset + 0 ] = 16;
								_ffb_pixels[ offset + 1 ] = 16;
								_ffb_pixels[ offset + 2 ] = 16;
							}
							else if ( y == centerLine )
							{
								_ffb_pixels[ offset + 0 ] = 0;
								_ffb_pixels[ offset + 1 ] = 128;
								_ffb_pixels[ offset + 2 ] = 0;
							}
							else
							{
								_ffb_pixels[ offset + 0 ] = 64;
								_ffb_pixels[ offset + 1 ] = 32;
								_ffb_pixels[ offset + 2 ] = 32;
							}
						}
					}

					_ffb_prettyGraphCurrentX = ( _ffb_prettyGraphCurrentX + 1 ) % FFB_PIXELS_BUFFER_WIDTH;
				}

				// disable wheel output if we dont want to send the playback to the wheel

				if ( _ffb_playingBackNow && !Settings.PlaybackSendToDevice )
				{
					if ( processThisFrame )
					{
						_ffb_outputWheelMagnitudeBuffer[ outputWheelMagnitudeBufferIndex ] = 0;
					}
				}
			}

			// update the pretty graph

			if ( _ffb_drawPrettyGraph )
			{
				for ( var y = 0; y < 200; y++ )
				{
					var offset = y * FFB_PIXELS_BUFFER_STRIDE + _ffb_prettyGraphCurrentX * FFB_PIXELS_BUFFER_BYTES_PER_PIXEL;

					_ffb_pixels[ offset + 0 ] = 0;
					_ffb_pixels[ offset + 1 ] = 128;
					_ffb_pixels[ offset + 2 ] = 0;
					_ffb_pixels[ offset + 3 ] = 255;
				}
			}
		}

		static private void MultimediaTimerEventCallback( uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2 )
		{
			var app = (App) Current;

			// check stopwatch

			var deltaMilliseconds = (float) app._ffb_stopwatch.Elapsed.TotalMilliseconds;

			if ( deltaMilliseconds < 0.25f )
			{
				return;
			}

			app._ffb_stopwatch.Restart();

			var deltaSeconds = deltaMilliseconds / 1000f;

			// update the magnitude buffer timer

			app._ffb_outputWheelMagnitudeBufferTimer += deltaMilliseconds;

			// reset the magnitude timer when its time

			if ( Interlocked.Exchange( ref app._ffb_resetOutputWheelMagnitudeBufferTimerNow, 0 ) == 1 )
			{
				app._ffb_outputWheelMagnitudeBufferTimer = 0f;
			}

			// figure out where we are at in the magnitude buffer

			var outputWheelMagnitudeBufferIndex = 1f + ( app._ffb_outputWheelMagnitudeBufferTimer * 360f / 1000f );

			// get the current magnitude, cubic interpolated

			var maxOffset = app._ffb_outputWheelMagnitudeBuffer.Length - 1;

			var i1 = Math.Min( maxOffset, (int) MathF.Truncate( outputWheelMagnitudeBufferIndex ) );
			var i2 = Math.Min( maxOffset, i1 + 1 );
			var i3 = Math.Min( maxOffset, i2 + 1 );
			var i0 = Math.Max( 0, i1 - 1 );

			var t = Math.Min( 1f, outputWheelMagnitudeBufferIndex - i1 );

			var m0 = app._ffb_outputWheelMagnitudeBuffer[ i0 ];
			var m1 = app._ffb_outputWheelMagnitudeBuffer[ i1 ];
			var m2 = app._ffb_outputWheelMagnitudeBuffer[ i2 ];
			var m3 = app._ffb_outputWheelMagnitudeBuffer[ i3 ];

			var magnitude = (int) InterpolateHermite( m0, m1, m2, m3, t );

			// update send start timer

			app._ffb_sendStartTimer = Math.Max( 0f, app._ffb_sendStartTimer - deltaSeconds );

			// update clipped timer

			app._ffb_clippedTimer = Math.Max( 0f, app._ffb_clippedTimer - deltaSeconds );

			// light clip indicator if we are out of range

			if ( magnitude > DI_FFNOMINALMAX )
			{
				magnitude = DI_FFNOMINALMAX;

				app._ffb_clippedTimer = 3f;
			}
			else if ( magnitude < -DI_FFNOMINALMAX )
			{
				magnitude = -DI_FFNOMINALMAX;

				app._ffb_clippedTimer = 3f;
			}

			// apply cool down to prevent sore thumbs

			if ( app._ffb_startCooldownNow )
			{
				app._ffb_startCooldownNow = false;
				app._ffb_magnitudeCoolDownTimer = 1f;

				app.UpdateConstantForce( [ 0 ] );
			}

			if ( app._ffb_magnitudeCoolDownTimer > 0 )
			{
				app._ffb_magnitudeCoolDownTimer = Math.Max( 0, app._ffb_magnitudeCoolDownTimer - deltaSeconds );

				magnitude = (int) ( app._ffb_magnitudeCoolDownTimer * app._ffb_lastNonCooldownMagnitudeSentToWheel );
			}
			else
			{
				app._ffb_lastNonCooldownMagnitudeSentToWheel = magnitude;
			}

			// update last magnitude sent to wheel

			app._ffb_lastMagnitudeSentToWheel = magnitude;

			// update forces on the wheel

			if ( !app._ffb_forceFeedbackExceptionThrown )
			{
				// update the constant force effect

				if ( ( app._ffb_constantForceEffectParameters != null ) && ( app._ffb_constantForceEffect != null ) )
				{
					if ( app.Settings.InvertDirectInputMagnitude )
					{
						magnitude = -magnitude;
					}

					( (ConstantForce) app._ffb_constantForceEffectParameters.Parameters ).Magnitude = magnitude;

					try
					{
						var effectParameterFlags = EffectParameterFlags.TypeSpecificParameters;

						if ( app._ffb_sendStartTimer == 0 )
						{
							effectParameterFlags |= EffectParameterFlags.Start;

							app._ffb_sendStartTimer = 30;
						}
						else
						{
							effectParameterFlags |= EffectParameterFlags.NoRestart;
						}

						app._ffb_constantForceEffect.SetParameters( app._ffb_constantForceEffectParameters, effectParameterFlags );
					}
					catch ( Exception exception )
					{
						app._ffb_forceFeedbackExceptionThrown = true;
						app._ffb_reinitializeNeeded = true;

						app.WriteLine( "An exception was thrown while trying to update the constant force effect parameters!", true );
						app.WriteLine( exception.Message.Trim() );
					}
				}
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static float InterpolateHermite( float v0, float v1, float v2, float v3, float t )
		{
			var a = 2.0f * v1;
			var b = v2 - v0;
			var c = 2.0f * v0 - 5.0f * v1 + 4.0f * v2 - v3;
			var d = -v0 + 3.0f * v1 - 3.0f * v2 + v3;

			return 0.5f * ( a + ( b * t ) + ( c * t * t ) + ( d * t * t * t ) );
		}
	}
}
