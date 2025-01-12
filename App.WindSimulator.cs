
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const float WIND_MPS_TO_MPH = 2.23694f;
		private const float WIND_MPS_TO_KPH = 3.6f;

		static readonly byte[] WIND_HANDSHAKE = { (byte) 'w', (byte) 'i', (byte) 'n', (byte) 'd' };

		private bool _wind_initialized = false;

		private float _wind_currentMagnitude = 0;

		private int _wind_leftSpinState = 0;
		private int _wind_rightSpinState = 0;

		private Arduino _wind_arduino = new( WIND_HANDSHAKE );

		public bool Wind_Initialized { get => _wind_initialized; }
		public int Wind_TestBand { get; set; } = -1;
		public float Wind_CurrentMagnitude { get => _wind_currentMagnitude; }

		public void InitializeWindSimulator()
		{
			WriteLine( "" );
			WriteLine( "InitializeWindSimulator called." );

			if ( _wind_arduino.SerialPort == null )
			{
				WriteLine( "We could not find the Arduino controller for the wind simulator." );
			}
			else
			{
				WriteLine( "We found the Arduino controller for the wind simulator." );

				_wind_initialized = true;
			}
		}

		public void StopWindSimulator()
		{
			WriteLine( "" );
			WriteLine( "StopWindSimulator called." );

			_wind_arduino.Close();
		}

		public void UpdateWindSimulator()
		{
			if ( _wind_initialized )
			{
				_wind_currentMagnitude = 0;

				var leftWindForce = _wind_currentMagnitude;
				var rightWindForce = _wind_currentMagnitude;

				if ( Settings.WindSimulatorEnabled )
				{
					int[] carSpeedArray = [ Settings.CarSpeed1, Settings.CarSpeed2, Settings.CarSpeed3, Settings.CarSpeed4, Settings.CarSpeed5, Settings.CarSpeed6, Settings.CarSpeed7, Settings.CarSpeed8 ];
					float[] windForceArray = [ Settings.WindForce1, Settings.WindForce2, Settings.WindForce3, Settings.WindForce4, Settings.WindForce5, Settings.WindForce6, Settings.WindForce7, Settings.WindForce8 ];

					_wind_currentMagnitude = windForceArray[ 0 ];

					leftWindForce = _wind_currentMagnitude;
					rightWindForce = _wind_currentMagnitude;

					if ( Wind_TestBand != -1 )
					{
						_wind_currentMagnitude = windForceArray[ Wind_TestBand ];

						leftWindForce = _wind_currentMagnitude;
						rightWindForce = _wind_currentMagnitude;
					}
					else if ( _irsdk.IsConnected )
					{
						if ( _isOnTrack )
						{
							var velocityX = _velocityX;
							var velocityY = _velocityY;

							var leftWindForceScale = 1f - 0.85f * Math.Max( 0, Math.Min( 1, velocityY * 5f ) );
							var rightWindForceScale = 1f - 0.85f * Math.Max( 0, Math.Min( 1, -velocityY * 5f ) );

							if ( _displayUnits == 0 )
							{
								velocityX *= WIND_MPS_TO_MPH;
							}
							else
							{
								velocityX *= WIND_MPS_TO_KPH;
							}

							_wind_currentMagnitude = GetWindForce( velocityX, carSpeedArray, windForceArray );

							leftWindForce = _wind_currentMagnitude * leftWindForceScale;
							rightWindForce = _wind_currentMagnitude * rightWindForceScale;
						}
					}
				}
				else
				{
					_wind_currentMagnitude = 0;
				}

				UpdateFanPowers( leftWindForce, rightWindForce );
			}
		}

		private float GetWindForce( float carSpeed, int[] carSpeedArray, float[] windForceArray )
		{
			if ( carSpeed < 0 )
			{
				carSpeed = 0;
			}

			int band0 = 0;
			int band1 = 0;

			for ( var i = 0; i < carSpeedArray.Length; i++ )
			{
				band1 = i;

				if ( carSpeed < carSpeedArray[ i ] )
				{
					break;
				}

				band0 = band1;
			}

			var bandWidth = carSpeedArray[ band1 ] - carSpeedArray[ band0 ];

			if ( bandWidth <= 0 )
			{
				return windForceArray[ band1 ];
			}

			var t = ( carSpeed - carSpeedArray[ band0 ] ) / bandWidth;

			if ( t <= 0 )
			{
				return windForceArray[ band0 ];
			}

			return t * ( windForceArray[ band1 ] - windForceArray[ band0 ] ) + windForceArray[ band0 ];
		}

		private void UpdateFanPowers( float leftWindForce, float rightWindForce )
		{
			var serialPort = _wind_arduino.SerialPort;

			if ( serialPort != null )
			{
				if ( leftWindForce < 0.5 )
				{
					leftWindForce = 0;
				}
				else if ( leftWindForce < 3 )
				{
					leftWindForce = 3;
				}

				var leftFanPower = Math.Min( 320, Math.Max( 0, (int) ( 320 * leftWindForce / 100 ) ) );

				if ( leftFanPower == 0 )
				{
					_wind_leftSpinState = 0;
				}
				else if ( _wind_leftSpinState < 2 )
				{
					_wind_leftSpinState++;

					leftFanPower = 160;
				}

				if ( rightWindForce < 0.5 )
				{
					rightWindForce = 0;
				}
				else if ( rightWindForce < 3 )
				{
					rightWindForce = 3;
				}

				var rightFanPower = Math.Min( 320, Math.Max( 0, (int) ( 320 * rightWindForce / 100 ) ) );

				if ( rightFanPower == 0 )
				{
					_wind_rightSpinState = 0;
				}
				else if ( _wind_rightSpinState < 2 )
				{
					_wind_rightSpinState++;

					rightFanPower = 160;
				}

				try
				{
					serialPort.Write( $"L{leftFanPower:000}" );
					serialPort.Write( $"R{rightFanPower:000}" );
				}
				catch
				{
				}
			}
		}
	}
}
