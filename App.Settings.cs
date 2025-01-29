
using System.IO;
using System.Windows;

using static MarvinsAIRA.Settings;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private Settings _settings = new();

		private bool _pauseSerialization = false;
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
				_pauseSerialization = true;

				var settings = (Settings?) Serializer.Load( filePath, typeof( Settings ) );

				if ( settings != null )
				{
					_settings = settings;
				}

				_pauseSerialization = false;
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

					WriteLine( "" );
					WriteLine( $"Saving configuration [{_ffb_wheelSaveName}, {_carSaveName}, {_trackSaveName}, {_trackConfigSaveName}]" );

					var forceFeedbackSettingsFound = false;

					foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
					{
						if ( ( forceFeedbackSettings.WheelName == _ffb_wheelSaveName ) && ( forceFeedbackSettings.CarName == _carSaveName ) && ( forceFeedbackSettings.TrackName == _trackSaveName ) && ( forceFeedbackSettings.TrackConfigName == _trackConfigSaveName ) )
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
							WheelName = _ffb_wheelSaveName,
							CarName = _carSaveName,
							TrackName = _trackSaveName,
							TrackConfigName = _trackConfigSaveName,
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
			if ( !_pauseSerialization )
			{
				_serializationTimer = 1;
			}
		}
	}
}
