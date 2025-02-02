
using System.IO;
using System.Windows;

using static MarvinsAIRA.Settings;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private Settings _settings = new();

		private bool _settings_pauseSerialization = false;
		private float _settings_serializationTimer = 0;

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
				_settings_pauseSerialization = true;

				var settings = (Settings?) Serializer.Load( filePath, typeof( Settings ) );

				if ( settings != null )
				{
					_settings = settings;
				}

				_settings_pauseSerialization = false;
			}

			var mainWindow = MarvinsAIRA.MainWindow.Instance;

			if ( mainWindow != null )
			{
				mainWindow.DataContext = _settings;
			}
		}

		public void UpdateSettings( float deltaTime )
		{
			if ( _settings_serializationTimer > 0 )
			{
				_settings_serializationTimer -= deltaTime;

				if ( _settings_serializationTimer <= 0 )
				{
					_settings_serializationTimer = 0;

					WriteLine( "" );
					WriteLine( $"Saving configuration [{_ffb_wheelSaveName}, {_car_carSaveName}, {_track_trackSaveName}, {_track_trackConfigSaveName}]" );

					var forceFeedbackSettingsFound = false;

					foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
					{
						if ( ( forceFeedbackSettings.WheelName == _ffb_wheelSaveName ) && ( forceFeedbackSettings.CarName == _car_carSaveName ) && ( forceFeedbackSettings.TrackName == _track_trackSaveName ) && ( forceFeedbackSettings.TrackConfigName == _track_trackConfigSaveName ) )
						{
							forceFeedbackSettings.OverallScale = Settings.OverallScale;
							forceFeedbackSettings.DetailScale = Settings.DetailScale;

							forceFeedbackSettings.USEffectStrength = Settings.USEffectStrength;
							forceFeedbackSettings.USYawRateFactor = Settings.USYawRateFactor;
							forceFeedbackSettings.USLateralForceFactor = Settings.USLateralForceFactor;
							forceFeedbackSettings.USSteeringWheelOffset = Settings.USSteeringWheelOffset;

							forceFeedbackSettingsFound = true;

							break;
						}
					}

					if ( !forceFeedbackSettingsFound )
					{
						var forceFeedbackSettings = new ForceFeedbackSettings
						{
							WheelName = _ffb_wheelSaveName,
							CarName = _car_carSaveName,
							TrackName = _track_trackSaveName,
							TrackConfigName = _track_trackConfigSaveName,

							OverallScale = Settings.OverallScale,
							DetailScale = Settings.DetailScale,

							USEffectStrength = Settings.USEffectStrength,
							USYawRateFactor = Settings.USYawRateFactor,
							USLateralForceFactor = Settings.USLateralForceFactor,
							USSteeringWheelOffset= Settings.USSteeringWheelOffset
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
			if ( !_settings_pauseSerialization )
			{
				_settings_serializationTimer = 1;
			}
		}
	}
}
