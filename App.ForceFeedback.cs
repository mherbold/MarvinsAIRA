﻿
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SharpDX.DirectInput;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		public const int DI_FFNOMINALMAX = 10000;
		private const int DIEB_NOTRIGGER = -1;

		private const int FFB_SAMPLES_PER_FRAME = 6;
		private const int FFB_UPDATE_FREQUENCY = 360;
		private const int FFB_MICROSECONDS_PER_SECOND = 1000000;
		private const int FFB_MICROSECONDS_PER_UPDATE = FFB_MICROSECONDS_PER_SECOND / FFB_UPDATE_FREQUENCY;

		private const int FFB_WRITEABLE_BITMAP_WIDTH = 768;
		private const int FFB_WRITEABLE_BITMAP_HEIGHT = 200;
		private const int FFB_WRITEABLE_BITMAP_DPI = 96;

		private const int FFB_PIXELS_BUFFER_WIDTH = FFB_WRITEABLE_BITMAP_WIDTH + 1;
		private const int FFB_PIXELS_BUFFER_HEIGHT = 200;
		private const int FFB_PIXELS_BUFFER_BYTES_PER_PIXEL = 4;
		private const int FFB_PIXELS_BUFFER_STRIDE = FFB_PIXELS_BUFFER_WIDTH * FFB_PIXELS_BUFFER_BYTES_PER_PIXEL;

		private Joystick? _ffb_drivingJoystick = null;
		private EffectInfo? _ffb_constantForceEffectInfo = null;
		private EffectParameters? _ffb_effectParameters = null;
		private Effect? _ffb_constantForceEffect = null;

		private bool _ffb_initialized = false;
		private bool _ffb_reacquireNeeded = false;
		private float _ffb_reacquireTimer = 0;

		private int _ffb_currentMagnitude = 0;
		private float _ffb_clippedTimer = 0;

		private readonly float[] _ffb_scaledMagnitudeArray = new float[ FFB_SAMPLES_PER_FRAME ];
		private int _ffb_scaledMagnitudeIndex = 0;

		private float _ffb_previousOriginalMagnitude = 0;
		private float _ffb_scaledMagnitude = 0;

		private readonly SoundPlayer _ffb_clickSoundPlayer = new( new MemoryStream( MarvinsAIRA.Properties.Resources.Click ) );

		private float _ffb_announceOverallScaleTimer = 0;
		private float _ffb_announceDetailScaleTimer = 0;

		private bool _ffb_drawPrettyGraph = false;
		private int _ffb_prettyGraphCurrentX = 0;
		private readonly WriteableBitmap _ffb_writeableBitmap = new( FFB_WRITEABLE_BITMAP_WIDTH, FFB_WRITEABLE_BITMAP_HEIGHT, FFB_WRITEABLE_BITMAP_DPI, FFB_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null );
		private readonly byte[] _ffb_pixels = new byte[ FFB_PIXELS_BUFFER_STRIDE * FFB_PIXELS_BUFFER_HEIGHT ];

		private bool _ffb_pauseTask = false;
		private bool _ffb_taskPaused = false;

		private bool _ffb_stopTask = false;
		private bool _ffb_taskStopped = true;

		public bool FFB_Initialized { get => _ffb_initialized; }
		public bool FFB_TaskIsRunning { get => !_ffb_taskStopped; }
		public int FFB_CurrentMagnitude { get => _ffb_currentMagnitude; }
		public float FFB_ClippedTimer { get => _ffb_clippedTimer; }

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

			WriteLine( "Initializing DirectInput (FF driving device)..." );

			var directInput = new DirectInput();

			DeviceInstance? forceFeedbackDeviceInstance = null;

			DeviceType[] deviceTypeArray = [ DeviceType.Driving, DeviceType.FirstPerson, DeviceType.Joystick, DeviceType.Gamepad ];

			if ( Settings.SelectedDeviceId == Guid.Empty )
			{
				WriteLine( "...there is not already a selected force feedback device, looking for the first attached force feedback driving device..." );

				foreach ( var deviceType in deviceTypeArray )
				{
					var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.ForceFeedback | DeviceEnumerationFlags.AttachedOnly );

					forceFeedbackDeviceInstance = deviceInstanceList.FirstOrDefault();

					if ( forceFeedbackDeviceInstance != null )
					{
						Settings.SelectedDeviceId = forceFeedbackDeviceInstance.InstanceGuid;
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
						if ( deviceInstance.InstanceGuid == Settings.SelectedDeviceId )
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

					ReinitializeForceFeedbackDevice( windowHandle );

					if ( Settings.ForceFeedbackEnabled )
					{
						StartForceFeedbackTask();
					}
				}
			}
			else
			{
				WriteLine( "...no force feedback driving device was selected!" );
			}
		}

		public void UninitializeForceFeedback()
		{
			WriteLine( "" );
			WriteLine( "UninitializeForceFeedback called." );

			if ( !_ffb_taskStopped )
			{
				StopForceFeedbackTask();
			}

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

			_ffb_initialized = false;
		}

		public void StartForceFeedbackTask()
		{
			WriteLine( "" );
			WriteLine( "StartForceFeedbackTask called." );
			WriteLine( "Starting the force feedback update thread..." );

			if ( !_ffb_initialized )
			{
				WriteLine( "...the force feedback system has not been initialized, so we will not start the force feedback background thread." );
			}
			else
			{
				Task.Run( ForceFeedbackUpdateThread );

				WriteLine( "...the force feedback update thread has been started." );
			}
		}

		public void StopForceFeedbackTask()
		{
			WriteLine( "" );
			WriteLine( "StopForceFeedbackTask called." );
			WriteLine( "Stopping the force feedback update thread..." );

			_ffb_stopTask = true;

			while ( !_ffb_taskStopped )
			{
				Thread.Sleep( 0 );
			}

			_ffb_stopTask = false;

			WriteLine( "...the force feedback update thread has been stopped." );
		}

		public void ReinitializeForceFeedbackDevice( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "ReinitializeForceFeedbackDevice called." );
			WriteLine( "Reinitializing the force feedback device..." );

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

				_ffb_reacquireNeeded = false;

				WriteLine( "...the force feedback device has been reinitialized." );
			}
			catch ( Exception exception )
			{
				WriteLine( "...failed to reacquire the force feedback device:" );
				WriteLine( exception.Message.Trim() );

				_ffb_reacquireTimer = 2;
			}
		}

		public void UpdateForceFeedback( float deltaTime, bool checkButtons, nint windowHandle )
		{
			if ( _ffb_clippedTimer > 0 )
			{
				_ffb_clippedTimer -= deltaTime;
			}

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
					if ( joystick.Information.InstanceGuid == Settings.DecreaseOverallScaleDeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.IncreaseOverallScaleDeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.DecreaseDetailScaleDeviceInstanceGuid || joystick.Information.InstanceGuid == Settings.IncreaseDetailScaleDeviceInstanceGuid )
					{
						try
						{
							joystick.Poll();

							var joystickUpdateArray = joystick.GetBufferedData();

							if ( joystick.Information.InstanceGuid == Settings.DecreaseOverallScaleDeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.DecreaseOverallScaleButtonNumber );

								if ( buttonPresses > 0 )
								{
									Settings.OverallScale -= 10 * buttonPresses;

									_ffb_announceOverallScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.IncreaseOverallScaleDeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.IncreaseOverallScaleButtonNumber );

								if ( buttonPresses > 0 )
								{
									Settings.OverallScale += 10 * buttonPresses;

									_ffb_announceOverallScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.DecreaseDetailScaleDeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.DecreaseDetailScaleButtonNumber );

								if ( buttonPresses > 0 )
								{
									Settings.DetailScale -= buttonPresses;

									_ffb_announceDetailScaleTimer = 1;

									playSound = true;
								}
							}

							if ( joystick.Information.InstanceGuid == Settings.IncreaseDetailScaleDeviceInstanceGuid )
							{
								var buttonPresses = GetButtonPressCount( joystickUpdateArray, Settings.IncreaseDetailScaleButtonNumber );

								if ( buttonPresses > 0 )
								{
									Settings.DetailScale += buttonPresses;

									_ffb_announceDetailScaleTimer = 1;

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
					_ffb_clickSoundPlayer.Play();
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

			if ( _ffb_reacquireNeeded )
			{
				_ffb_reacquireTimer -= deltaTime;

				if ( _ffb_reacquireTimer <= 0 )
				{
					ReinitializeForceFeedbackDevice( windowHandle );
				}
			}

			if ( _carChanged || _trackChanged )
			{
				_carChanged = false;
				_trackChanged = false;

				var forceFeedbackSettingsFound = false;

				foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
				{
					if ( ( forceFeedbackSettings.CarScreenName == _currentCarScreenName ) && ( forceFeedbackSettings.TrackDisplayName == _currentTrackDisplayName ) && ( forceFeedbackSettings.TrackConfigName == _currentTrackConfigName ) )
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
					Settings.OverallScale = 100;
					Settings.DetailScale = 100;

					Say( "This is the first time we've seen this car on this track on this track configuration, so we have reset the overall and detail scale to 100 percent." );
				}
			}
		}

		public bool TogglePrettyGraph()
		{
			_ffb_drawPrettyGraph = !_ffb_drawPrettyGraph;

			return _ffb_drawPrettyGraph;
		}

		public void PauseTask()
		{
			if ( FFB_TaskIsRunning )
			{
				_ffb_pauseTask = true;

				while ( !_ffb_taskPaused )
				{
					Thread.Sleep( 0 );
				}

				_ffb_taskPaused = false;
			}
		}

		public void UnpauseTask()
		{
			if ( FFB_TaskIsRunning )
			{
				_ffb_pauseTask = false;
			}
		}

		public void SendTestForceFeedbackSignal()
		{
			PauseTask();

			_ffb_scaledMagnitudeArray[ 0 ] = 3000;
			_ffb_scaledMagnitudeArray[ 1 ] = 0;
			_ffb_scaledMagnitudeArray[ 2 ] = -3000;
			_ffb_scaledMagnitudeArray[ 3 ] = 0;
			_ffb_scaledMagnitudeArray[ 4 ] = 3000;
			_ffb_scaledMagnitudeArray[ 5 ] = 0;

			_ffb_previousOriginalMagnitude = 0;
			_ffb_scaledMagnitude = 0;
			_ffb_scaledMagnitudeIndex = 0;

			UnpauseTask();
		}

		public void UpdateMagnitude( int newMagnitude )
		{
			if ( newMagnitude != _ffb_currentMagnitude )
			{
				_ffb_currentMagnitude = newMagnitude;

				if ( _ffb_currentMagnitude > DI_FFNOMINALMAX )
				{
					_ffb_currentMagnitude = DI_FFNOMINALMAX;

					_ffb_clippedTimer = 1;
				}
				else if ( _ffb_currentMagnitude < -DI_FFNOMINALMAX )
				{
					_ffb_currentMagnitude = -DI_FFNOMINALMAX;

					_ffb_clippedTimer = 1;
				}

				if ( !_ffb_reacquireNeeded )
				{
					if ( ( _ffb_effectParameters != null ) && ( _ffb_constantForceEffect != null ) )
					{
						( (ConstantForce) _ffb_effectParameters.Parameters ).Magnitude = _ffb_currentMagnitude;

						try
						{
							_ffb_constantForceEffect.SetParameters( _ffb_effectParameters, EffectParameterFlags.TypeSpecificParameters | EffectParameterFlags.NoRestart );
						}
						catch ( Exception exception )
						{
							_ffb_reacquireNeeded = true;

							WriteLine( "" );
							WriteLine( "An exception was thrown while trying to update the constant force effect parameters!" );
							WriteLine( exception.Message.Trim() );
						}
					}
				}
			}
		}

		private static int GetButtonPressCount( JoystickUpdate[] joystickUpdateArray, int buttonNumber )
		{
			var buttonPressCount = 0;

			foreach ( var joystickUpdate in joystickUpdateArray )
			{
				if ( joystickUpdate.Offset == JoystickOffset.Buttons0 + buttonNumber )
				{
					if ( joystickUpdate.Value != 0 )
					{
						buttonPressCount++;
					}
				}
			}

			return buttonPressCount;
		}

		private void ProcessSteeringWheelTorque( bool drawPrettyGraph )
		{
			if ( _irsdk.Data.GetBool( _isOnTrackDatum ) )
			{
				// we want to reduce forces while the car is moving very slow or parked

				var speed = _irsdk.Data.GetFloat( _speedDatum );

				var speedScale = ( speed >= 5 ) ? 1 : Math.Max( 0.1f, ( speed / 5 ) );

				// get the next 6 FFB samples from iRacing

				var originalMagnitudeArray = new float[ FFB_SAMPLES_PER_FRAME ];

				_irsdk.Data.GetFloatArray( _steeringWheelTorque_STDatum, originalMagnitudeArray, 0, originalMagnitudeArray.Length );

				// calculate the conversion scale from Newton-meters to DI_FFNOMINALMAX

				var conversionScale = DI_FFNOMINALMAX / Settings.WheelMaxForce;

				// apply conversion scale to the overall scale

				var overallScale = conversionScale * Settings.OverallScale / 100f;

				// calculate the detail scale

				var detailScale = overallScale + overallScale * ( ( Settings.DetailScale - 100f ) / 100f );

				// go through each sample

				for ( var x = 0; x < originalMagnitudeArray.Length; x++ )
				{
					// get the FFB magnitude (it is in Newton-meters)

					var currentOriginalMagnitude = originalMagnitudeArray[ x ];

					// calculate the impulse (change in FFB magnitude compared to the last sample)

					var deltaOriginalMagnitude = currentOriginalMagnitude - _ffb_previousOriginalMagnitude;

					_ffb_previousOriginalMagnitude = currentOriginalMagnitude;

					// scale the impulse by our detail scale and add it to our running scaled magnitude

					_ffb_scaledMagnitude += deltaOriginalMagnitude * detailScale;

					// ramp our running scaled magnitude towards the original signal

					_ffb_scaledMagnitude = ( _ffb_scaledMagnitude * 0.97f ) + ( currentOriginalMagnitude * overallScale * 0.03f );

					// apply the speed scale and update the array that the background thread uses

					_ffb_scaledMagnitudeArray[ x ] = _ffb_scaledMagnitude * speedScale;

					// update the pretty graph

					if ( drawPrettyGraph )
					{
						var forceToImageHeightScale = (float) Settings.OverallScale / 100;
						var forceToImageHeightOffset = 100;

						var oY2 = (int) ( currentOriginalMagnitude * forceToImageHeightScale + forceToImageHeightOffset ) + 1;
						var oY1 = oY2 - 2;

						var sY2 = (int) ( _ffb_scaledMagnitudeArray[ x ] / 100 + forceToImageHeightOffset ) + 1;
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

				if ( drawPrettyGraph )
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
				for ( var i = 0; i < _ffb_scaledMagnitudeArray.Length; i++ )
				{
					_ffb_scaledMagnitudeArray[ i ] = 0;
				}

				_ffb_previousOriginalMagnitude = 0;
				_ffb_scaledMagnitude = 0;
			}

			_ffb_scaledMagnitudeIndex = 0;
		}

		private void UpdateForceFeedback()
		{
			PauseTask();

			ProcessSteeringWheelTorque( true );

			UnpauseTask();
		}

		private void ForceFeedbackUpdateThread()
		{
			_ffb_taskStopped = false;

			var stopwatch = new Stopwatch();

			stopwatch.Start();

			while ( !_ffb_stopTask )
			{
				Thread.Sleep( 0 );

				var elapsed = stopwatch.Elapsed;

				if ( elapsed.TotalMicroseconds >= FFB_MICROSECONDS_PER_UPDATE )
				{
					stopwatch.Restart();

					if ( _ffb_scaledMagnitudeIndex < _ffb_scaledMagnitudeArray.Length )
					{
						UpdateMagnitude( (int) _ffb_scaledMagnitudeArray[ _ffb_scaledMagnitudeIndex++ ] );
					}
				}

				if ( _ffb_pauseTask )
				{
					_ffb_taskPaused = true;

					while ( _ffb_pauseTask )
					{
						Thread.Sleep( 0 );

						if ( _ffb_stopTask )
						{
							break;
						}
					}
				}
			}

			UpdateMagnitude( 0 );

			_ffb_taskStopped = true;
		}
	}
}