
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

		public void InitializeLFE()
		{
			WriteLine( "" );
			WriteLine( "InitializeLFE called." );

			if ( _lfe_initialized )
			{
				UninitializeLFE();
			}

			WriteLine( "...enumerating recording devices (for LFE)..." );

			var deviceInformationList = DirectSoundCapture.GetDevices();

			SerializableDictionary<Guid, string> lfeDeviceList = [];

			lfeDeviceList.Add( Guid.Empty, "Disable LFE to FFB" );

			foreach ( var deviceInformation in deviceInformationList )
			{
				if ( deviceInformation.DriverGuid != Guid.Empty )
				{
					WriteLine( $"...we found the {deviceInformation.Description} device..." );

					lfeDeviceList.Add( deviceInformation.DriverGuid, deviceInformation.Description );

					if ( deviceInformation.DriverGuid == Settings.SelectedLFEDeviceGuid )
					{
						WriteLine( "...we found the selected recording device..." );
						WriteLine( "...we are good to go with this LFE device." );

						_lfe_initialized = true;
						_lfe_directSoundCapture = new DirectSoundCapture( deviceInformation.DriverGuid );

						var captureBufferDescription = new CaptureBufferDescription
						{
							Format = new WaveFormat( 360, 16, 1 ),
							BufferBytes = 24
						};

						_lfe_captureBuffer = new CaptureBuffer( _lfe_directSoundCapture, captureBufferDescription );

						var notificationPositionArray = new NotificationPosition[]
						{
						new()
						{
							Offset = 0,
							WaitHandle = _lfe_autoResetEvent
						},
						new()
						{
							Offset = 12,
							WaitHandle = _lfe_autoResetEvent
						}
						};

						_lfe_captureBuffer.SetNotificationPositions( notificationPositionArray );
						_lfe_captureBuffer.Start( true );

						Task.Run( LFEThread );
					}
				}
			}

			Settings.UpdateLFEDeviceList( lfeDeviceList );
		}

		private void UninitializeLFE()
		{
			WriteLine( "" );
			WriteLine( "UninitializeLFE called." );

			WriteLine( "...terminating LFE thread..." );

			_lfe_keepThreadAlive = false;

			_lfe_autoResetEvent.Set();

			while ( _lfe_threadRunning )
			{
				Thread.Sleep( 0 );
			}

			WriteLine( "...LFE thread terminated..." );
			WriteLine( "...uninitializing capture buffer..." );

			_lfe_captureBuffer?.Stop();
			_lfe_captureBuffer?.Dispose();

			_lfe_captureBuffer = null;

			WriteLine( "...capture buffer uninitialized." );

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

			var byteSpan = new Span<byte>( new byte[ 12 ] );

			while ( _lfe_keepThreadAlive )
			{
				var signalReceived = _lfe_autoResetEvent?.WaitOne( 250 ) ?? false;

				if ( signalReceived && _lfe_keepThreadAlive )
				{
					if ( _lfe_captureBuffer != null )
					{
						var currentCapturePosition = _lfe_captureBuffer.CurrentCapturePosition;

						var magnitudeIndex = ( currentCapturePosition >= 12 ) ? 0 : 1;

						var dataStream = _lfe_captureBuffer.Lock( magnitudeIndex * 12, 12, LockFlags.None, out var secondPart );

						dataStream.Read( byteSpan );

						var shortSpan = MemoryMarshal.Cast<byte, short>( byteSpan );

						for ( var i = 0; i < FFB_SAMPLES_PER_FRAME; i++ )
						{
							_lfe_magnitude[ magnitudeIndex, i ] = shortSpan[ i ] / (float) short.MinValue;
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
