
using System.Windows;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const string NO_TRACK_DISPLAY_NAME = "No Track";
		private const string ALL_TRACKS_SAVE_NAME = "All";

		private const string NO_TRACK_CONFIG_NAME = "No Track Configuration";
		private const string ALL_TRACK_CONFIGS_SAVE_NAME = "All";

		private string _track_currentTrackDisplayName = NO_TRACK_DISPLAY_NAME;
		private string _track_trackSaveName = ALL_TRACKS_SAVE_NAME;

		private string _track_currentTrackConfigName = NO_TRACK_CONFIG_NAME;
		private string _track_trackConfigSaveName = ALL_TRACK_CONFIGS_SAVE_NAME;

		private bool _track_trackChanged = false;
		private bool _track_trackConfigChanged = false;

		private void InitializeCurrentTrack()
		{
			WriteLine( "InitializeCurrentTrack called.", true );

			UpdateCurrentTrack();
		}

		private void UpdateCurrentTrack()
		{
			var trackDisplayName = NO_TRACK_DISPLAY_NAME;
			var trackConfigName = NO_TRACK_CONFIG_NAME;

			if ( _irsdk.IsConnected && ( _irsdk.Data.SessionInfo != null ) )
			{
				trackDisplayName = _irsdk.Data.SessionInfo.WeekendInfo.TrackDisplayName ?? string.Empty;
				trackConfigName = _irsdk.Data.SessionInfo.WeekendInfo.TrackConfigName ?? string.Empty;
			}

			if ( ( _track_currentTrackDisplayName != trackDisplayName ) || ( _track_currentTrackConfigName != trackConfigName ) )
			{
				if ( trackDisplayName == NO_TRACK_DISPLAY_NAME )
				{
					WriteLine( "You are no longer on a track.", true );
				}
				else
				{
					WriteLine( $"You are racing at {trackDisplayName} ({trackConfigName}).", true );

					Say( Settings.SayTrackName, trackDisplayName );
					Say( Settings.SayTrackConfigName, trackConfigName );
				}

				_track_currentTrackDisplayName = trackDisplayName;

				UpdateTrackSaveName();

				_track_currentTrackConfigName = trackConfigName;

				UpdateTrackConfigSaveName();

				Dispatcher.BeginInvoke( () =>
				{
					var mainWindow = MarvinsAIRA.MainWindow.Instance;

					if ( mainWindow != null )
					{
						if ( _track_currentTrackConfigName != string.Empty )
						{
							mainWindow.CurrentTrack_StatusBarItem.Content = $"{_track_currentTrackDisplayName} ({_track_currentTrackConfigName})";
						}
						else
						{
							mainWindow.CurrentTrack_StatusBarItem.Content = $"{_track_currentTrackDisplayName}";
						}

						mainWindow.CurrentTrack_StatusBarItem.Foreground = ( _track_currentTrackDisplayName == NO_TRACK_DISPLAY_NAME ) ? Brushes.Gray : Brushes.ForestGreen;
					}
				} );
			}
		}

		public void UpdateTrackSaveName()
		{
			var oldTrackSaveName = _track_trackSaveName;

			_track_trackSaveName = Settings.SaveSettingsPerTrack ? _track_currentTrackDisplayName : ALL_TRACKS_SAVE_NAME;

			if ( _track_trackSaveName != oldTrackSaveName )
			{
				_track_trackChanged = true;
			}
		}

		public void UpdateTrackConfigSaveName()
		{
			var oldTrackConfigSaveName = _track_trackConfigSaveName;

			_track_trackConfigSaveName = Settings.SaveSettingsPerTrackConfig ? _track_currentTrackConfigName : ALL_TRACK_CONFIGS_SAVE_NAME;

			if ( _track_trackConfigSaveName != oldTrackConfigSaveName )
			{
				_track_trackConfigChanged = true;
			}
		}
	}
}
