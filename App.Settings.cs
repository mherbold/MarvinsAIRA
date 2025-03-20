
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
			WriteLine( "InitializeSettings called.", true );

			var filePath = Path.Combine( DocumentsFolder, "Settings.xml" );

			if ( File.Exists( filePath ) )
			{
				_settings_pauseSerialization = true;

				var settings = (Settings?) Serializer.Load( filePath, typeof( Settings ) );

				if ( settings != null )
				{
					// START TEMPORARY CODE - GET RID OF THIS AFTER ABOUT A MONTH
					if ( ( _settings.USEffectStyle == 2 ) && _settings.USEffectStyleInvert )
					{
						_settings.USEffectStyleConstantForce = false;
						_settings.USEffectStyleConstantForceInverted = true;
						_settings.USEffectStyle = 3;
					}

					_settings.USEffectStyleInvert = false;

					if ( ( _settings.OSEffectStyle == 2 ) && _settings.OSEffectStyleInvert )
					{
						_settings.OSEffectStyleConstantForce = false;
						_settings.OSEffectStyleConstantForceInverted = true;
						_settings.OSEffectStyle = 3;
					}

					_settings.USEffectStyleInvert = false;
					// END TEMPORARY CODE - GET RID OF THIS AFTER ABOUT A MONTH

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

							steeringEffectsSettings.USStartYawRateFactorLeft = Settings.USStartYawRateFactorLeft;
							steeringEffectsSettings.USEndYawRateFactorLeft = Settings.USEndYawRateFactorLeft;
							steeringEffectsSettings.USStartYawRateFactorRight = Settings.USStartYawRateFactorRight;
							steeringEffectsSettings.USEndYawRateFactorRight = Settings.USEndYawRateFactorRight;

							steeringEffectsSettings.OSStartYVelocity = Settings.OSStartYVelocity;
							steeringEffectsSettings.OSEndYVelocity = Settings.OSEndYVelocity;

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

							USStartYawRateFactorLeft = Settings.USStartYawRateFactorLeft,
							USEndYawRateFactorLeft = Settings.USEndYawRateFactorLeft,
							USStartYawRateFactorRight = Settings.USStartYawRateFactorRight,
							USEndYawRateFactorRight = Settings.USEndYawRateFactorRight,

							OSStartYVelocity = Settings.OSStartYVelocity,
							OSEndYVelocity = Settings.OSEndYVelocity,
						};

						Settings.SteeringEffectsSettingsList.Add( steeringEffectsSettings );
					}

					// save the configuration file

					var filePath = Path.Combine( DocumentsFolder, "Settings.xml" );

					Serializer.Save( filePath, _settings );

					WriteLine( $"Settings.xml file updated [{_ffb_wheelSaveName}, {_car_carSaveName}, {_track_trackSaveName}, {_track_trackConfigSaveName}, {_wetdry_conditionSaveName}]" );
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
