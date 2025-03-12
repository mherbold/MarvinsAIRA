
using System.Windows;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly WaveOutEvent _sounds_clickWaveOutEvent = new();

		private WaveStream? _sounds_clickWaveStream;
		private VolumeSampleProvider? _sounds_volumeSampleProvider;
		private bool _sounds_initialized = false;

		private void InitializeSounds()
		{
			WriteLine( "InitializeSounds called.", true );

			_sounds_initialized = false;

			WriteLine( "...loading click sound..." );

			var resourceStream = GetResourceStream( new Uri( "pack://application:,,,/click.wav" ) );

			_sounds_clickWaveStream = new WaveFileReader( resourceStream.Stream );

			_sounds_volumeSampleProvider = new VolumeSampleProvider( _sounds_clickWaveStream.ToSampleProvider() );

			WriteLine( "...click sound loaded..." );

			try
			{
				WriteLine( "...initializing wave out event..." );

				_sounds_clickWaveOutEvent.Init( _sounds_volumeSampleProvider );

				_sounds_clickWaveOutEvent.Volume = 1;

				WriteLine( "...wave out event initialized." );

				_sounds_initialized = true;
			}
			catch ( Exception exception )
			{
				WriteLine( $"Failed to create wave out event: {exception.Message.Trim()}" );
			}
		}

		public void PlayClick()
		{
			if ( Settings.EnableClickSound && _sounds_initialized && ( _sounds_clickWaveStream != null ) && ( _sounds_volumeSampleProvider != null ) )
			{
				_sounds_clickWaveStream.Seek( 0, System.IO.SeekOrigin.Begin );

				_sounds_volumeSampleProvider.Volume = Settings.ClickSoundVolume / 100f;

				_sounds_clickWaveOutEvent.Play();
			}
		}
	}
}
