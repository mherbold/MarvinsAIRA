
using System.Windows;

using NAudio.Wave;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly WaveOutEvent _sounds_clickWaveOutEvent = new();

		private WaveStream? _sounds_clickWaveStream;
		private bool _sounds_initialized = false;

		private void InitializeSounds()
		{
			WriteLine( "" );
			WriteLine( "InitializeSounds called." );

			_sounds_initialized = false;

			WriteLine( "...loading click sound..." );

			var resourceStream = GetResourceStream( new Uri( "pack://application:,,,/click.wav" ) );

			_sounds_clickWaveStream = new WaveFileReader( resourceStream.Stream );

			WriteLine( "...click sound loaded..." );

			try
			{
				WriteLine( "...initializing wave out event..." );

				_sounds_clickWaveOutEvent.Init( _sounds_clickWaveStream );

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
			if ( Settings.EnableClickSound && _sounds_initialized )
			{
				_sounds_clickWaveStream?.Seek( 0, System.IO.SeekOrigin.Begin );

				_sounds_clickWaveOutEvent.Volume = Settings.ClickSoundVolume / 100f;

				_sounds_clickWaveOutEvent.Play();
			}
		}
	}
}
