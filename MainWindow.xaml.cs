
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class MainWindow : Window
	{
		private bool _initialized = false;
		private bool _pauseScaleButtons = false;

		private nint _windowHandle = 0;

		private readonly System.Timers.Timer _timer = new( 100 );

		private int _timerMutex = 0;
		private int _sendForceFeedbackTestSignalCounter = 0;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "MainWindow.Window_Closing called." );

			app.WriteLine( "Stopping main window timer..." );

			_timer.Stop();
			_timer.Dispose();

			app.WriteLine( "...main window timer stopped." );

			app.Stop();
		}

		private void Window_Activated( object sender, EventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( $"MainWindow.Window_Activated called." );

			if ( !_initialized )
			{
				_windowHandle = new WindowInteropHelper( this ).Handle;

				app.Initialize( _windowHandle );

				_timer.Elapsed += OnTimer;
				_timer.Start();

				_initialized = true;
			}
		}

		private void OnTimer( object? sender, EventArgs e )
		{
			var timerMutex = Interlocked.Exchange( ref _timerMutex, 1 );

			if ( timerMutex == 0 )
			{
				var app = (App) Application.Current;

				try
				{
					var deltaTime = 0.1f;

					app.UpdateSettings( deltaTime );
					app.UpdateForceFeedback( deltaTime, !_pauseScaleButtons, _windowHandle );
					app.UpdateWindSimulator();

					if ( _sendForceFeedbackTestSignalCounter > 0 )
					{
						app.SendTestForceFeedbackSignal();

						_sendForceFeedbackTestSignalCounter--;
					}

					Dispatcher.BeginInvoke( () =>
					{
						if ( !app.FFB_Initialized )
						{
							ForceFeedbackStatusBarItem.Content = "FFB: Fault";
							ForceFeedbackStatusBarItem.Foreground = Brushes.Red;
						}
						else if ( app.FFB_ClippedTimer > 0 )
						{
							ForceFeedbackStatusBarItem.Content = "FFB: CLIPPING!";
							ForceFeedbackStatusBarItem.Foreground = Brushes.Red;
						}
						else if ( app.Settings.ForceFeedbackEnabled )
						{
							ForceFeedbackStatusBarItem.Content = $"FFB: {( app.FFB_CurrentMagnitude * 100f / App.DI_FFNOMINALMAX ):F0}%";
							ForceFeedbackStatusBarItem.Foreground = Brushes.White;
						}
						else
						{
							ForceFeedbackStatusBarItem.Content = $"FFB: Off";
							ForceFeedbackStatusBarItem.Foreground = Brushes.Gray;
						}

						if ( !app.Wind_Initialized )
						{
							WindStatusBarItem.Content = "Wind: Fault";
							WindStatusBarItem.Foreground = Brushes.Red;
						}
						else if ( app.Settings.WindSimulatorEnabled )
						{
							WindStatusBarItem.Content = $"Wind: {app.Wind_CurrentMagnitude:F0}%";
							WindStatusBarItem.Foreground = Brushes.White;
						}
						else
						{
							WindStatusBarItem.Content = $"Wind: Off";
							WindStatusBarItem.Foreground = Brushes.Gray;
						}
					} );
				}
				catch ( Exception exception )
				{
					app.WriteLine( "" );
					app.WriteLine( "Unexpected exception thrown:" );
					app.WriteLine( exception.Message.Trim() );
				}

				_timerMutex = 0;
			}
		}

		private void ForceFeedback_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "ForceFeedback_CheckBox_Click called." );

			var checkBox = (CheckBox) sender;

			if ( checkBox.IsChecked == true )
			{
				if ( !app.FFB_TaskIsRunning )
				{
					app.StartForceFeedbackTask();
				}
			}
			else
			{
				if ( app.FFB_TaskIsRunning )
				{
					app.StopForceFeedbackTask();

					app.UpdateMagnitude( 0 );
				}
			}
		}

		private void WindSimulator_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "WindSimulator_CheckBox_Click called." );
		}

		private void TogglePrettyGraph_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "EnablePrettyGraph_Button_Click called." );

			if ( app.TogglePrettyGraph() )
			{
				Dispatcher.BeginInvoke( () =>
				{
					TogglePrettyGraph_Button.Content = "Disable Pretty Graph";
				} );
			}
			else
			{
				Dispatcher.BeginInvoke( () =>
				{
					TogglePrettyGraph_Button.Content = "Enable Pretty Graph";
				} );
			}
		}

		private void DecreaseOverallScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "DecreaseOverallScaleButton_Click called." );

			(app.Settings.DecreaseOverallScaleDeviceInstanceGuid, app.Settings.DecreaseOverallScaleDeviceProductName, app.Settings.DecreaseOverallScaleButtonNumber) = ShowMapButtonWindow( app.Settings.DecreaseOverallScaleDeviceInstanceGuid, app.Settings.DecreaseOverallScaleDeviceProductName, app.Settings.DecreaseOverallScaleButtonNumber );
		}

		private void IncreaseOverallScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "IncreaseOverallScaleButton_Click called." );

			(app.Settings.IncreaseOverallScaleDeviceInstanceGuid, app.Settings.IncreaseOverallScaleDeviceProductName, app.Settings.IncreaseOverallScaleButtonNumber) = ShowMapButtonWindow( app.Settings.IncreaseOverallScaleDeviceInstanceGuid, app.Settings.IncreaseOverallScaleDeviceProductName, app.Settings.IncreaseOverallScaleButtonNumber );
		}

		private void DecreaseDetailScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "DecreaseDetailScaleButton_Click called." );

			(app.Settings.DecreaseDetailScaleDeviceInstanceGuid, app.Settings.DecreaseDetailScaleDeviceProductName, app.Settings.DecreaseDetailScaleButtonNumber) = ShowMapButtonWindow( app.Settings.DecreaseDetailScaleDeviceInstanceGuid, app.Settings.DecreaseDetailScaleDeviceProductName, app.Settings.DecreaseDetailScaleButtonNumber );
		}

		private void IncreaseDetailScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "IncreaseDetailScaleButton_Click called." );

			(app.Settings.IncreaseDetailScaleDeviceInstanceGuid, app.Settings.IncreaseDetailScaleDeviceProductName, app.Settings.IncreaseDetailScaleButtonNumber) = ShowMapButtonWindow( app.Settings.IncreaseDetailScaleDeviceInstanceGuid, app.Settings.IncreaseDetailScaleDeviceProductName, app.Settings.IncreaseDetailScaleButtonNumber );
		}

		private (Guid, string, int) ShowMapButtonWindow( Guid deviceInstanceGuid, string deviceProductName, int buttonNumber )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "Showing the map button dialog window..." );

			_pauseScaleButtons = true;

			var window = new MapButtonWindow
			{
				Owner = this,
				deviceInstanceGuid = deviceInstanceGuid,
				deviceProductName = deviceProductName,
				buttonNumber = buttonNumber
			};

			window.ShowDialog();

			if ( !window.canceled )
			{
				app.WriteLine( "...dialog window was closed..." );

				deviceInstanceGuid = window.deviceInstanceGuid;
				deviceProductName = window.deviceProductName;
				buttonNumber = window.buttonNumber;

				app.WriteLine( $"...control mapping was changed to {deviceProductName} button {buttonNumber + 1}." );
			}
			else
			{
				app.WriteLine( "...dialog window was closed (canceled)." );
			}

			_pauseScaleButtons = false;

			return (deviceInstanceGuid, deviceProductName, buttonNumber);
		}

		private void Test_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "Test_CheckBox_Click called." );

			if ( sender is CheckBox checkBox )
			{
				CheckBox[] testCheckBoxArray = { Test_1_CheckBox, Test_2_CheckBox, Test_3_CheckBox, Test_4_CheckBox, Test_5_CheckBox, Test_6_CheckBox, Test_7_CheckBox, Test_8_CheckBox };

				foreach ( var testCheckBox in testCheckBoxArray )
				{
					if ( testCheckBox != checkBox )
					{
						testCheckBox.IsChecked = false;
					}
				}

				var band = int.Parse( checkBox.Name.Substring( 5, 1 ) );

				if ( checkBox.IsChecked == true )
				{
					app.Wind_TestBand = band - 1;

					app.WriteLine( $"Band {band} selected for testing." );
				}
				else
				{
					app.Wind_TestBand = -1;

					app.WriteLine( $"Stopping testing on band {band}." );
				}
			}
		}

		private void SpeechSynthesizerVolume_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _initialized )
			{
				var app = (App) Application.Current;

				app.WriteLine( "" );
				app.WriteLine( "SpeechSynthesizerVolume_ValueChanged called." );

				app.UpdateVolume();

				app.Say( $"My voice is now at {app.Settings.SpeechSynthesizerVolume} percent.", true );
			}
		}

		private void SeeHelpDocumentation_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "SeeHelpDocumentation_Click called." );

			string url = "https://herboldracing.com";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		private void GoToIRacingForumThread_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "GoToIRacingForumThread_Click called." );

			string url = "https://forums.iracing.com/discussion/72467/marvins-awesome-iracing-app";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		private void SendMarvinYourConsoleLog_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "SendMarvinYourConsoleLog_Click called." );

			var consoleLog = app.ReadAllLines();

			var text = string.Join( "\r\n\t", consoleLog );

			Clipboard.SetText( $"\r\n\t{text}\r\n" );

			string url = "https://forums.iracing.com/messages/add/Marvin%20Herbold";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		private void ForceFeedbackDeviceComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( _initialized )
			{
				var app = (App) Application.Current;

				app.WriteLine( "" );
				app.WriteLine( "ForceFeedbackDeviceComboBox_SelectionChanged called." );

				app.InitializeForceFeedback( _windowHandle );
			}
		}

		private void ForceFeedbackTestButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "ForceFeedbackTestButton_Click called." );

			_sendForceFeedbackTestSignalCounter = 10;
		}
	}
}
