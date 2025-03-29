using System;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;
using System.Threading.Tasks;

using System.Windows;
using Windows.Media.Protection.PlayReady;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using SharpDX;
namespace MarvinsAIRA
{
    public partial class App: Application
	{
		public const float Version = 0.65f;

        private const string webpageUrl = "https://herboldracing.com/marvins-awesome-iracing-app-maira/previous-versions/"; 
        

        private bool updateAvailable = false;
        public float versionNumberAvalible { get; private set; }

        private string? downloadUrl;
        string UpdateFile;
        private bool _downloadedUpdate = false;

        public async void DownloadUpdate(MainWindow window)
        {
            if (_downloadedUpdate) //if we have already downloaded the update
            {
                

                window.Close();

                return;
            }
                

            if (!updateAvailable)
                return;

            
            UpdateFile = $"{AppDomain.CurrentDomain.BaseDirectory}MARVIN-Latest.exe";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    WriteLine("Starting download...");
                    window.UpdateInfo_Label.Content = "Downloading";
                    // Set request options to start reading headers before the content
                    HttpResponseMessage response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

                    response.EnsureSuccessStatusCode();
                    var contentLength = response.Content.Headers.ContentLength;

                    using (var fileStream = new FileStream(UpdateFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        long totalBytesRead = 0;
                        int bytesRead;

                        // Read the stream in chunks and write to the file
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            // Calculate and display progress
                            if (contentLength.HasValue)
                            {
                                double progressPercentage = ((double)totalBytesRead / contentLength.Value) * 100;
                                window.UpdateCheck_ProgressBar.Value = progressPercentage;
                                window.DownloadPer_StatusBarItem.Content = $"Downloading V{versionNumberAvalible} {progressPercentage:0.00}%";
                            }
                        }
                    }
                    window.UpdateInfo_Label.Content= $"Install V{versionNumberAvalible}";
                    window.UpdateInfo_Label.Foreground = System.Windows.Media.Brushes.Blue;
                    window.UpdateInfo_Label.Cursor = System.Windows.Input.Cursors.Hand;
                    updateAvailable = true;

                    WriteLine("Download completed successfully!");
                    _downloadedUpdate = true;
                    MessageBoxResult result = MessageBox.Show("Would you like to install Now?", "Download Completed", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                    

                        window.Close();
                        
                    }
                }
                catch (Exception ex)
                {
                    WriteLine($"Error: {ex.Message}");
                    window.UpdateInfo_Label.Content = "Download Failed";
                    window.UpdateInfo_Label.Foreground = System.Windows.Media.Brushes.White;
                    window.UpdateInfo_Label.Cursor = System.Windows.Input.Cursors.Arrow;
                }
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            new Thread(() => {

                Thread.Sleep(2000);
                Process process = new Process();
                process.StartInfo.FileName = UpdateFile;
                process.StartInfo.Arguments = ""; // Optional: Add command-line arguments if needed
                process.StartInfo.UseShellExecute = true; // Uses the shell for execution
                process.Start();



            }).Start();
        }

        

        public async void CheckForUpdates(MainWindow window, Action? OnUpdateExists = null)
        {
            WriteLine("CheckForUpdates called.", true);
            window.UpdateInfo_Label.Content = "Checking for updates...";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    WriteLine("Fetching webpage...");
                    string htmlContent = await client.GetStringAsync(webpageUrl);

                    // Parse the HTML
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(htmlContent);

                    // Find the latest .exe link (adjust XPath as needed)
                    downloadUrl = document.DocumentNode
                        .SelectNodes("//a[contains(@href, '.exe')]")
                        ?.Select(node => node.GetAttributeValue("href", null))
                        .FirstOrDefault();

                    if (downloadUrl != null)
                    {
                        string version = downloadUrl.Replace(".exe", "");
                        version = version.Substring(version.IndexOf("Setup-") + 6);

                        versionNumberAvalible = Convert.ToSingle(version);

                        if (versionNumberAvalible > Version)
                        {
                            WriteLine($"New version available: {versionNumberAvalible}");

                            window.UpdateInfo_Label.Content = $"{versionNumberAvalible} Available Download?";
                            window.UpdateInfo_Label.Foreground = System.Windows.Media.Brushes.CornflowerBlue;
                            window.UpdateInfo_Label.Cursor = System.Windows.Input.Cursors.Hand;
                            updateAvailable = true;
                            if (OnUpdateExists != null)
                                OnUpdateExists();
                        }
                        else
                        {
                            WriteLine("No new version available.");
                            window.UpdateInfo_Label.Content = "No new version available.";
                            window.UpdateInfo_Label.Foreground = System.Windows.Media.Brushes.White;
                            window.UpdateInfo_Label.Cursor = System.Windows.Input.Cursors.Arrow;
                            updateAvailable = false;
                        }

                    }
                    else
                    {
                        WriteLine("No .exe file found on the webpage.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Error: {ex.Message}");
            }
        }
		
	}
    
}
