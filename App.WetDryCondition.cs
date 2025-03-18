
using System.Windows;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		public const string ALL_WETDRYCONDITIONS_SAVE_NAME = "All";

		private const string WET_CONDITION_DISPLAY_NAME = "Wet";
		private const string DRY_CONDITION_DISPLAY_NAME = "Dry";

		private string _wetdry_currentConditionDisplayName = string.Empty;
		private string _wetdry_conditionSaveName = ALL_WETDRYCONDITIONS_SAVE_NAME;

		private bool _wetdry_conditionChanged = false;

		private void InitializeWetDryCondition()
		{
			WriteLine( "InitializeWetDryCondition called.", true );

			UpdateCurrentWetDryCondition();
		}

		private void UpdateCurrentWetDryCondition()
		{
			var wetDryConditionDisplayName = string.Empty;

			if ( _irsdk.IsConnected )
			{
				wetDryConditionDisplayName = _irsdk_weatherDeclaredWet ? WET_CONDITION_DISPLAY_NAME : DRY_CONDITION_DISPLAY_NAME;
			}

			if ( _wetdry_currentConditionDisplayName != wetDryConditionDisplayName )
			{
				if ( wetDryConditionDisplayName != string.Empty )
				{
					WriteLine( $"The track is {wetDryConditionDisplayName}.", true );

					if ( wetDryConditionDisplayName == WET_CONDITION_DISPLAY_NAME )
					{
						Say( Settings.SayTrackWet );
					}
					else
					{
						Say( Settings.SayTrackDry );
					}
				}

				_wetdry_currentConditionDisplayName = wetDryConditionDisplayName;

				UpdateWetDryConditionSaveName();

				Dispatcher.BeginInvoke( () =>
				{
					var mainWindow = MarvinsAIRA.MainWindow.Instance;

					if ( mainWindow != null )
					{
						mainWindow.CurrentTrackCondition_StatusBarItem.Content = _wetdry_currentConditionDisplayName;
						mainWindow.CurrentTrackCondition_StatusBarItem.Foreground = Brushes.ForestGreen;
					}
				} );
			}
		}

		public void UpdateWetDryConditionSaveName()
		{
			var oldWetDryConditionSaveName = _wetdry_conditionSaveName;

			_wetdry_conditionSaveName = Settings.SaveSettingsPerWetDryCondition ? _wetdry_currentConditionDisplayName : ALL_WETDRYCONDITIONS_SAVE_NAME;

			if ( _wetdry_conditionSaveName != oldWetDryConditionSaveName )
			{
				_wetdry_conditionChanged = true;
			}
		}
	}
}
