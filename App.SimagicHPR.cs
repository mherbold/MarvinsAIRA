
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Simagic;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const float HPR_MIN_FREQUENCY = 15f; // usable frequency range is 15 to 35
		private const float HPR_MAX_FREQUENCY = 35f;

		private const float HPR_MIN_AMPLITUDE = 18f; // usable amplitude range is 18 to 60
		private const float HPR_MAX_AMPLITUDE = 60f;

		public const int HPR_WRITEABLE_BITMAP_WIDTH = 317;
		public const int HPR_WRITEABLE_BITMAP_HEIGHT = 200;
		public const int HPR_WRITEABLE_BITMAP_DPI = 96;

		public const int HPR_PIXELS_BUFFER_WIDTH = HPR_WRITEABLE_BITMAP_WIDTH;
		public const int HPR_PIXELS_BUFFER_HEIGHT = 200;
		public const int HPR_PIXELS_BUFFER_BYTES_PER_PIXEL = 4;
		public const int HPR_PIXELS_BUFFER_STRIDE = HPR_PIXELS_BUFFER_WIDTH * HPR_PIXELS_BUFFER_BYTES_PER_PIXEL;

		public const int HPR_PRETTY_GRAPH_UPDATE_WIDTH = 20;

		private readonly HPR _hpr = new();

		private int _hpr_gearLastFrame = -1;
		private float _hpr_gearChangeFrequency = 0f;
		private float _hpr_gearChangeTimer = 0f;

		public bool _hpr_drawPrettyGraphs = false;
		public int _hpr_prettyGraphCurrentX = 0;
		public float[] _hpr_prettyGraphCycles = [ 0f, 0f, 0f ];
		public float[] _hpr_frequency = [ 0f, 0f, 0f ];
		public float[] _hpr_amplitude = [ 0f, 0f, 0f ];
		public float[] _hpr_averageRpmSpeedRatioPerGear = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
		public float _hpr_currentRpmSpeedRatio = 0f;

		public readonly WriteableBitmap[] _hpr_writeableBitmaps = [
			new( HPR_WRITEABLE_BITMAP_WIDTH, HPR_WRITEABLE_BITMAP_HEIGHT, HPR_WRITEABLE_BITMAP_DPI, HPR_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null ),
			new( HPR_WRITEABLE_BITMAP_WIDTH, HPR_WRITEABLE_BITMAP_HEIGHT, HPR_WRITEABLE_BITMAP_DPI, HPR_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null ),
			new( HPR_WRITEABLE_BITMAP_WIDTH, HPR_WRITEABLE_BITMAP_HEIGHT, HPR_WRITEABLE_BITMAP_DPI, HPR_WRITEABLE_BITMAP_DPI, PixelFormats.Bgra32, null )
		];

		public readonly byte[][] _hpr_pixels = [
			new byte[ HPR_PIXELS_BUFFER_STRIDE * HPR_PIXELS_BUFFER_HEIGHT ],
			new byte[ HPR_PIXELS_BUFFER_STRIDE * HPR_PIXELS_BUFFER_HEIGHT ],
			new byte[ HPR_PIXELS_BUFFER_STRIDE * HPR_PIXELS_BUFFER_HEIGHT ]
		];

		public void InitializeHPR( bool isFirstInitialization = false )
		{
			var mainWindow = MarvinsAIRA.MainWindow.Instance;

			if ( isFirstInitialization && ( mainWindow != null ) )
			{
				mainWindow.PedalHapticsClutch_Image.Source = _hpr_writeableBitmaps[ 0 ];
				mainWindow.PedalHapticsBrake_Image.Source = _hpr_writeableBitmaps[ 1 ];
				mainWindow.PedalHapticsThrottle_Image.Source = _hpr_writeableBitmaps[ 2 ];
			}

			WriteLine( "InitializeHPR called.", true );

			_hpr.Uninitialize();

			if ( Settings.PedalHapticsEnabled )
			{
				_hpr.Initialize();
			}
		}

		public bool TogglePedalHapticsPrettyGraph()
		{
			_hpr_drawPrettyGraphs = !_hpr_drawPrettyGraphs;

			return _hpr_drawPrettyGraphs;
		}

		private void UninitializeHPR()
		{
			WriteLine( "UninitializeHPR called.", true );

			_hpr.Uninitialize();
		}

		private void StopHPR()
		{
			UninitializeHPR();
		}

		private void ResetHPR()
		{
			_hpr_gearLastFrame = 0;

			for ( var i = 0; i < _hpr_averageRpmSpeedRatioPerGear.Length; i++ )
			{
				_hpr_averageRpmSpeedRatioPerGear[ i ] = 0f;
			}
		}

		private void UpdateHPR()
		{
			// reset values

			for ( var i = 0; i < 3; i++ )
			{
				_hpr_frequency[ i ] = 0f;
				_hpr_amplitude[ i ] = 0f;
			}

			// if not on track or in replay then just turn off pedal vibrations

			if ( !_irsdk_isOnTrack || ( _irsdk_simMode == "replay" ) )
			{
				_hpr.VibratePedal( HPR.Channel.Clutch, HPR.State.Off, 0, 0 );
				_hpr.VibratePedal( HPR.Channel.Brake, HPR.State.Off, 0, 0 );
				_hpr.VibratePedal( HPR.Channel.Throttle, HPR.State.Off, 0, 0 );

				return;
			}

			// initialize effects

			bool[] effectEngaged = [ false, false, false, false, false, false, false, false, false ];

			float[] effectFrequency = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			float[] effectAmplitude = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];

			// gear change effect (1)

			if ( !_irsdk_isOnTrackLastFrame )
			{
				_hpr_gearLastFrame = _irsdk_gear;
			}

			if ( _irsdk_gear != _hpr_gearLastFrame )
			{
				if ( _irsdk_gear == 0 )
				{
					_hpr_gearChangeFrequency = HPR_MAX_FREQUENCY;
					_hpr_gearChangeTimer = 0.025f;
				}
				else
				{
					_hpr_gearChangeFrequency = HPR_MIN_FREQUENCY;
					_hpr_gearChangeTimer = 0.100f;
				}
			}

			_hpr_gearLastFrame = _irsdk_gear;

			if ( _hpr_gearChangeTimer > 0f )
			{
				_hpr_gearChangeTimer -= 1.0f / 20.0f;

				effectEngaged[ 1 ] = true;

				effectFrequency[ 1 ] = _hpr_gearChangeFrequency;
				effectAmplitude[ 1 ] = HPR_MAX_AMPLITUDE;
			}

			// ABS effect (2)

			if ( _irsdk_brakeABSactive )
			{
				effectEngaged[ 2 ] = true;

				effectFrequency[ 2 ] = ( HPR_MAX_FREQUENCY - HPR_MIN_FREQUENCY ) * 0.5f + HPR_MIN_FREQUENCY;
				effectAmplitude[ 2 ] = ( HPR_MAX_AMPLITUDE - HPR_MIN_AMPLITUDE ) * ( _irsdk_brake * 0.9f + 0.1f ) + HPR_MIN_AMPLITUDE;
			}

			// RPM (wide) effect (3)

			var rpm = _irsdk_rpm;

			var rpmRange = _irsdk_shiftLightsShiftRPM * 0.5f;
			var thresholdRPM = _irsdk_shiftLightsShiftRPM - rpmRange;

			if ( rpm > thresholdRPM )
			{
				rpm = Math.Clamp( ( rpm - thresholdRPM ) / rpmRange, 0f, 1f );

				effectEngaged[ 3 ] = true;

				effectFrequency[ 3 ] = ( HPR_MAX_FREQUENCY - HPR_MIN_FREQUENCY ) * MathF.Pow( rpm, 2f ) + HPR_MIN_FREQUENCY;
				effectAmplitude[ 3 ] = ( HPR_MAX_AMPLITUDE - HPR_MIN_AMPLITUDE ) * MathF.Pow( rpm, 3f ) * ( _irsdk_throttle * 0.9f + 0.1f ) + HPR_MIN_AMPLITUDE;
			}

			// RPM (narrow) effect (4)

			if ( ( _irsdk_gear >= 1 ) && ( _irsdk_gear < _irsdk_numForwardGears ) )
			{
				rpm = _irsdk_rpm;

				rpmRange = _irsdk_shiftLightsShiftRPM * 0.05f;
				thresholdRPM = _irsdk_shiftLightsShiftRPM - rpmRange;

				if ( rpm > thresholdRPM )
				{
					rpm = Math.Clamp( ( rpm - thresholdRPM ) / rpmRange, 0f, 1f );

					effectEngaged[ 4 ] = true;

					effectFrequency[ 4 ] = ( HPR_MAX_FREQUENCY - HPR_MIN_FREQUENCY ) * MathF.Pow( rpm, 2f ) + HPR_MIN_FREQUENCY;
					effectAmplitude[ 4 ] = ( HPR_MAX_AMPLITUDE - HPR_MIN_AMPLITUDE ) * MathF.Pow( rpm, 3f ) * ( _irsdk_throttle * 0.9f + 0.1f ) + HPR_MIN_AMPLITUDE;
				}
			}

			// steering effects (5)

			if ( Settings.SteeringEffectsEnabled && ( ( Settings.USEffectStyle == 4 ) || ( Settings.OSEffectStyle == 4 ) ) )
			{
				var effectAmount = 0f;

				if ( Settings.USEffectStyle == 4 )
				{
					var absUndersteerAmount = MathF.Abs( _ffb_understeerAmount );

					effectAmount = absUndersteerAmount * Settings.USEffectStrength / 100f;
					effectFrequency[ 5 ] = HPR_MAX_FREQUENCY;
				}

				if ( Settings.OSEffectStyle == 4 )
				{
					var absOversteerAmount = MathF.Abs( _ffb_oversteerAmount );

					if ( absOversteerAmount > effectAmount )
					{
						effectAmount = absOversteerAmount * Settings.OSEffectStrength / 100f;
						effectFrequency[ 5 ] = ( HPR_MAX_FREQUENCY - HPR_MIN_FREQUENCY ) / 2f + HPR_MIN_FREQUENCY;
					}
				}

				if ( effectAmount > 0f )
				{
					effectEngaged[ 5 ] = true;

					effectAmplitude[ 5 ] = ( HPR_MAX_AMPLITUDE - HPR_MIN_AMPLITUDE ) * effectAmount + HPR_MIN_AMPLITUDE;
				}
			}

			// update rpm vs speed ratios for wheel lock and spin effects

			if ( ( _irsdk_gear > 0 ) && ( _irsdk_rpm > 100f ) && ( _irsdk_velocityX > 5f ) )
			{
				_hpr_currentRpmSpeedRatio = _irsdk_velocityX / _irsdk_rpm;

				if ( ( _irsdk_brake == 0f ) && ( _irsdk_clutch == 1f ) )
				{
					if ( _hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] == 0.0f )
					{
						_hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] = _hpr_currentRpmSpeedRatio;
					}
					else
					{
						_hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] = _hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] * 0.95f + _hpr_currentRpmSpeedRatio * 0.05f;
					}
				}

				// wheel lock (6)

				if ( _hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] != 0f )
				{
					if ( _irsdk_clutch == 1f )
					{
						if ( _hpr_currentRpmSpeedRatio > _hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] * 1.05f )
						{
							effectEngaged[ 6 ] = true;

							effectFrequency[ 6 ] = HPR_MAX_FREQUENCY;
							effectAmplitude[ 6 ] = HPR_MAX_AMPLITUDE;
						}
					}
				}

				// wheel spin (7)

				if ( _hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] != 0f )
				{
					if ( _irsdk_clutch == 1f )
					{
						if ( _hpr_currentRpmSpeedRatio < _hpr_averageRpmSpeedRatioPerGear[ _irsdk_gear ] * 0.95f )
						{
							effectEngaged[ 7 ] = true;

							effectFrequency[ 7 ] = HPR_MAX_FREQUENCY;
							effectAmplitude[ 7 ] = HPR_MAX_AMPLITUDE;
						}
					}
				}
			}
			else
			{
				_hpr_currentRpmSpeedRatio = 0f;
			}

			// clutch slip effect (8)

			if ( ( _irsdk_clutch > 0.25f ) && ( _irsdk_clutch < 0.75f ) )
			{
				rpm = _irsdk_rpm;

				rpmRange = _irsdk_shiftLightsShiftRPM * 0.5f;
				thresholdRPM = _irsdk_shiftLightsShiftRPM - rpmRange;

				if ( rpm > thresholdRPM )
				{
					rpm = Math.Clamp( ( rpm - thresholdRPM ) / rpmRange, 0f, 1f );

					effectEngaged[ 8 ] = true;

					effectFrequency[ 8 ] = ( HPR_MAX_FREQUENCY - HPR_MIN_FREQUENCY ) * MathF.Pow( rpm, 2f ) + HPR_MIN_FREQUENCY;
					effectAmplitude[ 8 ] = ( HPR_MAX_AMPLITUDE - HPR_MIN_AMPLITUDE ) * 0.5f + HPR_MIN_AMPLITUDE;
				}
			}

			// apply effects to pedals

			for ( var i = 0; i < 3; i++ )
			{
				var effect1 = ( i == 0 ) ? Settings.PedalHapticsClutchEffect1 : ( i == 1 ) ? Settings.PedalHapticsBrakeEffect1 : Settings.PedalHapticsThrottleEffect1;
				var effect2 = ( i == 0 ) ? Settings.PedalHapticsClutchEffect2 : ( i == 1 ) ? Settings.PedalHapticsBrakeEffect2 : Settings.PedalHapticsThrottleEffect2;
				var effect3 = ( i == 0 ) ? Settings.PedalHapticsClutchEffect3 : ( i == 1 ) ? Settings.PedalHapticsBrakeEffect3 : Settings.PedalHapticsThrottleEffect3;

				var scale1 = ( i == 0 ) ? Settings.PedalHapticsClutchEffectStrength1 : ( i == 1 ) ? Settings.PedalHapticsBrakeEffectStrength1 : Settings.PedalHapticsThrottleEffectStrength1;
				var scale2 = ( i == 0 ) ? Settings.PedalHapticsClutchEffectStrength2 : ( i == 1 ) ? Settings.PedalHapticsBrakeEffectStrength2 : Settings.PedalHapticsThrottleEffectStrength2;
				var scale3 = ( i == 0 ) ? Settings.PedalHapticsClutchEffectStrength3 : ( i == 1 ) ? Settings.PedalHapticsBrakeEffectStrength3 : Settings.PedalHapticsThrottleEffectStrength3;

				if ( effectEngaged[ effect1 ] )
				{
					_hpr_frequency[ i ] = effectFrequency[ effect1 ];
					_hpr_amplitude[ i ] = ( effectAmplitude[ effect1 ] - HPR_MIN_AMPLITUDE ) * scale1 + HPR_MIN_AMPLITUDE;
				}
				else if ( effectEngaged[ effect2 ] )
				{
					_hpr_frequency[ i ] = effectFrequency[ effect2 ];
					_hpr_amplitude[ i ] = ( effectAmplitude[ effect2 ] - HPR_MIN_AMPLITUDE ) * scale2 + HPR_MIN_AMPLITUDE;
				}
				else if ( effectEngaged[ effect3 ] )
				{
					_hpr_frequency[ i ] = effectFrequency[ effect3 ];
					_hpr_amplitude[ i ] = ( effectAmplitude[ effect3 ] - HPR_MIN_AMPLITUDE ) * scale3 + HPR_MIN_AMPLITUDE;
				}

				if ( ( _hpr_frequency[ i ] == 0f ) || ( _hpr_amplitude[ i ] == 0f ) )
				{
					_hpr.VibratePedal( (HPR.Channel) i, HPR.State.Off, 0f, 0f );
				}
				else
				{
					_hpr.VibratePedal( (HPR.Channel) i, HPR.State.On, _hpr_frequency[ i ], _hpr_amplitude[ i ] );
				}
			}

			// update pretty graphs

			if ( _hpr_drawPrettyGraphs )
			{
				var centerLine = HPR_PIXELS_BUFFER_HEIGHT / 2;

				for ( var j = 0; j < HPR_PRETTY_GRAPH_UPDATE_WIDTH; j++ )
				{
					for ( var i = 0; i < 3; i++ )
					{
						_hpr_prettyGraphCycles[ i ] += ( _hpr_frequency[ i ] * 2f * MathF.PI ) / ( HPR_PRETTY_GRAPH_UPDATE_WIDTH * 20f );

						int y1, y2;

						var a = MathF.Sin( _hpr_prettyGraphCycles[ i ] ) * ( _hpr_amplitude[ i ] / 100.0f ) * centerLine;

						if ( a < 0 )
						{
							y1 = (int) a + centerLine;
							y2 = centerLine;
						}
						else
						{
							y1 = centerLine;
							y2 = (int) a + centerLine;
						}

						for ( var y = 0; y < HPR_PIXELS_BUFFER_HEIGHT; y++ )
						{
							var offset = y * HPR_PIXELS_BUFFER_STRIDE + _hpr_prettyGraphCurrentX * HPR_PIXELS_BUFFER_BYTES_PER_PIXEL;

							if ( ( y >= y1 ) && ( y <= y2 ) )
							{
								_hpr_pixels[ i ][ offset + 0 ] = 255;
								_hpr_pixels[ i ][ offset + 1 ] = 255;
								_hpr_pixels[ i ][ offset + 2 ] = 255;
								_hpr_pixels[ i ][ offset + 3 ] = 255;
							}
							else if ( y == centerLine )
							{
								_hpr_pixels[ i ][ offset + 0 ] = 0;
								_hpr_pixels[ i ][ offset + 1 ] = 128;
								_hpr_pixels[ i ][ offset + 2 ] = 0;
								_hpr_pixels[ i ][ offset + 3 ] = 255;
							}
							else
							{
								_hpr_pixels[ i ][ offset + 0 ] = 64;
								_hpr_pixels[ i ][ offset + 1 ] = 32;
								_hpr_pixels[ i ][ offset + 2 ] = 32;
								_hpr_pixels[ i ][ offset + 3 ] = 255;
							}
						}
					}

					_hpr_prettyGraphCurrentX = ( _hpr_prettyGraphCurrentX + 1 ) % HPR_PIXELS_BUFFER_WIDTH;
				}
			}
		}
	}
}
