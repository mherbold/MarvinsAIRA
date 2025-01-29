
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SharpDX.DirectInput;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		[DllImport( "user32.dll" )]
		private static extern IntPtr GetForegroundWindow();

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool SetForegroundWindow( IntPtr hWnd );

		private delegate void MultimediaTimerCallback( UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2 );

		[DllImport( "winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent" )]
		private static extern UInt32 TimeSetEvent( UInt32 msDelay, UInt32 msResolution, MultimediaTimerCallback callback, ref UInt32 userCtx, UInt32 eventType );

		[DllImport( "winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent" )]
		private static extern void TimeKillEvent( UInt32 uTimerId );

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

		private const int FFB_WRITEABLE_BITMAP_WIDTH = 768;
		private const int FFB_WRITEABLE_BITMAP_HEIGHT = 200;
		private const int FFB_WRITEABLE_BITMAP_DPI = 96;

		private const int FFB_PIXELS_BUFFER_WIDTH = FFB_WRITEABLE_BITMAP_WIDTH + 1;
		private const int FFB_PIXELS_BUFFER_HEIGHT = 200;
		private const int FFB_PIXELS_BUFFER_BYTES_PER_PIXEL = 4;
		private const int FFB_PIXELS_BUFFER_STRIDE = FFB_PIXELS_BUFFER_WIDTH * FFB_PIXELS_BUFFER_BYTES_PER_PIXEL;

		private const string ALL_WHEELS_SAVE_NAME = "All";

		private Joystick? _ffb_drivingJoystick = null;
		private EffectInfo? _ffb_constantForceEffectInfo = null;
		private EffectParameters? _ffb_effectParameters = null;
		private Effect? _ffb_constantForceEffect = null;

		private bool _ffb_initialized = false;
		private int _ffb_updatesToSkip = 0;
		private bool _ffb_reinitializeNeeded = false;
		private float _ffb_reinitializeTimer = 0;
		private bool _ffb_forceFeedbackExceptionThrown = false;
		private UInt32 _ffb_multimediaTimerId = 0;

		private bool _ffb_wheelChanged = false;
		private string _ffb_wheelSaveName = ALL_WHEELS_SAVE_NAME;

		private float _ffb_clippedTimer = 0;

		private readonly float[] _ffb_steeringWheelTorque = new float[ FFB_SAMPLES_PER_FRAME * IRSDK_TICK_RATE * 10 ];
		private int _ffb_steeringWheelTorqueIndex = 0;

		private readonly int[] _ffb_magnitude = new int[ FFB_SAMPLES_PER_FRAME ];
		private float _ffb_magnitudeMilliseconds = 0;
		private int _ffb_resetMagnitudeMilliseconds = 0;

		private float _ffb_previousSteeringWheelTorque = 0;
		private float _ffb_scaledSteeringWheelTorque = 0;

		private float _ffb_announceOverallScaleTimer = 0;
		private float _ffb_announceDetailScaleTimer = 0;
		private float _ffb_announceLFEScaleTimer = 0;

		private bool _ffb_drawPrettyGraph = false;
		private int _ffb_prettyGraphCurrentX = 0;
		private readonly WriteableBitmap _ffb_writeableBitmap = new( FFB_WRITEABLE_BITMAP_WIDTH, FFB_WRITEABLE_BITMAP_HEIGHT, FFB_WRITEABLE_BITMAP_DPI, FFB_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null );
		private readonly byte[] _ffb_pixels = new byte[ FFB_PIXELS_BUFFER_STRIDE * FFB_PIXELS_BUFFER_HEIGHT ];

		private Stopwatch _ffb_stopwatch = new Stopwatch();

		public bool FFB_Initialized { get => _ffb_initialized; }
		public float FFB_ClippedTimer { get => _ffb_clippedTimer; }
		public int FFB_CurrentMagnitude { get => _ffb_magnitude[ 0 ]; }

		class TestData
		{
			public float deltaMilliseconds;
			public float magnitudeIndex;
			public int m0;
			public int m1;
			public int m2;
			public int m3;
			public int magnitude;
		}

		static int testCounter = 0;
		static TestData[] testData = new TestData[ 1000 ];

		public void InitializeForceFeedback( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "InitializeForceFeedback called." );

			var mainWindow = (MainWindow) MainWindow;

			if ( mainWindow != null )
			{
				mainWindow.WheelForceFeedbackImage.Source = _ffb_writeableBitmap;
			}

			if ( _ffb_initialized )
			{
				UninitializeForceFeedback();
			}

			WriteLine( "...initializing DirectInput (FF driving device)..." );

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
						break;
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
					_ffb_wheelChanged = true;

					UpdateWheelSaveName();

					ReinitializeForceFeedbackDevice( windowHandle );
				}
			}
			else
			{
				WriteLine( "...no force feedback driving device was selected!" );
			}
		}

		private void UninitializeForceFeedback()
		{
			WriteLine( "" );
			WriteLine( "UninitializeForceFeedback called." );

			if ( _ffb_constantForceEffect != null )
			{
				WriteLine( "...disposing of the constant force effect..." );

				_ffb_constantForceEffect.Dispose();

				_ffb_constantForceEffect = null;

				WriteLine( "...the constant force effect has been disposed of..." );
			}

			if ( _ffb_constantForceEffectInfo != null )
			{
				_ffb_constantForceEffectInfo = null;
			}

			if ( _ffb_drivingJoystick != null )
			{
				WriteLine( "...unacquiring the force feedback device..." );

				_ffb_drivingJoystick.Unacquire();

				WriteLine( "...the force feedback device has been unacquired..." );
				WriteLine( "...disposing of the force feedback device..." );

				_ffb_drivingJoystick.Dispose();

				_ffb_drivingJoystick = null;

				WriteLine( "...the force feedback device has been disposed of..." );
			}

			if ( _ffb_multimediaTimerId != 0 )
			{
				WriteLine( "...killing the multimedia timer event..." );

				TimeKillEvent( _ffb_multimediaTimerId );

				_ffb_multimediaTimerId = 0;

				WriteLine( "...multimedia timer event killed..." );
			}

			WriteLine( "...force feedback uninitialized." );

			_ffb_initialized = false;
		}

		public void ReinitializeForceFeedbackDevice( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "ReinitializeForceFeedbackDevice called." );

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
				if ( _ffb_multimediaTimerId != 0 )
				{
					WriteLine( "...killing the multimedia timer event..." );

					TimeKillEvent( _ffb_multimediaTimerId );

					WriteLine( "...the multimedia timer event killed..." );
				}

				if ( _ffb_constantForceEffect != null )
				{
					WriteLine( "...disposing of the old constant force effect..." );

					_ffb_constantForceEffect.Dispose();

					_ffb_constantForceEffect = null;

					WriteLine( "...the old constant force effect has been disposed of..." );
				}

				WriteLine( "...unacquiring the force feedback device..." );

				_ffb_drivingJoystick.Unacquire();

				WriteLine( "...the force feedback device has been unacquired..." );
				WriteLine( "...setting the cooperative level (exclusive background) on the force feedback device..." );

				_ffb_drivingJoystick.SetCooperativeLevel( windowHandle, CooperativeLevel.Exclusive | CooperativeLevel.Background );

				WriteLine( "...the cooperative level has been set..." );
				WriteLine( "...acquiring the force feedback device..." );

				_ffb_drivingJoystick.Acquire();

				WriteLine( "...the force feedback device has been acquired..." );
				WriteLine( "...creating the constant force effect..." );

				_ffb_effectParameters = new EffectParameters
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

				_ffb_constantForceEffect = new Effect( _ffb_drivingJoystick, _ffb_constantForceEffectInfo.Guid, _ffb_effectParameters );

				WriteLine( "...the constant force effect has been created..." );
				WriteLine( "...starting the constant force effect..." );

				_ffb_constantForceEffect.Start();

				WriteLine( "...the constant force effect has been started..." );
				WriteLine( "...starting the multimedia timer event..." );

				UInt32 userCtx = 0;

				var periodInMilliseconds = (UInt32) ( 18 - Settings.Frequency );

				_ffb_multimediaTimerId = TimeSetEvent( periodInMilliseconds, 0, MultimediaTimerEventCallback, ref userCtx, EVENTTYPE_PERIODIC );

				WriteLine( "...the multimedia timer event has been started..." );
				WriteLine( "...the force feedback device has been reinitialized." );

				_ffb_forceFeedbackExceptionThrown = false;
			}
			catch ( Exception exception )
			{
				WriteLine( "...failed to reacquire the force feedback device:" );
				WriteLine( exception.Message.Trim() );

				_ffb_forceFeedbackExceptionThrown = true;
			}
		}

		public void StopForceFeedback()
		{
			UpdateConstantForce( [ 0, 0, 0, 0, 0, 0 ] );

			UninitializeForceFeedback();
		}

		public void ScheduleReinitializeForceFeedback()
		{
			_ffb_reinitializeNeeded = true;
		}

		public void UpdateForceFeedback( float deltaTime, bool checkButtons, nint windowHandle )
		{
			if ( _ffb_drawPrettyGraph )
			{
				if ( _ffb_writeableBitmap != null )
				{
					Dispatcher.BeginInvoke( () =>
					{
						_ffb_writeableBitmap.WritePixels( new Int32Rect( 0, 0, FFB_WRITEABLE_BITMAP_WIDTH, FFB_WRITEABLE_BITMAP_HEIGHT ), _ffb_pixels, FFB_PIXELS_BUFFER_STRIDE, 0, 0 );
					} );
				}
			}

			if ( checkButtons )
			{
				var playSound = false;

				foreach ( var joystick in _joystickList )
				{
					if ( joystick.Information.InstanceGuid == Settings.SetForegroundWindow.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.AutoOverallScale.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.DecreaseOverallScale.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.IncreaseOverallScale.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.DecreaseDetailScale.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.IncreaseDetailScale.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.DecreaseLFEScale.DeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.IncreaseLFEScale.DeviceInstanceGuid )
					{
						try
						{
							var joystickUpdateArray = joystick.GetBufferedData();

							if ( joystick.Information.InstanceGuid == Settings.SetForegroundWindow.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.SetForegroundWindow );

								if ( buttonPresses > 0 )
								{
									IntPtr foregroundWindowHandle = GetForegroundWindow();

									if ( foregroundWindowHandle != windowHandle )
									{
										SetForegroundWindow( windowHandle );
									}
									else
									{
										ReinitializeForceFeedbackDevice( windowHandle );
									}
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.AutoOverallScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.AutoOverallScale );

								if ( buttonPresses > 0 )
								{
									var smoothedTorque = 0f;
									var smoothedPeak = 0f;

									for ( var i = 0; i < _ffb_steeringWheelTorque.Length; i++ )
									{
										smoothedTorque = smoothedTorque * 0.9f + Math.Abs( _ffb_steeringWheelTorque[ i ] ) * 0.1f;

										if ( smoothedTorque > smoothedPeak )
										{
											smoothedPeak = smoothedTorque;
										}
									}

									if ( smoothedPeak > 0 )
									{
										var ratio = Math.Min( 1, Settings.TargetForce / smoothedPeak );

										Settings.OverallScale = (int) ( ratio * 100 );
									}
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.DecreaseOverallScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.DecreaseOverallScale );

								if ( buttonPresses > 0 )
								{
									Settings.OverallScale -= buttonPresses;

									_ffb_announceOverallScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.IncreaseOverallScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.IncreaseOverallScale );

								if ( buttonPresses > 0 )
								{
									Settings.OverallScale += buttonPresses;

									_ffb_announceOverallScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.DecreaseDetailScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.DecreaseDetailScale );

								if ( buttonPresses > 0 )
								{
									Settings.DetailScale -= buttonPresses;

									_ffb_announceDetailScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.IncreaseDetailScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.IncreaseDetailScale );

								if ( buttonPresses > 0 )
								{
									Settings.DetailScale += buttonPresses;

									_ffb_announceDetailScaleTimer = 1;

									playSound = true;
								}
							}
							if ( joystick.Information.InstanceGuid == Settings.DecreaseLFEScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.DecreaseLFEScale );

								if ( buttonPresses > 0 )
								{
									Settings.LFEScale -= buttonPresses;

									_ffb_announceLFEScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.IncreaseLFEScale.DeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.IncreaseLFEScale );

								if ( buttonPresses > 0 )
								{
									Settings.LFEScale += buttonPresses;

									_ffb_announceLFEScaleTimer = 1;

									playSound = true;
								}
							}
						}
						catch ( Exception )
						{
							joystick.Acquire();
						}
					}
				}

				if ( playSound )
				{
					PlayClick();
				}
			}

			if ( _ffb_announceOverallScaleTimer > 0 )
			{
				_ffb_announceOverallScaleTimer -= deltaTime;

				if ( _ffb_announceOverallScaleTimer <= 0 )
				{
					Say( $"The overall scale is now {Settings.OverallScale} percent." );
				}
			}

			if ( _ffb_announceDetailScaleTimer > 0 )
			{
				_ffb_announceDetailScaleTimer -= deltaTime;

				if ( _ffb_announceDetailScaleTimer <= 0 )
				{
					Say( $"The detail scale is now {Settings.DetailScale} percent." );
				}
			}

			if ( _ffb_announceLFEScaleTimer > 0 )
			{
				_ffb_announceLFEScaleTimer -= deltaTime;

				if ( _ffb_announceLFEScaleTimer <= 0 )
				{
					Say( $"The LFE scale is now {Settings.LFEScale} percent." );
				}
			}

			if ( _ffb_reinitializeNeeded )
			{
				_ffb_reinitializeNeeded = false;

				_ffb_reinitializeTimer = Math.Max( 0.25f, _ffb_reinitializeTimer );
			}

			if ( _ffb_forceFeedbackExceptionThrown )
			{
				_ffb_reinitializeTimer = Math.Max( 1.0f, _ffb_reinitializeTimer );
			}

			if ( _ffb_reinitializeTimer > 0 )
			{
				_ffb_reinitializeTimer -= deltaTime;

				if ( _ffb_reinitializeTimer <= 0 )
				{
					ReinitializeForceFeedbackDevice( windowHandle );
				}
			}

			if ( _ffb_wheelChanged || _carChanged || _trackChanged || _trackConfigChanged )
			{
				WriteLine( "" );
				WriteLine( $"Loading configuration [{_ffb_wheelSaveName}, {_carSaveName}, {_trackSaveName}, {_trackConfigSaveName}]" );

				_ffb_wheelChanged = false;
				_carChanged = false;
				_trackChanged = false;
				_trackConfigChanged = false;

				_pauseSerialization = true;

				var forceFeedbackSettingsFound = false;

				foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
				{
					if ( ( forceFeedbackSettings.WheelName == _ffb_wheelSaveName ) && ( forceFeedbackSettings.CarName == _carSaveName ) && ( forceFeedbackSettings.TrackName == _trackSaveName ) && ( forceFeedbackSettings.TrackConfigName == _trackConfigSaveName ) )
					{
						Settings.OverallScale = forceFeedbackSettings.OverallScale;
						Settings.DetailScale = forceFeedbackSettings.DetailScale;

						forceFeedbackSettingsFound = true;

						if ( Settings.OverallScale == Settings.DetailScale )
						{
							Say( $"The overall and detail scale have been restored to {Settings.OverallScale} percent." );
						}
						else
						{
							Say( $"The overall scale has been restored to {Settings.OverallScale} percent, and the detail scale to {Settings.DetailScale} percent." );
						}

						break;
					}
				}

				if ( !forceFeedbackSettingsFound )
				{
					Settings.OverallScale = 10;
					Settings.DetailScale = 100;

					Say( "This is the first time you have driven this combination, so we have reset the overall and detail scale." );
				}

				_pauseSerialization = false;
			}
		}

		public bool TogglePrettyGraph()
		{
			_ffb_drawPrettyGraph = !_ffb_drawPrettyGraph;

			return _ffb_drawPrettyGraph;
		}

		public void SendTestForceFeedbackSignal( bool invert )
		{
			var magnitude = invert ? -1500 : 1500;

			UpdateConstantForce( [ magnitude * 1, magnitude * 2, magnitude * 3, magnitude * 2, magnitude * 1, 0 ] );
		}

		public void UpdateWheelSaveName()
		{
			_ffb_wheelSaveName = ALL_WHEELS_SAVE_NAME;

			if ( Settings.SaveSettingsPerWheel )
			{
				if ( _ffb_drivingJoystick != null )
				{
					_ffb_wheelSaveName = _ffb_drivingJoystick.Information.ProductName;
				}
			}
		}

		public void UpdateConstantForce( int[] forceMagnitudeList )
		{
			for ( var i = 0; i < forceMagnitudeList.Length; i++ )
			{
				_ffb_magnitude[ i ] = forceMagnitudeList[ i ];
			}

			_ffb_updatesToSkip = 6;
			_ffb_resetMagnitudeMilliseconds = 1;
			_ffb_previousSteeringWheelTorque = 0;
			_ffb_scaledSteeringWheelTorque = 0;
		}

		private void ProcessSteeringWheelTorque()
		{
			if ( _isOnTrack )
			{
				// we want to reduce forces while the car is moving very slow or parked

				var speedScale = 1f;

				if ( _speed < 5 )
				{
					var t = _speed / 5;

					speedScale = ( Settings.ParkedScale / 100f ) * ( 1 - t ) + t;
				}

				// calculate the conversion scale from Newton-meters to DI_FFNOMINALMAX

				var ffbConversionScale = DI_FFNOMINALMAX / Settings.WheelMaxForce;

				// apply conversion scale to the overall scale

				var overallScale = ffbConversionScale * Settings.OverallScale / 100f;

				// calculate the detail scale

				var detailScale = overallScale + overallScale * ( ( Settings.DetailScale - 100f ) / 100f );

				// calculate the lfe scale

				var lfeScale = Settings.LFEScale / 100f * DI_FFNOMINALMAX;

				// grab the lfe read index

				var lfeMagnitudeIndex = _lfe_magnitudeIndex;

				// go through each sample

				for ( var x = 0; x < _steeringWheelTorque_ST.Length; x++ )
				{
					// get the next steering wheel torque sample (it is in Newton-meters)

					var currentSteeringWheelTorque = _steeringWheelTorque_ST[ x ];

					// save the original steering wheel torque (for auto-overall-scale feature)

					_ffb_steeringWheelTorque[ _ffb_steeringWheelTorqueIndex ] = currentSteeringWheelTorque;

					_ffb_steeringWheelTorqueIndex = ( _ffb_steeringWheelTorqueIndex + 1 ) % _ffb_steeringWheelTorque.Length;

					// calculate the impulse (change in steering wheel torque compared to the last sample)

					var deltaSteeringWheelTorque = currentSteeringWheelTorque - _ffb_previousSteeringWheelTorque;

					_ffb_previousSteeringWheelTorque = currentSteeringWheelTorque;

					// scale the impulse by our detail scale and add it to our running scaled steering wheel torque

					_ffb_scaledSteeringWheelTorque += deltaSteeringWheelTorque * detailScale;

					// ramp our running scaled magnitude towards the original signal (feed in steady state signal)

					_ffb_scaledSteeringWheelTorque = ( _ffb_scaledSteeringWheelTorque * 0.9f ) + ( currentSteeringWheelTorque * overallScale * 0.1f );

					// apply the speed scale and update the array that the force feedback thread uses

					_ffb_magnitude[ x ] = (int) ( _ffb_scaledSteeringWheelTorque * speedScale );

					// add in the low frequency effects

					_ffb_magnitude[ x ] += (int) ( _lfe_magnitude[ lfeMagnitudeIndex, x ] * lfeScale );

					// reset the magnitude timer

					_ffb_resetMagnitudeMilliseconds = 1;

					// update the pretty graph

					if ( _ffb_drawPrettyGraph )
					{
						var forceFeedbackMaxToPixelBufferHeightScale = DI_FFNOMINALMAX * 2 / FFB_PIXELS_BUFFER_HEIGHT;
						var halfPixelBufferOffset = FFB_PIXELS_BUFFER_HEIGHT / 2;

						var oY2 = (int) ( currentSteeringWheelTorque * overallScale / forceFeedbackMaxToPixelBufferHeightScale + halfPixelBufferOffset ) + 1;
						var oY1 = oY2 - 2;

						var sY2 = (int) ( _ffb_magnitude[ x ] / forceFeedbackMaxToPixelBufferHeightScale + halfPixelBufferOffset ) + 1;
						var sY1 = sY2 - 2;

						for ( var y = 0; y < 200; y++ )
						{
							var offset = y * FFB_PIXELS_BUFFER_STRIDE + _ffb_prettyGraphCurrentX * FFB_PIXELS_BUFFER_BYTES_PER_PIXEL;

							var onOriginal = ( y >= oY1 ) && ( y <= oY2 );
							var onScaled = ( y >= sY1 ) && ( y <= sY2 );

							if ( onOriginal && onScaled )
							{
								_ffb_pixels[ offset + 0 ] = 255;
								_ffb_pixels[ offset + 1 ] = 128;
								_ffb_pixels[ offset + 2 ] = 255;
							}
							else if ( onOriginal )
							{
								_ffb_pixels[ offset + 0 ] = 0;
								_ffb_pixels[ offset + 1 ] = 0;
								_ffb_pixels[ offset + 2 ] = 255;
							}
							else if ( onScaled )
							{
								_ffb_pixels[ offset + 0 ] = 255;
								_ffb_pixels[ offset + 1 ] = 128;
								_ffb_pixels[ offset + 2 ] = 0;
							}
							else if ( y == 25 || y == 50 || y == 75 || y == 125 || y == 150 || y == 175 )
							{
								_ffb_pixels[ offset + 0 ] = 64;
								_ffb_pixels[ offset + 1 ] = 64;
								_ffb_pixels[ offset + 2 ] = 64;
							}
							else if ( y == 100 )
							{
								_ffb_pixels[ offset + 0 ] = 0;
								_ffb_pixels[ offset + 1 ] = 128;
								_ffb_pixels[ offset + 2 ] = 0;
							}
							else
							{
								_ffb_pixels[ offset + 0 ] = 32;
								_ffb_pixels[ offset + 1 ] = 32;
								_ffb_pixels[ offset + 2 ] = 32;
							}

							_ffb_pixels[ offset + 3 ] = 255;
						}

						_ffb_prettyGraphCurrentX = ( _ffb_prettyGraphCurrentX + 1 ) % FFB_PIXELS_BUFFER_WIDTH;
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
			else
			{
				for ( var i = 0; i < _ffb_magnitude.Length; i++ )
				{
					_ffb_magnitude[ i ] = 0;
				}

				_ffb_previousSteeringWheelTorque = 0;
				_ffb_scaledSteeringWheelTorque = 0;
			}
		}

		private void UpdateForceFeedback()
		{
			if ( Settings.ForceFeedbackEnabled )
			{
				if ( Interlocked.Decrement( ref _ffb_updatesToSkip ) < 0 )
				{
					ProcessSteeringWheelTorque();
				}
			}
		}

		static private void MultimediaTimerEventCallback( uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2 )
		{
			var app = (App) Current;

			// check stopwatch

			var deltaMilliseconds = (float) app._ffb_stopwatch.Elapsed.TotalMilliseconds;
			var deltaSeconds = deltaMilliseconds / 1000f;

			app._ffb_stopwatch.Restart();

			// update clipped timer

			if ( app._ffb_clippedTimer > 0 )
			{
				app._ffb_clippedTimer = app._ffb_clippedTimer - deltaMilliseconds;
			}

			// reset the magnitude timer when its time

			if ( Interlocked.Exchange( ref app._ffb_resetMagnitudeMilliseconds, 0 ) == 1 )
			{
				app._ffb_magnitudeMilliseconds = 0;
			}

			// figure out where we are at in the 6 sample magnitude array

			var magnitudeIndex = app._ffb_magnitudeMilliseconds * 360 / 1000;

			// get the current magnitude, cubic interpolated

			var maxOffset = app._ffb_magnitude.Length - 1;

			var i1 = Math.Min( maxOffset, (int) Math.Truncate( magnitudeIndex ) );
			var i2 = Math.Min( maxOffset, i1 + 1 );
			var i3 = Math.Min( maxOffset, i2 + 1 );
			var i0 = Math.Max( 0, i1 - 1 );

			var t = Math.Min( 1, magnitudeIndex - i1 );

			var m0 = app._ffb_magnitude[ i0 ];
			var m1 = app._ffb_magnitude[ i1 ];
			var m2 = app._ffb_magnitude[ i2 ];
			var m3 = app._ffb_magnitude[ i3 ];

			var magnitude = (int) InterpolateHermite( m0, m1, m2, m3, t );

			// light clip indicator if we are out of range

			if ( magnitude > DI_FFNOMINALMAX )
			{
				magnitude = DI_FFNOMINALMAX;

				app._ffb_clippedTimer = 3;
			}
			else if ( magnitude < -DI_FFNOMINALMAX )
			{
				magnitude = -DI_FFNOMINALMAX;

				app._ffb_clippedTimer = 3;
			}

			// send the magnitude to the wheel

			if ( !app._ffb_forceFeedbackExceptionThrown && ( app._ffb_effectParameters != null ) && ( app._ffb_constantForceEffect != null ) )
			{
				( (ConstantForce) app._ffb_effectParameters.Parameters ).Magnitude = magnitude;

				try
				{
					app._ffb_constantForceEffect.SetParameters( app._ffb_effectParameters, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.NoRestart );
				}
				catch ( Exception exception )
				{
					app._ffb_forceFeedbackExceptionThrown = true;

					app.WriteLine( "" );
					app.WriteLine( "An exception was thrown while trying to update the constant force effect parameters!" );
					app.WriteLine( exception.Message.Trim() );
				}
			}

			// advance timer

			app._ffb_magnitudeMilliseconds += deltaMilliseconds;

			// test

			if ( false )
			{
				if ( testCounter < testData.Length )
				{
					testData[ testCounter++ ] = new TestData
					{
						deltaMilliseconds = deltaMilliseconds,
						magnitudeIndex = magnitudeIndex,
						m0 = m0,
						m1 = m1,
						m2 = m2,
						m3 = m3,
						magnitude = magnitude
					};

					if ( testCounter == testData.Length )
					{
						var targetMilliseconds = 18 - app.Settings.Frequency;

						foreach ( var data in testData )
						{
							var skew = data.deltaMilliseconds - targetMilliseconds;

							app.WriteLine( $"d={data.deltaMilliseconds:F3} ({skew:F3}); i={data.magnitudeIndex}; m0={data.m0:F0}; m1={data.m1:F0}; m2={data.m2:F0}; m3={data.m3:F0}; m={data.magnitude:F0}" );
						}
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
