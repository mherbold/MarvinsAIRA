
using System.IO;
using System.Windows;

using static MarvinsAIRA.Settings;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private Settings _settings = new();

		private float _serializationTimer = 0;

		public Settings Settings
		{
			get => _settings;
		}

		private void InitializeSettings()
		{
			WriteLine( "" );
			WriteLine( "InitializeSettings called." );

			var filePath = Path.Combine( DocumentsFolder, "Settings.xml" );

			if ( File.Exists( filePath ) )
			{
				var settings = (Settings?) Serializer.Load( filePath, typeof( Settings ) );

				if ( settings != null )
				{
					_settings = settings;
				}
			}

			var mainWindow = (MainWindow) MainWindow;

			if ( mainWindow != null )
			{
				mainWindow.DataContext = _settings;
			}
		}

		public void UpdateSettings( float deltaTime )
		{
			if ( _serializationTimer > 0 )
			{
				_serializationTimer -= deltaTime;

				if ( _serializationTimer <= 0 )
				{
					_serializationTimer = 0;

					var forceFeedbackSettingsFound = false;

					foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
					{
						if ( ( forceFeedbackSettings.CarScreenName == _currentCarScreenName ) && ( forceFeedbackSettings.TrackDisplayName == _currentTrackDisplayName ) && ( forceFeedbackSettings.TrackConfigName == _currentTrackConfigName ) )
						{
							forceFeedbackSettings.OverallScale = Settings.OverallScale;
							forceFeedbackSettings.DetailScale = Settings.DetailScale;

							forceFeedbackSettingsFound = true;

							break;
						}
					}

					if ( !forceFeedbackSettingsFound )
					{
						var forceFeedbackSettings = new ForceFeedbackSettings
						{
							CarScreenName = _currentCarScreenName,
							TrackDisplayName = _currentTrackDisplayName,
							TrackConfigName = _currentTrackConfigName,
							OverallScale = Settings.OverallScale,
							DetailScale = Settings.DetailScale,
						};

						Settings.ForceFeedbackSettingsList.Add( forceFeedbackSettings );
					}

					var filePath = Path.Combine( DocumentsFolder, "Settings.xml" );

					Serializer.Save( filePath, _settings );
				}
			}
		}

		public void QueueForSerialization()
		{
			_serializationTimer = 1;
		}
	}
}
