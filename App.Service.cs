
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows;

using Newtonsoft.Json;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private Guid networkIdGuid;

		private void InitializeService()
		{
			WriteLine( "Initializing service...", true );

			var networkInterfaceList = NetworkInterface.GetAllNetworkInterfaces();

			var networkInterface = networkInterfaceList.FirstOrDefault();

			if ( networkInterface == null )
			{
				throw new Exception( "No network interfaces found!" );
			}

			if ( !Guid.TryParse( networkInterface.Id, out networkIdGuid ) )
			{
				throw new Exception( "Could not convert the network interface ID to a GUID!" );
			}

			WriteLine( $"...network ID = {networkIdGuid}" );

			WriteLine( "...service initialized." );
		}

		struct GetCurrentVersionResponse
		{
			public string currentVersion;
			public string downloadUrl;
			public string changeLog;
		}

		public async Task CheckForUpdates( bool manuallyLaunched )
		{
			var mainWindow = MarvinsAIRA.MainWindow.Instance;

			if ( mainWindow != null )
			{
				WriteLine( "Checking for updates...", true );

				mainWindow.UpdateInfo_StatusBarItem.Content = "Checking for updates...";

				mainWindow.UpdateInfo_Separator.Visibility = Visibility.Visible;
				mainWindow.UpdateInfo_StatusBarItem.Visibility = Visibility.Visible;

				try
				{
					var getCurrentVersionUrl = $"https://herboldracing.com/wp-json/maira/v1/get-current-version?id={networkIdGuid}";

					using var httpClient = new HttpClient();

					var jsonString = await httpClient.GetStringAsync( getCurrentVersionUrl );

					WriteLine( jsonString );

					var getCurrentVersionResponse = JsonConvert.DeserializeObject<GetCurrentVersionResponse>( jsonString );

					var appVersion = MarvinsAIRA.MainWindow.GetVersion();

					if ( appVersion != getCurrentVersionResponse.currentVersion )
					{
						WriteLine( "...newer version is available..." );

						var localFilePath = Path.Combine( DocumentsFolder, $"MarvinsAIRA-Setup-{getCurrentVersionResponse.currentVersion}.exe" );

						var updateDownloaded = File.Exists( localFilePath );

						if ( updateDownloaded && !manuallyLaunched )
						{
							WriteLine( "...file is already downloaded, skipping update process..." );
						}
						else
						{
							if ( !updateDownloaded )
							{
								var downloadUpdate = false;

								if ( Settings.AutomaticallyDownloadUpdates )
								{
									downloadUpdate = true;
								}
								else
								{
									WriteLine( "...asking user if they want to download the update..." );

									var window = new NewVersionAvailableWindow( getCurrentVersionResponse.currentVersion, getCurrentVersionResponse.changeLog )
									{
										Owner = mainWindow
									};

									window.ShowDialog();

									downloadUpdate = window.downloadUpdate;
								}

								if ( downloadUpdate )
								{
									WriteLine( $"...downloading update from {getCurrentVersionResponse.downloadUrl}..." );

									mainWindow.UpdateInfo_StatusBarItem.Content = $"Downloading update...";

									var httpResponseMessage = await httpClient.GetAsync( getCurrentVersionResponse.downloadUrl, HttpCompletionOption.ResponseHeadersRead );

									httpResponseMessage.EnsureSuccessStatusCode();

									var contentLength = httpResponseMessage.Content.Headers.ContentLength;

									using var fileStream = new FileStream( localFilePath, FileMode.Create, FileAccess.Write, FileShare.None );

									using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();

									var buffer = new byte[ 1024 * 1024 ];

									var totalBytesRead = 0;

									while ( true )
									{
										var bytesRead = await stream.ReadAsync( buffer );

										if ( bytesRead == 0 )
										{
											break;
										}

										await fileStream.WriteAsync( buffer.AsMemory( 0, bytesRead ) );

										totalBytesRead += bytesRead;

										if ( contentLength.HasValue && ( contentLength.Value > 0 ) )
										{
											var progressPct = 100f * (float) totalBytesRead / (float) contentLength.Value;

											mainWindow.UpdateInfo_StatusBarItem.Content = $"Downloading update ({progressPct:F0}%)...";
										}
									}

									WriteLine( $"...update downloaded..." );

									updateDownloaded = true;
								}
							}

							if ( updateDownloaded )
							{
								WriteLine( "...asking user if they want to run the installer..." );

								var window = new RunInstallerWindow( localFilePath )
								{
									Owner = mainWindow
								};

								window.ShowDialog();

								if ( window.installUpdate )
								{
									mainWindow.CloseAndLaunchInstaller( localFilePath );
								}
							}
						}
					}
				}
				catch ( Exception exception )
				{
					WriteLine( "...failed trying to check for updates:" );
					WriteLine( exception.Message.Trim() );
				}
				finally
				{
					mainWindow.UpdateInfo_Separator.Visibility = Visibility.Collapsed;
					mainWindow.UpdateInfo_StatusBarItem.Visibility = Visibility.Collapsed;
				}
			}
		}
	}
}
