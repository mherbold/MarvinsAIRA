
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

		public const int FFB_PIXELS_BUFFER_WIDTH = FFB_WRITEABLE_BITMAP_WIDTH;
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

		private float _ffb_autoTorqueNM = 0f;

		public readonly float[] _ffb_recordedTorqueNMBuffer = new float[ FFB_SAMPLES_PER_FRAME * IRSDK_TICK_RATE * 60 * 10 ];
		public int _ffb_recordedTorqueNMBufferIndex = 0;
		public bool _ffb_recordingNow = false;
		public bool _ffb_playingBackNow = false;

		private readonly int[] _ffb_outputDI = new int[ FFB_SAMPLES_PER_FRAME + 1 ];
		private float _ffb_outputDITimer = 0f;
		private int _ffb_resetOutputDITimerNow = 0;
		private int _ffb_lastMagnitudeSentToWheel = 0;

		private float _ffb_inTorqueNM = 0f;
		private float _ffb_outTorqueNM = 0f;
		private float _ffb_lfeInMagnitude = 0f;
		private float _ffb_lfeOutTorqueNM = 0f;

		private bool _ffb_startCooldownNow = false;
		private float _ffb_magnitudeCoolDownTimer = 0f;
		private int _ffb_lastNonCooldownMagnitudeSentToWheel = 0;

		private float _ffb_previousTorqueNM = 0f;
		private float _ffb_runningTorqueNM = 0f;
		private float _ffb_rawSteadyStateTorqueNM = 0f;
		private float _ffb_steadyStateTorqueNM = 0f;

		public bool _ffb_drawPrettyGraph = false;
		public int _ffb_prettyGraphCurrentX = 0;
		public readonly WriteableBitmap _ffb_writeableBitmap = new( FFB_WRITEABLE_BITMAP_WIDTH, FFB_WRITEABLE_BITMAP_HEIGHT, FFB_WRITEABLE_BITMAP_DPI, FFB_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null );
		public readonly byte[] _ffb_pixels = new byte[ FFB_PIXELS_BUFFER_STRIDE * FFB_PIXELS_BUFFER_HEIGHT ];

		private readonly Stopwatch _ffb_stopwatch = new();

		private readonly float[] _ffb_gForceBuffer = new float[ 120 ];
		private int _ffb_gForceBufferIndex = 0;
		private float _ffb_peakGForce = 0f;

		private readonly float[] _ffb_maxShockVelBuffer = new float[ 120 ];
		private int _ffb_maxShockVelBufferIndex = 0;
		private float _ffb_peakShockVel = 0f;
		private float _ffb_maxShockVel = 0f;

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

		private float _ffb_curbProtectionTimer = 0f;
		private float _ffb_curbProtectionScale = 1f;

		private int _ffb_centerWheelForce = 0;

		public bool FFB_Initialized { get => _ffb_initialized; }
		public float FFB_ClippedTimer { get => _ffb_clippedTimer; }
		public int FFB_LastMagnitudeSentToWheel { get => _ffb_lastMagnitudeSentToWheel; }
		public float FFB_PeakGForce { get => _ffb_peakGForce; }
		public float FFB_PeakShockVel { get => _ffb_peakShockVel; }
		public float FFB_YawRateFactorInstant { get => _ffb_yawRateFactorInstant; }
		public float FFB_YawRateFactorAverage { get => _ffb_yawRateFactorAverage; }
		public float FFB_UndersteerAmount { get => _ffb_understeerAmount; }
		public float FFB_UndersteerAmountLinear { get => _ffb_understeerAmountLinear; }
		public float FFB_OversteerAmount { get => _ffb_oversteerAmount; }
		public float FFB_OversteerAmountLinear { get => _ffb_oversteerAmountLinear; }
		public bool FFB_AutoOverallScaleIsReady { get => _ffb_autoTorqueNM > 1f; }
		public float FFB_AutoOverallScalePeakForceInNewtonMeters { get => _ffb_autoTorqueNM; }
		public bool FFB_IsCoolingDown { get => _ffb_startCooldownNow || _ffb_magnitudeCoolDownTimer > 0f; }

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

			DeviceType[] deviceTypeArray = [ DeviceType.Keyboard, DeviceType.Joystick, DeviceType.Gamepad, DeviceType.Driving, DeviceType.Flight, DeviceType.FirstPerson, DeviceType.ControlDevice, DeviceType.ScreenPointer, DeviceType.Remote, DeviceType.Supplemental ];

			if ( Settings.SelectedFFBDeviceGuid == Guid.Empty )
			{
				WriteLine( "...there is not already a selected force feedback device, looking for the first attached force feedback driving device..." );

				foreach ( var deviceType in deviceTypeArray )
				{
					WriteLine( $"...scanning for {deviceType} devices..." );

					bool deviceFound = false;

					var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.AttachedOnly );

					foreach ( var joystickDeviceInstance in deviceInstanceList )
					{
						var hasForceFeedback = joystickDeviceInstance.ForceFeedbackDriverGuid != Guid.Empty;

						if ( hasForceFeedback )
						{
							Settings.SelectedFFBDeviceGuid = joystickDeviceInstance.InstanceGuid;
							forceFeedbackDeviceInstance = joystickDeviceInstance;
							deviceFound = true;
							break;
						}
					}

					if ( deviceFound )
					{
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

					var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.AttachedOnly );

					foreach ( var joystickDeviceInstance in deviceInstanceList )
					{
						if ( joystickDeviceInstance.InstanceGuid == Settings.SelectedFFBDeviceGuid )
						{
							forceFeedbackDeviceInstance = joystickDeviceInstance;
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
				WriteLine( "...killing the FFB multimedia timer event..." );

				WinApi.TimeKillEvent( _ffb_multimediaTimerId );

				_ffb_multimediaTimerId = 0;

				WriteLine( "...FFB multimedia timer event killed..." );
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

				WriteLine( $"...starting the FFB multimedia timer event (period = {periodInMilliseconds} ms)..." );

				_ffb_sendStartTimer = 0;
				_ffb_forceFeedbackExceptionThrown = false;

				_ffb_multimediaTimerId = WinApi.TimeSetEvent( periodInMilliseconds, 0, FFBMultimediaTimerEventCallback, ref userCtx, EVENTTYPE_PERIODIC );

				WriteLine( "...the FFB multimedia timer event has been started..." );
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

		public void ResetAutoOverallScaleMetrics()
		{
			_ffb_autoTorqueNM = 0f;
		}

		public void DoAutoOverallScaleNow()
		{
			Settings.OverallScale = 100f * Settings.WheelMaxForce / _ffb_autoTorqueNM;

			if ( Settings.OverallScale < 10f )
			{
				Say( Settings.SayOverallScale, $"{Settings.OverallScale:F1}" );
			}
			else
			{
				Say( Settings.SayOverallScale, $"{Settings.OverallScale:F0}" );
			}

			ResetAutoOverallScaleMetrics();
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

					if ( FFB_AutoOverallScaleIsReady )
					{
						DoAutoOverallScaleNow();
					}
				}

				buttonPresses = Settings.ClearAutoOverallScaleButtons.ClickCount;

				if ( buttonPresses > 0 )
				{
					WriteLine( "CLEAR-AUTO-OVERALL-SCALE button pressed!", true );

					if ( FFB_AutoOverallScaleIsReady )
					{
						ResetAutoOverallScaleMetrics();
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

				_ffb_reinitializeTimer = MathF.Max( 1f, _ffb_reinitializeTimer );
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

					Settings.OSSoftness = 90f;
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

							var leftDelta = (float) -MathF.Min( 0f, currentWheelPosition - Settings.WheelCenterValue );
							var rightDelta = (float) MathF.Max( 0f, currentWheelPosition - Settings.WheelCenterValue );

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
			for ( var i = 0; i < _ffb_outputDI.Length; i++ )
			{
				var value = forceMagnitudeList[ i % forceMagnitudeList.Length ];

				_ffb_outputDI[ i ] = value;
			}

			_ffb_updatesToSkip = 6;
			_ffb_resetOutputDITimerNow = 1;
			_ffb_previousTorqueNM = 0f;
			_ffb_runningTorqueNM = 0f;
			_ffb_rawSteadyStateTorqueNM = 0f;
			_ffb_steadyStateTorqueNM = 0f;
		}

		private void UFF_ProcessYawRateFactor()
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

			// keep track of average yaw rate factor over the last two seconds (for skid pad)

			_ffb_yawRateFactorBuffer[ _ffb_yawRateFactorBufferIndex ] = _ffb_yawRateFactorInstant;

			_ffb_yawRateFactorBufferIndex = ( _ffb_yawRateFactorBufferIndex + 1 ) % _ffb_yawRateFactorBuffer.Length;

			var totalYawRateFactor = 0f;

			for ( var i = 0; i < _ffb_yawRateFactorBuffer.Length; i++ )
			{
				totalYawRateFactor += _ffb_yawRateFactorBuffer[ i ];
			}

			_ffb_yawRateFactorAverage = totalYawRateFactor / _ffb_yawRateFactorBuffer.Length;
		}

		private void UFF_ProcessGForce()
		{
			// keep track of peak g force over the last two seconds (for skid pad)

			_ffb_gForceBuffer[ _ffb_gForceBufferIndex ] = _irsdk_gForce;

			_ffb_gForceBufferIndex = ( _ffb_gForceBufferIndex + 1 ) % _ffb_gForceBuffer.Length;

			var peakGForce = 0f;

			for ( var i = 0; i < _ffb_gForceBuffer.Length; i++ )
			{
				peakGForce = MathF.Max( peakGForce, _ffb_gForceBuffer[ i ] );
			}

			_ffb_peakGForce = peakGForce;
		}

		private void UFF_ProcessShocks()
		{
			// calculate the max shock velocity for this frame

			var maxShockVel = 0f;

			for ( var i = 0; i < IRSDK_360HZ_SAMPLES_PER_FRAME; i++ )
			{
				maxShockVel = MathF.Max( maxShockVel, MathF.Abs( _irsdk_cfShockVel_ST[ i ] ) );
				maxShockVel = MathF.Max( maxShockVel, MathF.Abs( _irsdk_crShockVel_ST[ i ] ) );
				maxShockVel = MathF.Max( maxShockVel, MathF.Abs( _irsdk_lfShockVel_ST[ i ] ) );
				maxShockVel = MathF.Max( maxShockVel, MathF.Abs( _irsdk_lrShockVel_ST[ i ] ) );
				maxShockVel = MathF.Max( maxShockVel, MathF.Abs( _irsdk_rfShockVel_ST[ i ] ) );
				maxShockVel = MathF.Max( maxShockVel, MathF.Abs( _irsdk_rrShockVel_ST[ i ] ) );
			}

			_ffb_maxShockVel = maxShockVel;

			// keep track of peak shock velocity over the last two seconds (for skid pad)

			_ffb_maxShockVelBuffer[ _ffb_maxShockVelBufferIndex ] = maxShockVel;

			_ffb_maxShockVelBufferIndex = ( _ffb_maxShockVelBufferIndex + 1 ) % _ffb_maxShockVelBuffer.Length;

			var peakShockVel = 0f;

			for ( var i = 0; i < _ffb_maxShockVelBuffer.Length; i++ )
			{
				peakShockVel = MathF.Max( peakShockVel, _ffb_maxShockVelBuffer[ i ] );
			}

			_ffb_peakShockVel = peakShockVel;
		}

		private void UpdateForceFeedback()
		{
			UFF_ProcessYawRateFactor();
			UFF_ProcessGForce();
			UFF_ProcessShocks();

			// calculate understeer amount (in -1 to +1) and frequency (in rads per 1/360sec)

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

				understeerFrequency = MathF.Max( 0.25f, _ffb_understeerAmountLinear );

				_ffb_understeerAmount = MathF.Sign( _irsdk_steeringWheelAngle ) * MathF.Pow( _ffb_understeerAmountLinear, Settings.USCurve );
			}
			else
			{
				_ffb_understeerAmount = 0f;
				_ffb_understeerAmountLinear = 0f;
			}

			// calculate oversteer amount (in -1 to +1) and frequency (in rads per 1/360sec)

			var oversteerFrequency = 0f;

			if ( _irsdk_velocityX > 0f )
			{
				_ffb_oversteerAmountLinear = Math.Clamp( ( MathF.Abs( _irsdk_velocityY ) - Settings.OSStartYVelocity ) / ( Settings.OSEndYVelocity - Settings.OSStartYVelocity ), 0f, 1f );

				oversteerFrequency = MathF.Max( 0.25f, _ffb_oversteerAmountLinear );

				_ffb_oversteerAmount = -MathF.Sign( _irsdk_velocityY ) * MathF.Pow( _ffb_oversteerAmountLinear, Settings.OSCurve );

				// apply softness if counter-steering

				if ( MathF.Sign( _irsdk_steeringWheelAngle ) == MathF.Sign( _irsdk_velocityY ) )
				{
					if ( ( Settings.OSSoftness >= 1f ) && ( Settings.OSSoftness <= 179f ) )
					{
						var fadeDistanceInRadians = Settings.OSSoftness * MathF.PI / 180f;

						var fadeAmount = Math.Clamp( ( fadeDistanceInRadians - MathF.Abs( _irsdk_steeringWheelAngle ) ) / fadeDistanceInRadians, 0f, 1f );

						_ffb_oversteerAmount *= fadeAmount;
					}
				}
			}
			else
			{
				_ffb_oversteerAmount = 0f;
				_ffb_oversteerAmountLinear = 0f;
			}

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

			// if we just got off track we want to enable cool down mode (cooldown is actually handled in the multimedia thread)

			if ( _irsdk_isOnTrackLastFrame && !_irsdk_isOnTrack )
			{
				_ffb_startCooldownNow = true;
			}

			// get the FFB input from the telemetry data (in N*M)

			float[] ffbInputNM = [
				_irsdk_steeringWheelTorque_ST[ 0 ],
				_irsdk_steeringWheelTorque_ST[ 1 ],
				_irsdk_steeringWheelTorque_ST[ 2 ],
				_irsdk_steeringWheelTorque_ST[ 3 ],
				_irsdk_steeringWheelTorque_ST[ 4 ],
				_irsdk_steeringWheelTorque_ST[ 5 ]
			];

			// override ffb input with playback values if we are playing back a recording (in N*M)

			if ( _ffb_playingBackNow )
			{
				for ( var x = 0; x < ffbInputNM.Length; x++ )
				{
					ffbInputNM[ x ] = _ffb_recordedTorqueNMBuffer[ _ffb_recordedTorqueNMBufferIndex ];

					_ffb_recordedTorqueNMBufferIndex = ( _ffb_recordedTorqueNMBufferIndex + 1 ) % _ffb_recordedTorqueNMBuffer.Length;
				}
			}

			// if we are not on track or the iracing simulator is in replay mode then zero out ffb input forces (in N*M)

			if ( !_irsdk_isOnTrack || ( _irsdk_simMode == "replay" ) )
			{
				ffbInputNM[ 0 ] = 0f;
				ffbInputNM[ 1 ] = 0f;
				ffbInputNM[ 2 ] = 0f;
				ffbInputNM[ 3 ] = 0f;
				ffbInputNM[ 4 ] = 0f;
				ffbInputNM[ 5 ] = 0f;
			}

			// calculate soft lock magnitude (in N*M)

			var softLockTorqueNM = 0f;

			if ( Settings.EnableSoftLock )
			{
				var softLockMarginInRadians = Settings.SoftLockMargin * MathF.PI / 180f;

				var softLockPercentage = Math.Clamp( ( MathF.Abs( _irsdk_steeringWheelAngle ) - ( _irsdk_steeringWheelAngleMax * .5f - softLockMarginInRadians ) ) / softLockMarginInRadians, 0f, 1f );

				softLockTorqueNM = -MathF.Sign( _irsdk_steeringWheelAngle ) * softLockPercentage * Settings.WheelMaxForce * Settings.SoftLockStrength / 100f;
			}

			// calculate crash protection scale (in 0-1)

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

				_ffb_crashProtectionTimer = MathF.Max( 0f, _ffb_crashProtectionTimer - ( 1f / _irsdk_tickRate ) );

				if ( _ffb_crashProtectionTimer == 0f )
				{
					Say( Settings.SayCrashProtectionOff );
				}
			}
			else
			{
				_ffb_crashProtectionScale = ( _ffb_crashProtectionScale * 0.99f ) + ( 1f * 0.01f );
			}

			// calculate curb protection scale (in 0-1)

			if ( Settings.EnableCurbProtection )
			{
				if ( MathF.Abs( _ffb_maxShockVel ) >= Settings.ShockVelocity )
				{
					_ffb_curbProtectionTimer = Settings.CurbProtectionDuration;
				}
			}

			if ( _ffb_curbProtectionTimer > 0f )
			{
				_ffb_curbProtectionScale = 1f - Settings.CurbProtectionDetailScale / 100f;

				_ffb_curbProtectionTimer = MathF.Max( 0f, _ffb_curbProtectionTimer - ( 1f / _irsdk_tickRate ) );
			}
			else
			{
				_ffb_curbProtectionScale = ( _ffb_curbProtectionScale * 0.99f ) + ( 1f * 0.01f );
			}

			// calculate speed scale (reduce forces while the car is moving very slow or parked) (in 0-1)

			var speedScale = 1f;

			if ( !_ffb_playingBackNow && ( _irsdk_speed < 5f ) )
			{
				var t = _irsdk_speed / 5f;

				speedScale = ( Settings.ParkedScale / 100f ) * ( 1f - t ) + t;
			}

			// normalize the scale settings ("100%" slider value = 1)

			var overallScale = Settings.OverallScale / 100f;
			var detailScale = Settings.DetailScale / 100f;
			var lfeScale = Settings.LFEScale / 100f;
			var autoOverallScaleClipLimit = Settings.AutoOverallScaleClipLimit / 100f;

			// calculate steering effect strength (in N*M)

			var understeerEffectStrengthNM = Settings.WheelMaxForce * Settings.USEffectStrength / 100f;
			var oversteerEffectStrengthNM = Settings.WheelMaxForce * Settings.OSEffectStrength / 100f;

			// apply crash protection to overall scale

			overallScale = ( overallScale * ( 1f - Settings.CrashProtectionOverallScale / 100f ) * ( 1f - _ffb_crashProtectionScale ) ) + ( overallScale * _ffb_crashProtectionScale );

			// apply crash and curb protection to detail scale

			detailScale *= _ffb_crashProtectionScale * _ffb_curbProtectionScale;

			// make a copy of the lfe read index so it doesn't change in the middle of this update (its updated in the lfe thread)

			var lfeMagnitudeIndex = _lfe_magnitudeIndex;

			// copy the last magnitude sample from the last frame into the first frame to make interpolation even better

			if ( processThisFrame )
			{
				_ffb_outputDI[ 0 ] = _ffb_outputDI[ 6 ];
			}

			// for telemetry

			var inTorqueNMAbs = 0f;
			var inTorqueNM = 0f;
			var outTorqueNMAbs = 0f;
			var outTorqueNM = 0f;

			var lfeInMagnitudeAbs = 0f;
			var lfeInMagnitude = 0f;
			var lfeOutTorqueNMAbs = 0f;
			var lfeOutTorqueNM = 0f;

			// go through each sample

			for ( var x = 0; x < ffbInputNM.Length; x++ )
			{
				// get the next steering wheel torque sample

				var torqueNM = ffbInputNM[ x ];

				// for telemetry

				var torqueNMAbs = MathF.Abs( torqueNM );

				if ( torqueNMAbs > inTorqueNMAbs )
				{
					inTorqueNMAbs = torqueNMAbs;
					inTorqueNM = torqueNM;
				}

				// save the torque (for playback feature)

				if ( _ffb_recordingNow )
				{
					_ffb_recordedTorqueNMBuffer[ _ffb_recordedTorqueNMBufferIndex ] = torqueNM;

					_ffb_recordedTorqueNMBufferIndex = ( _ffb_recordedTorqueNMBufferIndex + 1 ) % _ffb_recordedTorqueNMBuffer.Length;
				}

				// calculate the impulse (change in steering wheel torque compared to the last sample)

				var deltaTorqueNM = torqueNM - _ffb_previousTorqueNM;

				_ffb_previousTorqueNM = torqueNM;

				// delta limiter

				var limitedDeltaTorqueNM = Math.Clamp( deltaTorqueNM, -0.09f, 0.09f ); // TODO make this user customizable

				// calculate raw steady state wheel torque

				var rawSteadyStateTorqueNM = _ffb_rawSteadyStateTorqueNM + limitedDeltaTorqueNM;

				_ffb_rawSteadyStateTorqueNM = ( rawSteadyStateTorqueNM * 0.9f ) + ( torqueNM * 0.1f ); // TODO make this user customizable

				// calculate torque for the auto overall scale feature

				var autoTorqueNM = torqueNM * ( 1f - autoOverallScaleClipLimit ) + ( _ffb_rawSteadyStateTorqueNM * autoOverallScaleClipLimit );

				// calculate scaled steady state wheel torque

				_ffb_steadyStateTorqueNM += limitedDeltaTorqueNM * overallScale;

				_ffb_steadyStateTorqueNM = ( _ffb_steadyStateTorqueNM * 0.9f ) + ( torqueNM * overallScale * 0.1f ); // TODO make this user customizable

				// algorithm is different for detail scale >= 100% and < 100%

				if ( detailScale >= 1f )
				{
					// scale the impulse by our detail scale and add it to our running steering wheel torque

					_ffb_runningTorqueNM += deltaTorqueNM * overallScale * detailScale;

					// ramp our running scaled magnitude towards the original signal (feed in steady state signal)

					_ffb_runningTorqueNM = ( _ffb_runningTorqueNM * 0.9f ) + ( torqueNM * overallScale * 0.1f ); // TODO make this user customizable
				}
				else
				{
					// blend between steady state force and original force using detail scale amount

					_ffb_runningTorqueNM = ( torqueNM * overallScale * detailScale ) + ( _ffb_steadyStateTorqueNM * ( 1f - detailScale ) );
				}

				// our initial output torque

				var ffbOutputNM = _ffb_runningTorqueNM;

				// apply the speed scale

				ffbOutputNM *= speedScale;
				autoTorqueNM *= speedScale;

				// mix in the low frequency effects

				if ( Settings.LFEToFFBEnabled )
				{
					var lfeMagnitude = _lfe_magnitude[ lfeMagnitudeIndex, x ];

					var lfeMagnitudeAbs = Math.Abs( lfeMagnitude );

					if ( lfeMagnitudeAbs > lfeInMagnitudeAbs )
					{
						lfeInMagnitudeAbs = lfeMagnitudeAbs;
						lfeInMagnitude = lfeMagnitude;
					}

					var lfeTorqueNM = lfeMagnitude * lfeScale * Settings.WheelMaxForce;

					var lfeTorqueNMAbs = Math.Abs( lfeTorqueNM );

					if ( lfeTorqueNMAbs > lfeOutTorqueNMAbs )
					{
						lfeOutTorqueNMAbs = lfeTorqueNMAbs;
						lfeOutTorqueNM = lfeTorqueNM;
					}

					ffbOutputNM += lfeTorqueNM;
				}

				// add in the steering wheel stops magnitude

				ffbOutputNM += softLockTorqueNM;

				// mix in the understeer and oversteer effects

				if ( Settings.SteeringEffectsEnabled )
				{
					// understeer effects

					_ffb_understeerEffectWaveAngle += understeerFrequency;

					if ( _ffb_understeerEffectWaveAngle > MathF.PI * 2f )
					{
						_ffb_understeerEffectWaveAngle -= MathF.PI * 2f;
					}

					if ( Settings.USEffectStyle == 0 )
					{
						var waveAmplitude = MathF.Sin( _ffb_understeerEffectWaveAngle );

						ffbOutputNM += waveAmplitude * _ffb_understeerAmount * understeerEffectStrengthNM;
					}
					else if ( Settings.USEffectStyle == 1 )
					{
						var waveAmplitude = _ffb_understeerEffectWaveAngle / ( MathF.PI * 2f );

						ffbOutputNM -= waveAmplitude * _ffb_understeerAmount * understeerEffectStrengthNM;
					}
					else if ( Settings.USEffectStyle == 2 )
					{
						ffbOutputNM -= _ffb_steadyStateTorqueNM * MathF.Abs( _ffb_understeerAmount ) * Settings.USEffectStrength / 100f;
					}
					else if ( Settings.USEffectStyle == 3 )
					{
						ffbOutputNM -= _ffb_understeerAmount * understeerEffectStrengthNM;
					}

					// oversteer effects

					_ffb_oversteerEffectWaveAngle += oversteerFrequency;

					if ( _ffb_oversteerEffectWaveAngle > MathF.PI * 2f )
					{
						_ffb_oversteerEffectWaveAngle -= MathF.PI * 2f;
					}

					if ( Settings.OSEffectStyle == 0 )
					{
						var waveAmplitude = MathF.Sin( _ffb_oversteerEffectWaveAngle );

						ffbOutputNM += waveAmplitude * _ffb_oversteerAmount * oversteerEffectStrengthNM;
					}
					else if ( Settings.OSEffectStyle == 1 )
					{
						var waveAmplitude = _ffb_oversteerEffectWaveAngle / ( MathF.PI * 2f );

						ffbOutputNM -= waveAmplitude * _ffb_oversteerAmount * oversteerEffectStrengthNM;
					}
					else if ( Settings.OSEffectStyle == 2 )
					{
						ffbOutputNM -= _ffb_steadyStateTorqueNM * MathF.Abs( _ffb_oversteerAmount ) * Settings.OSEffectStrength / 100f;
					}
					else if ( Settings.OSEffectStyle == 3 )
					{
						ffbOutputNM -= _ffb_oversteerAmount * oversteerEffectStrengthNM;
					}
				}

				// apply FFB minimum force and curve

				var ffbOutputNMAbs = MathF.Abs( ffbOutputNM );

				if ( ffbOutputNMAbs < Settings.MinForce )
				{
					ffbOutputNMAbs = Settings.MinForce;
				}

				var curveScale = 1f / ( Settings.WheelMaxForce - Settings.MinForce );

				ffbOutputNM = MathF.Sign( ffbOutputNM ) * ( MathF.Min( Settings.MaxForce, MathF.Pow( ( ffbOutputNMAbs - Settings.MinForce ) * curveScale, Settings.FFBCurve ) / curveScale + Settings.MinForce ) );

				// finally convert from NM to DI units

				if ( processThisFrame )
				{
					if ( _ffb_playingBackNow && !Settings.PlaybackSendToDevice )
					{
						_ffb_outputDI[ x + 1 ] = 0;
					}
					else
					{
						_ffb_outputDI[ x + 1 ] = (int) ( ffbOutputNM * DI_FFNOMINALMAX / Settings.WheelMaxForce );
					}
				}

				// reset the magnitude index now

				_ffb_resetOutputDITimerNow = 1;

				// save the original steering wheel torque (for auto-overall-scale feature)

				if ( _ffb_playingBackNow || ( _irsdk_isOnTrack && !_irsdk_onPitRoad && ( _irsdk_playerTrackSurface != IRSDKSharper.IRacingSdkEnum.TrkLoc.OffTrack ) ) )
				{
					autoTorqueNM = MathF.Abs( autoTorqueNM );

					if ( autoTorqueNM > 0f )
					{
						if ( _ffb_autoTorqueNM < autoTorqueNM )
						{
							_ffb_autoTorqueNM = autoTorqueNM;
						}
					}
				}

				// update the pretty graph

				if ( _ffb_drawPrettyGraph )
				{
					var torqueToPixelBufferScale = Settings.WheelMaxForce * 2f / ( FFB_PIXELS_BUFFER_HEIGHT - 40f );
					var halfPixelBufferHeight = FFB_PIXELS_BUFFER_HEIGHT / 2f;

					var centerLine = FFB_PIXELS_BUFFER_HEIGHT / 2;

					var midPoint1 = (int) ( ( Settings.WheelMaxForce / 2f ) / torqueToPixelBufferScale + halfPixelBufferHeight );
					var midPoint2 = (int) ( ( Settings.WheelMaxForce / -2f ) / torqueToPixelBufferScale + halfPixelBufferHeight );

					var clipPoint1 = 19;
					var clipPoint2 = FFB_PIXELS_BUFFER_HEIGHT - 20;

					var shockProtection1 = clipPoint1 + 1;
					var shockProtection2 = clipPoint1 + 2;
					var shockProtection3 = clipPoint2 - 1;
					var shockProtection4 = clipPoint2 - 2;

					var iY2 = (int) ( torqueNM * overallScale / torqueToPixelBufferScale + halfPixelBufferHeight ) + 1;
					var iY1 = iY2 - 2;

					var oY2 = (int) ( ffbOutputNM / torqueToPixelBufferScale + halfPixelBufferHeight ) + 1;
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
							else if ( ( _ffb_crashProtectionTimer > 0f ) && ( ( y == shockProtection1 ) || ( y == shockProtection2 ) || ( y == shockProtection3 ) || ( y == shockProtection4 ) ) )
							{
								_ffb_pixels[ offset + 0 ] = 0;
								_ffb_pixels[ offset + 1 ] = 255;
								_ffb_pixels[ offset + 2 ] = 255;
							}
							else if ( ( _ffb_curbProtectionTimer > 0f ) && ( ( y == shockProtection1 ) || ( y == shockProtection2 ) || ( y == shockProtection3 ) || ( y == shockProtection4 ) ) )
							{
								if ( ( _irsdk_tickCount & 1 ) == 0 )
								{
									_ffb_pixels[ offset + 0 ] = 0;
									_ffb_pixels[ offset + 1 ] = 0;
									_ffb_pixels[ offset + 2 ] = 255;
								}
								else
								{
									_ffb_pixels[ offset + 0 ] = 255;
									_ffb_pixels[ offset + 1 ] = 255;
									_ffb_pixels[ offset + 2 ] = 255;
								}
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

				// for telemetry

				ffbOutputNMAbs = Math.Abs( ffbOutputNM );

				if ( ffbOutputNMAbs > outTorqueNMAbs )
				{
					outTorqueNMAbs = ffbOutputNMAbs;
					outTorqueNM = ffbOutputNM;
				}
			}

			// for telemetry

			_ffb_inTorqueNM = inTorqueNM;
			_ffb_outTorqueNM = outTorqueNM;
			_ffb_lfeInMagnitude = lfeInMagnitude;
			_ffb_lfeOutTorqueNM = lfeOutTorqueNM;
		}

		private static void FFBMultimediaTimerEventCallback( uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2 )
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

			app._ffb_outputDITimer += deltaMilliseconds;

			// reset the magnitude timer when its time

			if ( Interlocked.Exchange( ref app._ffb_resetOutputDITimerNow, 0 ) == 1 )
			{
				app._ffb_outputDITimer = 0f;
			}

			// figure out where we are at in the magnitude buffer

			var ffbOutputDIIndex = 1f + ( app._ffb_outputDITimer * 360f / 1000f );

			// get the current magnitude, cubic interpolated

			var maxOffset = app._ffb_outputDI.Length - 1;

			var i1 = Math.Min( maxOffset, (int) MathF.Truncate( ffbOutputDIIndex ) );
			var i2 = Math.Min( maxOffset, i1 + 1 );
			var i3 = Math.Min( maxOffset, i2 + 1 );
			var i0 = Math.Max( 0, i1 - 1 );

			var t = MathF.Min( 1f, ffbOutputDIIndex - i1 );

			var m0 = app._ffb_outputDI[ i0 ];
			var m1 = app._ffb_outputDI[ i1 ];
			var m2 = app._ffb_outputDI[ i2 ];
			var m3 = app._ffb_outputDI[ i3 ];

			var magnitude = (int) InterpolateHermite( m0, m1, m2, m3, t );

			// update send start timer

			app._ffb_sendStartTimer = MathF.Max( 0f, app._ffb_sendStartTimer - deltaSeconds );

			// update clipped timer

			app._ffb_clippedTimer = MathF.Max( 0f, app._ffb_clippedTimer - deltaSeconds );

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

			if ( app._ffb_magnitudeCoolDownTimer > 0f )
			{
				app._ffb_magnitudeCoolDownTimer = MathF.Max( 0f, app._ffb_magnitudeCoolDownTimer - deltaSeconds );

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
