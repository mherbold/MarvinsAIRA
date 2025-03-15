
using System.Windows;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly WaveOutEvent _sounds_clickWaveOutEvent = new();
		private WaveStream? _sounds_clickWaveStream;
		private VolumeSampleProvider? _sounds_clickVolumeSampleProvider;

		private readonly WaveOutEvent _sounds_absWaveOutEvent = new();
		private LoopStream? _sounds_absLoopStream;
		private SmbPitchShiftingSampleProvider? _sounds_absPitchShiftingSampleProvider;
		private VolumeSampleProvider? _sounds_absVolumeSampleProvider;

		private bool _sounds_initialized = false;

		private void InitializeSounds()
		{
			WriteLine( "InitializeSounds called.", true );

			_sounds_initialized = false;

			WriteLine( "...loading click sound..." );

			var resourceStream = GetResourceStream( new Uri( "pack://application:,,,/click.wav" ) );

			_sounds_clickWaveStream = new WaveFileReader( resourceStream.Stream );

			_sounds_clickVolumeSampleProvider = new VolumeSampleProvider( _sounds_clickWaveStream.ToSampleProvider() );

			WriteLine( "...click sound loaded..." );

			try
			{
				WriteLine( "...initializing click wave out event..." );

				_sounds_clickWaveOutEvent.Init( _sounds_clickVolumeSampleProvider );

				_sounds_clickWaveOutEvent.Volume = 1;

				WriteLine( "...click wave out event initialized." );
			}
			catch ( Exception exception )
			{
				WriteLine( $"Failed to create click wave out event: {exception.Message.Trim()}" );
			}

			WriteLine( "...loading ABS sound..." );

			resourceStream = GetResourceStream( new Uri( "pack://application:,,,/abs.wav" ) );

			_sounds_absLoopStream = new LoopStream( new WaveFileReader( resourceStream.Stream ) );

			_sounds_absPitchShiftingSampleProvider = new SmbPitchShiftingSampleProvider( _sounds_absLoopStream.ToSampleProvider() );

			_sounds_absVolumeSampleProvider = new VolumeSampleProvider( _sounds_absPitchShiftingSampleProvider );

			WriteLine( "...ABS sound loaded..." );

			try
			{
				WriteLine( "...initializing ABS wave out event..." );

				_sounds_absWaveOutEvent.Init( _sounds_absVolumeSampleProvider );

				_sounds_absWaveOutEvent.Volume = 1;

				WriteLine( "...ABS wave out event initialized." );
			}
			catch ( Exception exception )
			{
				WriteLine( $"Failed to create click wave out event: {exception.Message.Trim()}" );
			}

			_sounds_initialized = true;
		}

		public void PlayClick()
		{
			if ( _sounds_initialized )
			{
				if ( Settings.EnableClickSound )
				{
					if ( ( _sounds_clickWaveStream != null ) && ( _sounds_clickVolumeSampleProvider != null ) )
					{
						_sounds_clickWaveStream.Seek( 0, System.IO.SeekOrigin.Begin );

						_sounds_clickVolumeSampleProvider.Volume = Settings.ClickSoundVolume / 100f;

						_sounds_clickWaveOutEvent.Play();
					}
				}
			}
		}

		public void PlayABS()
		{
			if ( _sounds_initialized )
			{
				if ( Settings.EnableABSSound )
				{
					if ( ( _sounds_absLoopStream != null ) && ( _sounds_absPitchShiftingSampleProvider != null ) && ( _sounds_absVolumeSampleProvider != null ) )
					{
						_sounds_absLoopStream.Seek( 0, System.IO.SeekOrigin.Begin );

						_sounds_absVolumeSampleProvider.Volume = Settings.ABSSoundVolume / 100f;
						_sounds_absPitchShiftingSampleProvider.PitchFactor = Settings.ABSSoundPitch;

						_sounds_absWaveOutEvent.Play();
					}
				}
			}
		}

		public void StopABS()
		{
			if ( _sounds_initialized )
			{
				if ( ( _sounds_absLoopStream != null ) && ( _sounds_absPitchShiftingSampleProvider != null ) )
				{
					_sounds_absWaveOutEvent.Stop();
				}
			}
		}
	}
}
