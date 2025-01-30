
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace MarvinsAIRA
{
	public partial class MainWindow : Window
	{
		#region Properties

		private bool _initialized = false;
		private bool _pauseScaleButtons = false;
		private bool _restartForceFeedback = false;

		private nint _windowHandle = 0;

		private readonly System.Timers.Timer _timer = new( 100 );

		private int _timerMutex = 0;
		private int _sendForceFeedbackTestSignalCounter = 0;

		#endregion

		#region Window

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

				app.WriteLine( "" );
				app.WriteLine( $"{Title} has been initialized!" );

				_timer.Elapsed += OnTimer;
				_timer.Start();

				if ( app.Settings.StartMinimized )
				{
					WindowState = WindowState.Minimized;
				}

				_initialized = true;
			}

			if ( _restartForceFeedback )
			{
				app.ReinitializeForceFeedbackDevice( _windowHandle );
			}
		}

		private void Window_Deactivated( object sender, EventArgs e )
		{
			_restartForceFeedback = true;
		}

		#endregion

		#region Timer

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
					app.UpdateInputs();
					app.UpdateForceFeedback( deltaTime, !_pauseScaleButtons, _windowHandle );
					app.UpdateWindSimulator();

					if ( _sendForceFeedbackTestSignalCounter > 0 )
					{
						if ( _sendForceFeedbackTestSignalCounter == 1 )
						{
							app.UpdateConstantForce( [ 0, 0, 0, 0, 0, 0 ] );
						}
						else
						{
							app.SendTestForceFeedbackSignal( ( _sendForceFeedbackTestSignalCounter & 1 ) == 0 );
						}

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
							ForceFeedbackStatusBarItem.Foreground = Brushes.ForestGreen;
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
							WindStatusBarItem.Foreground = Brushes.ForestGreen;
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

		#endregion

		#region Force feedback tab

		private void ForceFeedback_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "ForceFeedback_CheckBox_Click called." );

			var checkBox = (CheckBox) sender;

			if ( checkBox.IsChecked == true )
			{
				app.InitializeForceFeedback( _windowHandle );
			}
			else
			{
				app.StopForceFeedback();
			}
		}

		private void FFBDeviceComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( _initialized )
			{
				var app = (App) Application.Current;

				app.WriteLine( "" );
				app.WriteLine( "FFBDeviceComboBox_SelectionChanged called." );

				app.InitializeForceFeedback( _windowHandle );
			}
		}

		private void ForceFeedbackTestButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "ForceFeedbackTestButton_Click called." );

			_sendForceFeedbackTestSignalCounter = 11;
		}

		private void ResetForceFeedbackButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "ResetForceFeedbackButton_Click called." );

			app.Settings.SetForegroundWindow = ShowMapButtonWindow( app.Settings.SetForegroundWindow );
		}

		private void AutoOverallScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "AutoOverallScaleButton_Click called." );

			app.Settings.AutoOverallScale = ShowMapButtonWindow( app.Settings.AutoOverallScale );
		}

		private void DecreaseOverallScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "DecreaseOverallScaleButton_Click called." );

			app.Settings.DecreaseOverallScale = ShowMapButtonWindow( app.Settings.DecreaseOverallScale );
		}

		private void IncreaseOverallScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "IncreaseOverallScaleButton_Click called." );

			app.Settings.IncreaseOverallScale = ShowMapButtonWindow( app.Settings.IncreaseOverallScale );
		}

		private void DecreaseDetailScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "DecreaseDetailScaleButton_Click called." );

			app.Settings.DecreaseDetailScale = ShowMapButtonWindow( app.Settings.DecreaseDetailScale );
		}

		private void IncreaseDetailScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "IncreaseDetailScaleButton_Click called." );

			app.Settings.IncreaseDetailScale = ShowMapButtonWindow( app.Settings.IncreaseDetailScale );
		}

		private void FrequencySlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _initialized )
			{
				var app = (App) Application.Current;

				app.WriteLine( "" );
				app.WriteLine( "FrequencySlider_ValueChanged called." );

				app.ScheduleReinitializeForceFeedback();
			}
		}

		private void LFEDeviceComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( _initialized )
			{
				var app = (App) Application.Current;

				app.WriteLine( "" );
				app.WriteLine( "LFEDeviceComboBox_SelectionChanged called." );

				app.InitializeLFE();
			}
		}

		private void DecreaseLFEScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "DecreaseLFEScaleButton_Click called." );

			app.Settings.DecreaseLFEScale = ShowMapButtonWindow( app.Settings.DecreaseLFEScale );
		}

		private void IncreaseLFEScaleButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "IncreaseLFEScaleButton_Click called." );

			app.Settings.IncreaseLFEScale = ShowMapButtonWindow( app.Settings.IncreaseLFEScale );
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

		#endregion

		#region Wind simulator tab

		private void WindSimulator_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "WindSimulator_CheckBox_Click called." );
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

		#endregion

		#region General settings tab

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

		private void ClickSoundVolume_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _initialized )
			{
				var app = (App) Application.Current;

				app.WriteLine( "" );
				app.WriteLine( "ClickSoundVolume_ValueChanged called." );

				app.PlayClick();
			}
		}

		private void SaveForEachWheel_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.UpdateWheelSaveName();
			app.QueueForSerialization();
		}

		private void SaveForEachCar_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.UpdateCarSaveName();
			app.QueueForSerialization();
		}

		private void SaveForEachTrack_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.UpdateTrackSaveName();
			app.QueueForSerialization();
		}

		private void SaveForEachTrackConfig_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.UpdateTrackConfigSaveName();
			app.QueueForSerialization();
		}

		#endregion

		#region Help tab

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

			var text = ConsoleTextBox.Text.Replace( "\r\n", "\r\n\t" );

			Clipboard.SetText( $"\r\n\r\n\t{text}\r\n" );

			string url = "https://forums.iracing.com/messages/add/Marvin%20Herbold";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		#endregion

		#region Map button window

		private Settings.MappedButton ShowMapButtonWindow( Settings.MappedButton mappedButton )
		{
			var app = (App) Application.Current;

			app.WriteLine( "" );
			app.WriteLine( "Showing the map button dialog window..." );

			_pauseScaleButtons = true;

			var window = new MapButtonWindow
			{
				Owner = this,
				useShift = mappedButton.UseShift,
				useCtrl = mappedButton.UseCtrl,
				useAlt = mappedButton.UseAlt,
				deviceInstanceGuid = mappedButton.DeviceInstanceGuid,
				deviceProductName = mappedButton.DeviceProductName,
				buttonNumber = mappedButton.ButtonNumber
			};

			window.ShowDialog();

			if ( !window.canceled )
			{
				app.WriteLine( "...dialog window was closed..." );

				mappedButton.UseShift = window.useShift;
				mappedButton.UseCtrl = window.useCtrl;
				mappedButton.UseAlt = window.useAlt;
				mappedButton.DeviceInstanceGuid = window.deviceInstanceGuid;
				mappedButton.DeviceProductName = window.deviceProductName;
				mappedButton.ButtonNumber = window.buttonNumber;

				app.QueueForSerialization();

				app.WriteLine( $"...control mapping was changed to {mappedButton.DeviceProductName} button {mappedButton.ButtonNumber + 1}." );
			}
			else
			{
				app.WriteLine( "...dialog window was closed (canceled)." );
			}

			_pauseScaleButtons = false;

			return mappedButton;
		}

		#endregion
	}
}
