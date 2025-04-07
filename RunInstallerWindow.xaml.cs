using System.Windows;

namespace MarvinsAIRA
{
	public partial class RunInstallerWindow : Window
	{
		public bool installUpdate = false;

		public RunInstallerWindow( string downloadPath )
		{
			InitializeComponent();

			DownloadPath_Label.Content = downloadPath;
		}

		private void InstallNow_Click( object sender, RoutedEventArgs e )
		{
			installUpdate = true;

			Close();
		}

		private void InstallLater_Click( object sender, RoutedEventArgs e )
		{
			installUpdate = false;

			Close();
		}
	}
}
