
using System.Speech.Synthesis;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly SpeechSynthesizer _voice_speechSynthesizer = new();

		private void InitializeVoice()
		{
			WriteLine( "" );
			WriteLine( "InitializeVoice called." );

			_voice_speechSynthesizer.SetOutputToDefaultAudioDevice();
			_voice_speechSynthesizer.SelectVoiceByHints( VoiceGender.Female, VoiceAge.Adult );

			_voice_speechSynthesizer.Rate = 1;

			UpdateVolume();
		}

		public void Say( string text, bool interrupt = false )
		{
			if ( Settings.EnableSpeechSynthesizer )
			{
				if ( interrupt )
				{
					_voice_speechSynthesizer.Pause();
					_voice_speechSynthesizer.SpeakAsyncCancelAll();
					_voice_speechSynthesizer.Resume();
				}

				_voice_speechSynthesizer.SpeakAsync( text );
			}
		}

		public void UpdateVolume()
		{
			_voice_speechSynthesizer.Volume = Settings.SpeechSynthesizerVolume;
		}
	}
}
