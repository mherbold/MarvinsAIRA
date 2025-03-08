
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

					// update per wheel/car/track/track configuration force feedback settings

					var forceFeedbackSettingsFound = false;

					foreach ( var forceFeedbackSettings in Settings.ForceFeedbackSettingsList )
					{
						if ( ( forceFeedbackSettings.WheelName == _ffb_wheelSaveName ) && ( forceFeedbackSettings.CarName == _car_carSaveName ) && ( forceFeedbackSettings.TrackName == _track_trackSaveName ) && ( forceFeedbackSettings.TrackConfigName == _track_trackConfigSaveName ) )
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
							CarName = _car_carSaveName,
							TrackName = _track_trackSaveName,
							TrackConfigName = _track_trackConfigSaveName,

							OverallScale = Settings.OverallScale,
							DetailScale = Settings.DetailScale,
						};

						Settings.ForceFeedbackSettingsList.Add( forceFeedbackSettings );
					}

					// update per-car steering effects settings

					var steeringEffectsSettingsFound = false;

					foreach ( var steeringEffectsSettings in Settings.SteeringEffectsSettingsList )
					{
						if ( steeringEffectsSettings.CarName == _car_carSaveName )
						{
							steeringEffectsSettings.SteeringEffectsEnabled = Settings.SteeringEffectsEnabled;

							steeringEffectsSettings.USYawRateFactorLeft = Settings.USYawRateFactorLeft;
							steeringEffectsSettings.USYawRateFactorRight = Settings.USYawRateFactorRight;
							steeringEffectsSettings.USTolerance = Settings.USTolerance;

							steeringEffectsSettings.OSYawRateFactorLeft = Settings.OSYawRateFactorLeft;
							steeringEffectsSettings.OSYawRateFactorRight = Settings.OSYawRateFactorRight;
							steeringEffectsSettings.OSTolerance = Settings.OSTolerance;

							steeringEffectsSettingsFound = true;

							break;
						}
					}

					if ( !steeringEffectsSettingsFound )
					{
						var steeringEffectsSettings = new SteeringEffectsSettings
						{
							CarName = _car_carSaveName,

							SteeringEffectsEnabled = Settings.SteeringEffectsEnabled,

							USYawRateFactorLeft = Settings.USYawRateFactorLeft,
							USYawRateFactorRight = Settings.USYawRateFactorRight,
							USTolerance = Settings.USTolerance,

							OSYawRateFactorLeft = Settings.OSYawRateFactorLeft,
							OSYawRateFactorRight = Settings.OSYawRateFactorRight,
							OSTolerance = Settings.OSTolerance
						};

						Settings.SteeringEffectsSettingsList.Add( steeringEffectsSettings );
					}

					// save the configuration file

					var filePath = Path.Combine( DocumentsFolder, "Settings.xml" );

					Serializer.Save( filePath, _settings );

					WriteLine( "Settings.xml file updated." );
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
