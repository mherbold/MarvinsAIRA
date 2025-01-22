
using System.Runtime.InteropServices;
using System.Windows;

using NAudio.Wave;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private bool _lfe_initialized = false;
		private int _lfe_selectedDeviceIndex = -1;
		private WaveInEvent? _lfe_waveInEvent;
		private float[,] _lfe_magnitude = new float[ 2, FFB_SAMPLES_PER_FRAME ];
		private int _lfe_magnitudeIndex = 0;

		public void InitializeLFE()
		{
			WriteLine( "" );
			WriteLine( "InitializeLFE called." );

			if ( _lfe_initialized )
			{
				UninitializeLFE();
			}

			WriteLine( "...enumerating recording devices (for LFE)..." );

			SerializableDictionary<Guid, string> lfeDeviceList = [];

			for ( var deviceIndex = 0; deviceIndex < WaveIn.DeviceCount; deviceIndex++ )
			{
				var deviceInfo = WaveIn.GetCapabilities( deviceIndex );

				WriteLine( $"...we found the {deviceInfo.ProductName} device..." );

				lfeDeviceList.Add( deviceInfo.ProductGuid, deviceInfo.ProductName );

				if ( deviceInfo.ProductGuid == Settings.SelectedLFEDeviceGuid )
				{
					WriteLine( "...we found the selected recording device..." );
					WriteLine( "...we are good to go with this LFE device." );

					_lfe_initialized = true;
					_lfe_selectedDeviceIndex = deviceIndex;

					ReinitializeLFE();
				}
			}

			Settings.UpdateLFEDeviceList( lfeDeviceList );
		}

		private void UninitializeLFE()
		{
			WriteLine( "" );
			WriteLine( "UninitializeLFE called." );

			if ( _lfe_waveInEvent != null )
			{
				_lfe_waveInEvent.StopRecording();
				_lfe_waveInEvent.Dispose();

				_lfe_waveInEvent = null;
			}

			for ( var i = 0; i < _lfe_magnitude.Length; i++ )
			{
				_lfe_magnitude[ 0, i ] = 0;
				_lfe_magnitude[ 1, i ] = 0;
			}

			_lfe_initialized = false;
		}

		private void ReinitializeLFE()
		{
			WriteLine( "" );
			WriteLine( "ReinitializeLFE called." );

			if ( !_lfe_initialized )
			{
				WriteLine( "...the LFE system has faulted, we will not attempt to reinitialize the LFE device." );
				return;
			}

			if ( !Settings.ForceFeedbackEnabled )
			{
				WriteLine( "...force feedback has been disabled, we will not attempt to reinitialize the LFE device." );
				return;
			}

			_lfe_waveInEvent = new WaveInEvent
			{
				DeviceNumber = _lfe_selectedDeviceIndex,
				WaveFormat = new WaveFormat( 360, 16, 1 ),
				BufferMilliseconds = 17,
				NumberOfBuffers = 2
			};

			_lfe_waveInEvent.DataAvailable += LFEDataAvailable;

			_lfe_waveInEvent.StartRecording();
		}

		private void StopLFE()
		{
			UninitializeLFE();
		}

		private void LFEDataAvailable( object? sender, WaveInEventArgs waveInEventArgs )
		{
			var sbyteSpan = MemoryMarshal.Cast<byte, short>( waveInEventArgs.Buffer.AsSpan() );

			var writeMagnitudeIndex = ( _lfe_magnitudeIndex + 1 ) % 2;

			for ( var i = 0; i < FFB_SAMPLES_PER_FRAME; i++ )
			{
				_lfe_magnitude[ writeMagnitudeIndex, i ] = sbyteSpan[ i ] / (float) short.MinValue;
			}

			_lfe_magnitudeIndex = writeMagnitudeIndex;
		}
	}
}
