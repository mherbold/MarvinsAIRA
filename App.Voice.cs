
using System.Speech.Synthesis;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly SpeechSynthesizer _speechSynthesizer = new();

		private void InitializeVoice()
		{
			WriteLine( "" );
			WriteLine( "InitializeVoice called." );

			_speechSynthesizer.SetOutputToDefaultAudioDevice();
			_speechSynthesizer.SelectVoiceByHints( VoiceGender.Female, VoiceAge.Adult );

			_speechSynthesizer.Rate = 1;

			UpdateVolume();
		}

		public void Say( string text, bool interrupt = false )
		{
			if ( Settings.EnableSpeechSynthesizer )
			{
				if ( interrupt )
				{
					_speechSynthesizer.Pause();
					_speechSynthesizer.SpeakAsyncCancelAll();
					_speechSynthesizer.Resume();
				}

				_speechSynthesizer.SpeakAsync( text );
			}
		}

		public void UpdateVolume()
		{
			_speechSynthesizer.Volume = Settings.SpeechSynthesizerVolume;
		}
	}
}
