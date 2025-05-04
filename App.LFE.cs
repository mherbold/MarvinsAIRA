
using System.Runtime.InteropServices;
using System.Windows;

using SharpDX.DirectSound;
using SharpDX.Multimedia;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private bool _lfe_initialized = false;
		private DirectSoundCapture? _lfe_directSoundCapture;
		private CaptureBuffer? _lfe_captureBuffer;
		private AutoResetEvent _lfe_autoResetEvent = new( false );
		private float[,] _lfe_magnitude = new float[ 2, FFB_SAMPLES_PER_FRAME ];
		private int _lfe_magnitudeIndex = 0;
		private bool _lfe_keepThreadAlive = false;
		private bool _lfe_threadRunning = false;

		private const int LFE_360HZ_TO_8KHZ_SCALE = 22;
		private const int LFE_FRAME_SIZE_IN_SAMPLES = FFB_SAMPLES_PER_FRAME * LFE_360HZ_TO_8KHZ_SCALE;
		private const int LFE_BYTES_PER_SAMPLE = 2;
		private const int LFE_FRAME_SIZE_IN_BYTES = LFE_FRAME_SIZE_IN_SAMPLES * LFE_BYTES_PER_SAMPLE;

		private const int LFE_CAPTURE_BUFFER_FREQUENCY = 8000;
		private const int LFE_CAPTURE_BUFFER_BITS_PER_SAMPLE = LFE_BYTES_PER_SAMPLE * 8;
		private const int LFE_CAPTURE_BUFFER_SAMPLES = 360 * LFE_360HZ_TO_8KHZ_SCALE;
		private const int LFE_CAPTURE_BUFFER_SIZE_IN_BYTES = LFE_CAPTURE_BUFFER_SAMPLES * LFE_BYTES_PER_SAMPLE;

		public void InitializeLFE()
		{
			WriteLine( "InitializeLFE called.", true );

			if ( _lfe_initialized )
			{
				UninitializeLFE();
			}

			WriteLine( "...enumerating recording devices (for LFE)..." );

			DeviceInformation? selectedDeviceInformation = null;

			SerializableDictionary<Guid, string> lfeDeviceList = [];

			var deviceInformationList = DirectSoundCapture.GetDevices();

			foreach ( var deviceInformation in deviceInformationList )
			{
				if ( deviceInformation.DriverGuid != Guid.Empty )
				{
					WriteLine( $"...we found the {deviceInformation.Description} device..." );

					lfeDeviceList.Add( deviceInformation.DriverGuid, deviceInformation.Description );

					if ( deviceInformation.DriverGuid == Settings.SelectedLFEDeviceGuid )
					{
						WriteLine( "...we found the selected recording device..." );

						selectedDeviceInformation = deviceInformation;
					}
				}
			}

			Settings.UpdateLFEDeviceList( lfeDeviceList );

			if ( Settings.LFEToFFBEnabled )
			{
				if ( selectedDeviceInformation != null )
				{
					_lfe_initialized = true;

					WriteLine( "...initializing direct sound capture..." );

					_lfe_directSoundCapture = new DirectSoundCapture( selectedDeviceInformation.DriverGuid );

					WriteLine( "...direct sound capture initialized..." );
					WriteLine( "...initializing capture buffer..." );

					var captureBufferDescription = new CaptureBufferDescription
					{
						Format = new WaveFormat( LFE_CAPTURE_BUFFER_FREQUENCY, LFE_CAPTURE_BUFFER_BITS_PER_SAMPLE, 1 ),
						BufferBytes = LFE_CAPTURE_BUFFER_SIZE_IN_BYTES
					};

					_lfe_captureBuffer = new CaptureBuffer( _lfe_directSoundCapture, captureBufferDescription );

					WriteLine( "...capture buffer initialized..." );
					WriteLine( "...initializing notification positions..." );

					var notificationPositionArray = new NotificationPosition[ LFE_CAPTURE_BUFFER_SAMPLES / ( FFB_SAMPLES_PER_FRAME * LFE_360HZ_TO_8KHZ_SCALE ) ];

					for ( var i = 0; i < notificationPositionArray.Length; i++ )
					{
						notificationPositionArray[ i ] = new()
						{
							Offset = i * LFE_FRAME_SIZE_IN_BYTES,
							WaitHandle = _lfe_autoResetEvent
						};
					}

					_lfe_captureBuffer.SetNotificationPositions( notificationPositionArray );

					WriteLine( "...notification positions initialized..." );
					WriteLine( "...starting capture..." );

					_lfe_captureBuffer.Start( true );

					WriteLine( "...capture started..." );

					WriteLine( "...starting LFE thread..." );

					Task.Run( LFEThread );

					WriteLine( "...LFE thread started..." );
				}
			}
		}

		private void UninitializeLFE()
		{
			WriteLine( "UninitializeLFE called.", true );

			if ( _lfe_threadRunning )
			{
				WriteLine( "...terminating LFE thread..." );

				_lfe_keepThreadAlive = false;

				_lfe_autoResetEvent.Set();

				while ( _lfe_threadRunning )
				{
					Thread.Sleep( 0 );
				}

				WriteLine( "...LFE thread terminated..." );
			}

			if ( _lfe_captureBuffer != null )
			{
				WriteLine( "...uninitializing capture buffer..." );

				_lfe_captureBuffer.Stop();
				_lfe_captureBuffer.Dispose();

				_lfe_captureBuffer = null;

				WriteLine( "...capture buffer uninitialized..." );
			}

			for ( var i = 0; i < FFB_SAMPLES_PER_FRAME; i++ )
			{
				_lfe_magnitude[ 0, i ] = 0;
				_lfe_magnitude[ 1, i ] = 0;
			}

			_lfe_initialized = false;
		}

		private void StopLFE()
		{
			UninitializeLFE();
		}

		private void LFEThread()
		{
			_lfe_keepThreadAlive = true;
			_lfe_threadRunning = true;

			var byteSpan = new Span<byte>( new byte[ LFE_FRAME_SIZE_IN_BYTES ] );

			while ( _lfe_keepThreadAlive )
			{
				var signalReceived = _lfe_autoResetEvent?.WaitOne( 250 ) ?? false;

				if ( signalReceived && _lfe_keepThreadAlive )
				{
					if ( _lfe_captureBuffer != null )
					{
						var currentCapturePosition = _lfe_captureBuffer.CurrentCapturePosition;

						currentCapturePosition = ( currentCapturePosition / LFE_FRAME_SIZE_IN_BYTES ) * LFE_FRAME_SIZE_IN_BYTES;

						var magnitudeIndex = ( _lfe_magnitudeIndex + 1 ) % 2;

						var currentReadPosition = ( currentCapturePosition + LFE_CAPTURE_BUFFER_SIZE_IN_BYTES - LFE_FRAME_SIZE_IN_BYTES ) % LFE_CAPTURE_BUFFER_SIZE_IN_BYTES;

						var dataStream = _lfe_captureBuffer.Lock( currentReadPosition, LFE_FRAME_SIZE_IN_BYTES, LockFlags.None, out var secondPart );

						dataStream.Read( byteSpan );

						var shortSpan = MemoryMarshal.Cast<byte, short>( byteSpan );

						var sampleOffset = 0;

						for ( var i = 0; i < FFB_SAMPLES_PER_FRAME; i++ )
						{
							var amplitudeSum = 0f;

							for ( var j = 0; j < LFE_360HZ_TO_8KHZ_SCALE; j++ )
							{
								amplitudeSum += shortSpan[ sampleOffset ] / (float) short.MinValue;

								sampleOffset++;
							}

							_lfe_magnitude[ magnitudeIndex, i ] = amplitudeSum / LFE_360HZ_TO_8KHZ_SCALE;
						}

						_lfe_captureBuffer.Unlock( dataStream, secondPart );

						_lfe_magnitudeIndex = magnitudeIndex;
					}
				}
			}

			_lfe_threadRunning = false;
		}
	}
}
