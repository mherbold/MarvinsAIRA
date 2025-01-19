
using System.Windows;

using NAudio.Wave;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly WaveOutEvent _sounds_clickWaveOutEvent = new();

		private WaveStream? _sounds_clickWaveStream;

		private void InitializeSounds()
		{
			WriteLine( "" );
			WriteLine( "InitializeSounds called." );

			var resourceStream = GetResourceStream( new Uri( "pack://application:,,,/click.wav" ) );

			_sounds_clickWaveStream = new WaveFileReader( resourceStream.Stream );

			_sounds_clickWaveOutEvent.Init( _sounds_clickWaveStream );
		}

		public void PlayClick()
		{
			if ( Settings.EnableClickSound )
			{
				_sounds_clickWaveStream?.Seek( 0, System.IO.SeekOrigin.Begin );

				_sounds_clickWaveOutEvent.Volume = Settings.ClickSoundVolume / 100f;

				_sounds_clickWaveOutEvent.Play();
			}
		}
	}
}
