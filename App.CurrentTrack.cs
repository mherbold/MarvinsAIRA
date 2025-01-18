
using System.Windows;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private const string NO_TRACK_DISPLAY_NAME = "No Track";
		private const string ALL_TRACKS_DISPLAY_NAME = "All";

		private const string NO_TRACK_CONFIG_NAME = "No Track Configuration";
		private const string ALL_TRACK_CONFIGS_DISPLAY_NAME = "All";

		private string _currentTrackDisplayName = NO_TRACK_DISPLAY_NAME;
		private string _trackSaveName = ALL_TRACKS_DISPLAY_NAME;

		private string _currentTrackConfigName = NO_TRACK_CONFIG_NAME;
		private string _trackConfigSaveName = ALL_TRACK_CONFIGS_DISPLAY_NAME;

		private bool _trackChanged = false;
		private bool _trackConfigChanged = false;

		private void InitializeCurrentTrack()
		{
			WriteLine( "" );
			WriteLine( "InitializeCurrentTrack called." );

			UpdateCurrentTrack();
		}

		private void UpdateCurrentTrack()
		{
			var trackDisplayName = NO_TRACK_DISPLAY_NAME;
			var trackConfigName = NO_TRACK_CONFIG_NAME;

			if ( _irsdk.IsConnected && ( _irsdk.Data.SessionInfo != null ) )
			{
				trackDisplayName = _irsdk.Data.SessionInfo.WeekendInfo.TrackDisplayName;
				trackConfigName = _irsdk.Data.SessionInfo.WeekendInfo.TrackConfigName;
			}

			if ( ( _currentTrackDisplayName != trackDisplayName ) || ( _currentTrackConfigName != trackConfigName ) )
			{
				if ( trackDisplayName == NO_TRACK_DISPLAY_NAME )
				{
					WriteLine( "" );
					WriteLine( "You are no longer on a track." );
				}
				else
				{
					WriteLine( "" );
					WriteLine( $"You are racing at {trackDisplayName} ({trackConfigName})." );

					Say( $"You are racing at {trackDisplayName} ({trackConfigName})." );

					_trackChanged = ( _currentTrackDisplayName != trackDisplayName );
					_trackConfigChanged = ( _currentTrackConfigName != trackConfigName );
				}

				_currentTrackDisplayName = trackDisplayName;
				_trackSaveName = Settings.SaveSettingsPerTrack ? _currentTrackDisplayName : ALL_TRACKS_DISPLAY_NAME;

				_currentTrackConfigName = trackConfigName;
				_trackConfigSaveName = Settings.SaveSettingsPerTrackConfig ? _currentTrackConfigName : ALL_TRACK_CONFIGS_DISPLAY_NAME;

				Dispatcher.BeginInvoke( () =>
				{
					var mainWindow = (MainWindow) MainWindow;

					if ( mainWindow != null )
					{
						mainWindow.CurrentTrackStatusBarItem.Content = $"{_currentTrackDisplayName} ({_currentTrackConfigName})";
						mainWindow.CurrentTrackStatusBarItem.Foreground = ( _currentTrackDisplayName == NO_TRACK_DISPLAY_NAME ) ? Brushes.Gray : Brushes.ForestGreen;					}
				} );
			}
		}
	}
}
