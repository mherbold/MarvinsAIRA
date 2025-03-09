
using System.Speech.Synthesis;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private SpeechSynthesizer? _voice_speechSynthesizer = null;

		public void InitializeVoice()
		{
			WriteLine( "" );
			WriteLine( "InitializeVoice called." );

			if ( _voice_speechSynthesizer != null )
			{
				WriteLine( "...disposing of old voice syntehsizer..." );

				_voice_speechSynthesizer.Pause();
				_voice_speechSynthesizer.SpeakAsyncCancelAll();
				_voice_speechSynthesizer.Dispose();

				_voice_speechSynthesizer = null;

				WriteLine( "...old voice syntehsizer has been disposed..." );
			}

			WriteLine( "...creating new voice synthesizer..." );

			_voice_speechSynthesizer = new();

			try
			{
				SpeechApiReflectionHelper.InjectOneCoreVoices( _voice_speechSynthesizer );
			}
			catch ( Exception )
			{
				WriteLine( "...note - exception thrown while trying to inject one core voices into the speech synthesizer..." );
			}

			var installedVoices = _voice_speechSynthesizer.GetInstalledVoices();

			SerializableDictionary<string, string> voiceList = [];

			foreach ( var installedVoice in installedVoices )
			{
				if ( !voiceList.ContainsKey( installedVoice.VoiceInfo.Name ) )
				{
					voiceList.Add( installedVoice.VoiceInfo.Name, installedVoice.VoiceInfo.Description );

					if ( Settings.SelectedVoice == string.Empty )
					{
						Settings.SelectedVoice = installedVoice.VoiceInfo.Name;
					}
				}
			}

			Settings.UpdateVoiceList( voiceList );

			_voice_speechSynthesizer.SetOutputToDefaultAudioDevice();

			if ( Settings.SelectedVoice != string.Empty )
			{
				_voice_speechSynthesizer.SelectVoice( Settings.SelectedVoice );
			}

			_voice_speechSynthesizer.Rate = 1;

			WriteLine( "...voice synthesizer has been created." );

			UpdateVolume();
		}

		public void Say( string message, string? value = null, bool interrupt = false, bool alsoAddToChatQueue = true )
		{
			if ( Settings.EnableSpeechSynthesizer && ( _voice_speechSynthesizer != null ) )
			{
				if ( value != null )
				{
					if ( value == "" )
					{
						return;
					}

					message = message.Replace( ":value:", value );
				}

				if ( message == string.Empty )
				{
					return;
				}

				if ( interrupt )
				{
					_voice_speechSynthesizer.Pause();
					_voice_speechSynthesizer.SpeakAsyncCancelAll();
					_voice_speechSynthesizer.Resume();
				}

				_voice_speechSynthesizer.SpeakAsync( message );

				if ( alsoAddToChatQueue )
				{
					Chat( message );
				}
			}
		}

		public void UpdateVolume()
		{
			if ( _voice_speechSynthesizer != null )
			{
				_voice_speechSynthesizer.Volume = Settings.SpeechSynthesizerVolume;
			}
		}
	}
}
