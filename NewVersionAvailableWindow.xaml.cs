using System.Windows;

namespace MarvinsAIRA
{
	public partial class NewVersionAvailableWindow : Window
	{
		public bool downloadUpdate = false;

		public NewVersionAvailableWindow( string currentVersion, string changeLog )
		{
			InitializeComponent();

			Version_Label.Content = $"Latest Version = {currentVersion}";

			var changeLogLines = changeLog.Split( "\n" );

			var changeLogText = string.Empty;

			foreach ( var changeLogLine in changeLogLines )
			{
				var trimmedChangeLogLine = changeLogLine.Trim();

				if ( trimmedChangeLogLine.Length > 0 )
				{
					changeLogText += $"• {trimmedChangeLogLine}\r\n";
				}
			}

			ChangeLog_TextBox.Text = changeLogText.Trim();
		}

		private void DownloadNow_Click( object sender, RoutedEventArgs e )
		{
			downloadUpdate = true;

			Close();
		}

		private void DownloadLater_Click( object sender, RoutedEventArgs e )
		{
			downloadUpdate= false;

			Close();
		}
	}
}
