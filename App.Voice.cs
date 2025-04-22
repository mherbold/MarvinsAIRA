
using System.Speech.Synthesis;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private SpeechSynthesizer? _voice_speechSynthesizer = null;

		public void InitializeVoice()
		{
			WriteLine( "InitializeVoice called.", true );

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
			var firstInstalledVoice = string.Empty;
			var selectedVoiceFound = false;

			SerializableDictionary<string, string> voiceList = [];

			foreach ( var installedVoice in installedVoices )
			{
				if ( !voiceList.ContainsKey( installedVoice.VoiceInfo.Name ) )
				{
					voiceList.Add( installedVoice.VoiceInfo.Name, installedVoice.VoiceInfo.Description );

					if ( firstInstalledVoice == string.Empty )
					{
						firstInstalledVoice = installedVoice.VoiceInfo.Name;
					}

					if ( Settings.SelectedVoice == installedVoice.VoiceInfo.Name )
					{
						selectedVoiceFound = true;
					}
				}
			}

			if ( !selectedVoiceFound )
			{
				Settings.SelectedVoice = firstInstalledVoice;
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
			message ??= string.Empty;

			if ( value != null )
			{
				if ( value == string.Empty )
				{
					return;
				}

				message = message.Replace( ":value:", value );
			}

			if ( message == string.Empty )
			{
				return;
			}

			if ( Settings.EnableSpeechSynthesizer && ( _voice_speechSynthesizer != null ) )
			{
				if ( interrupt )
				{
					_voice_speechSynthesizer.Pause();
					_voice_speechSynthesizer.SpeakAsyncCancelAll();
					_voice_speechSynthesizer.Resume();
				}

				_voice_speechSynthesizer.SpeakAsync( message );
			}

			if ( alsoAddToChatQueue )
			{
				Chat( message );
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
