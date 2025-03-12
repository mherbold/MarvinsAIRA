﻿
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ModernWpf.Controls;

namespace MarvinsAIRA
{
	public partial class MainWindow : Window
	{
		#region Properties

		private const int IMAGE_WIDTH = 360;
		private const int IMAGE_HEIGHT = 200;
		private const int IMAGE_BYTES_PER_PIXEL = 4;
		private const int IMAGE_DPI = 96;
		private const int IMAGE_STRIDE = IMAGE_WIDTH * IMAGE_BYTES_PER_PIXEL;

		private bool _win_initialized = false;
		private bool _win_updateLoopRunning = false;
		private bool _win_pauseButtons = false;

		private nint _win_windowHandle = 0;

		private readonly WriteableBitmap _win_oversteerBitmap = new( IMAGE_WIDTH, IMAGE_HEIGHT, IMAGE_DPI, IMAGE_DPI, PixelFormats.Bgra32, null );
		private readonly byte[] _win_oversteerPixels = new byte[ IMAGE_STRIDE * IMAGE_HEIGHT ];

		private readonly WriteableBitmap _win_understeerBitmap = new( IMAGE_WIDTH, IMAGE_HEIGHT, IMAGE_DPI, IMAGE_DPI, PixelFormats.Bgra32, null );
		private readonly byte[] _win_understeerPixels = new byte[ IMAGE_STRIDE * IMAGE_HEIGHT ];

		private readonly System.Timers.Timer _win_timer = new( 100 );

		private int _win_keepThreadsAlive = 1;
		private int _win_sendForceFeedbackTestSignalCounter = 0;

		private float _win_guiUpdateTimer = 0;
		private float _win_inputReinitTimer = 0;

		private IntPtr _win_deviceChangeNotificationHandle = 0;

		private readonly Stopwatch _win_stopwatch = new();

		public readonly AutoResetEvent _win_autoResetEvent = new( false );

		public static MainWindow? Instance { get; private set; } = null;

		#endregion

		#region Window

		public MainWindow()
		{
			InitializeComponent();

			Instance = this;
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "MainWindow.Window_Closing called.", true );

			app.WriteLine( "...stopping the main window timer..." );

			_win_timer.Stop();
			_win_timer.Dispose();

			app.WriteLine( "...main window timer stopped." );

			if ( _win_updateLoopRunning )
			{
				app.WriteLine( "...killing the update loop..." );

				_win_keepThreadsAlive = 0;

				_win_autoResetEvent.Set();

				while ( _win_updateLoopRunning )
				{
					Thread.Sleep( 0 );
				}

				app.WriteLine( "...update loop killed..." );
			}

			app.WriteLine( "...unregistering device change notificiation..." );

			UnregisterDeviceChangeNotification();

			app.WriteLine( "...device change notificiation unregistered..." );

			app.Stop();

			Instance = null;
		}

		private void Window_Activated( object sender, EventArgs e )
		{
			if ( !_win_initialized )
			{
				var app = (App) Application.Current;

				_win_windowHandle = new WindowInteropHelper( this ).Handle;

				app.Initialize( _win_windowHandle );

				Oversteer_Image.Source = _win_oversteerBitmap;
				Understeer_Image.Source = _win_understeerBitmap;

				UpdateImages();

				app.WriteLine( "Starting the update loop...", true );

				_win_stopwatch.Restart();

				var thread = new Thread( UpdateLoop );

				thread.Start();

				while ( !_win_updateLoopRunning )
				{
					Thread.Sleep( 0 );
				}

				app.WriteLine( "...update loop started." );

				app.WriteLine( "Starting the window timer...", true );

				_win_timer.Elapsed += OnTimer;
				_win_timer.Start();

				app.WriteLine( "...window timer started." );

				LoadRecording();

				app.WriteLine( $"{Title} has been initialized!", true );
				app.WriteLine( string.Empty );

				if ( app.Settings.TopmostWindow )
				{
					app.WriteLine( "Setting window to be topmost." );

					Topmost = true;
				}

				if ( app.Settings.StartMinimized )
				{
					app.WriteLine( "Minimizing the window." );

					WindowState = WindowState.Minimized;
				}

				Advanced_ToggleSwitch_Toggled( Advanced_ToggleSwitch, new RoutedEventArgs() );

				_win_initialized = true;
			}
		}

		private void Window_SourceInitialized( object sender, EventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "Registering device change notification...", true );

			var source = PresentationSource.FromVisual( this ) as HwndSource;

			source?.AddHook( WndProc );

			RegisterDeviceChangeNotification();

			app.WriteLine( "...device change notification registered." );
		}

		private void RegisterDeviceChangeNotification()
		{
			var devBroadcastDeviceInterface = new DeviceChangeNotification.DEV_BROADCAST_DEVICEINTERFACE();

			devBroadcastDeviceInterface.dbcc_size = Marshal.SizeOf( devBroadcastDeviceInterface );
			devBroadcastDeviceInterface.dbcc_devicetype = DeviceChangeNotification.DBT_DEVTYP_DEVICEINTERFACE;
			devBroadcastDeviceInterface.dbcc_reserved = 0;
			devBroadcastDeviceInterface.dbcc_classguid = DeviceChangeNotification.USB_HID_GUID;

			var devBroadcastDeviceInterfacePtr = Marshal.AllocHGlobal( devBroadcastDeviceInterface.dbcc_size );

			Marshal.StructureToPtr( devBroadcastDeviceInterface, devBroadcastDeviceInterfacePtr, true );

			_win_deviceChangeNotificationHandle = WinApi.RegisterDeviceNotification( ( new WindowInteropHelper( this ) ).Handle, devBroadcastDeviceInterfacePtr, DeviceChangeNotification.DEVICE_NOTIFY_WINDOW_HANDLE );

			Marshal.PtrToStructure( devBroadcastDeviceInterfacePtr, devBroadcastDeviceInterface );
			Marshal.FreeHGlobal( devBroadcastDeviceInterfacePtr );
		}

		private void UnregisterDeviceChangeNotification()
		{
			_ = WinApi.UnregisterDeviceNotification( _win_deviceChangeNotificationHandle );
		}

		protected IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			if ( msg == DeviceChangeNotification.WM_DEVICECHANGE )
			{
				var nEventType = wParam.ToInt32();

				if ( ( nEventType == DeviceChangeNotification.DBT_DEVICEARRIVAL ) || ( nEventType == DeviceChangeNotification.DBT_DEVICEREMOVECOMPLETE ) )
				{
					var broadcastHdr = new DeviceChangeNotification.DEV_BROADCAST_HDR();

					Marshal.PtrToStructure( lParam, broadcastHdr );

					if ( broadcastHdr.dbch_devicetype == DeviceChangeNotification.DBT_DEVTYP_DEVICEINTERFACE )
					{
						var devBroadcastDeviceInterface1 = new DeviceChangeNotification.DEV_BROADCAST_DEVICEINTERFACE_1();

						Marshal.PtrToStructure( lParam, devBroadcastDeviceInterface1 );

						string devicePath = new string( devBroadcastDeviceInterface1.dbcc_name );

						int pos = devicePath.IndexOf( (char) 0 );

						if ( pos != -1 )
						{
							devicePath = devicePath[ ..pos ];
						}

						var app = (App) Application.Current;

						if ( nEventType == DeviceChangeNotification.DBT_DEVICEREMOVECOMPLETE )
						{
							app.WriteLine( $"Device {devicePath} was removed!" );
						}
						else if ( nEventType == DeviceChangeNotification.DBT_DEVICEARRIVAL )
						{
							app.WriteLine( $"Device {devicePath} was added!" );
						}

						_win_inputReinitTimer = 2f;
					}
				}
			}

			return IntPtr.Zero;
		}

		#endregion

		#region Update loop

		private void OnTimer( object? sender, EventArgs e )
		{
			var app = (App) Application.Current;

			if ( !app._irsdk_connected )
			{
				_win_autoResetEvent.Set();
			}
		}

		private void UpdateLoop()
		{
			var app = (App) Application.Current;

			_win_updateLoopRunning = true;

			try
			{
				while ( _win_keepThreadsAlive == 1 )
				{
					_win_autoResetEvent?.WaitOne();

					if ( _win_keepThreadsAlive == 1 )
					{
						var deltaTime = Math.Min( 0.1f, (float) _win_stopwatch.Elapsed.TotalSeconds );

						if ( deltaTime >= 1f / 120f )
						{
							_win_stopwatch.Restart();

							app.UpdateSettings( deltaTime );

							if ( !_win_pauseButtons )
							{
								app.UpdateInputs( deltaTime );
							}

							app.UpdateForceFeedback( deltaTime, !_win_pauseButtons, _win_windowHandle );
							app.UpdateWindSimulator();
							app.UpdateSpotter( deltaTime );
							app.UpdateLogitech();
							app.ProcessChatMessageQueue();

							// test signal

							if ( _win_sendForceFeedbackTestSignalCounter > 0 )
							{
								if ( _win_sendForceFeedbackTestSignalCounter == 1 )
								{
									app.UpdateConstantForce( [ 0 ] );
								}
								else
								{
									app.SendTestForceFeedbackSignal( ( _win_sendForceFeedbackTestSignalCounter & 1 ) == 0 );
								}

								_win_sendForceFeedbackTestSignalCounter--;
							}

							// gui

							_win_guiUpdateTimer -= deltaTime;

							if ( _win_guiUpdateTimer <= 0f )
							{
								_win_guiUpdateTimer = 0.05f;

								Dispatcher.BeginInvoke( () =>
								{
									// Pretty graph

									if ( app._ffb_drawPrettyGraph )
									{
										app._ffb_writeableBitmap?.WritePixels( new Int32Rect( 0, 0, App.FFB_WRITEABLE_BITMAP_WIDTH, App.FFB_WRITEABLE_BITMAP_HEIGHT ), app._ffb_pixels, App.FFB_PIXELS_BUFFER_STRIDE, 0, 0 );
									}

									// Recording status

									if ( app._ffb_recordingNow )
									{
										var recordTime = GetRecordingIndexAsTime();

										Recording_Label.Content = $"Recording - {recordTime}";
										Recording_Label.Visibility = ( ( app._irsdk_tickCount % 60 ) < 15 ) ? Visibility.Hidden : Visibility.Visible;
									}
									else
									{
										Recording_Label.Visibility = Visibility.Hidden;
									}

									// Playback status

									if ( app._ffb_playingBackNow )
									{
										var recordTime = GetRecordingIndexAsTime();

										Playback_Label.Content = $"Playback - {recordTime}";
										Playback_Label.Visibility = Visibility.Visible;
									}
									else
									{
										Playback_Label.Visibility = Visibility.Hidden;
									}

									// Clipping status

									if ( app.FFB_ClippedTimer > 0 )
									{
										if ( Clipping_Label.Visibility != Visibility.Visible )
										{
											Clipping_Label.Visibility = Visibility.Visible;

											app.Say( app.Settings.SayClipping );
										}
									}
									else
									{
										Clipping_Label.Visibility = Visibility.Hidden;
									}

									// Steering wheel angle

									var steeringWheelAngleInDegrees = app._irsdk_steeringWheelAngle * 180f / Math.PI;

									SteeringWheel_Image.RenderTransform = new RotateTransform( -steeringWheelAngleInDegrees );

									SteeringWheel_Label.Content = $"{steeringWheelAngleInDegrees:F0}°";

									if ( (string) SteeringWheel_Label.Content == "-0°" )
									{
										SteeringWheel_Label.Content = "0°";
									}

									// Speed

									if ( app._irsdk_displayUnits == 0 )
									{
										Speed_Label.Content = $"{app._irsdk_speed * App.MPS_TO_MPH:F0} MPH";
									}
									else
									{
										Speed_Label.Content = $"{app._irsdk_speed * App.MPS_TO_KPH:F0} KPH";
									}

									// X Velocity

									XVelocity_Label.Content = $"{app._irsdk_velocityX:F0} m/s";

									if ( (string) XVelocity_Label.Content == "-0 m/s" )
									{
										XVelocity_Label.Content = "0 m/s";
									}

									// Y Velocity

									YVelocity_Label.Content = $"{app._irsdk_velocityY:F1} m/s";

									if ( (string) YVelocity_Label.Content == "-0.0 m/s" )
									{
										YVelocity_Label.Content = "0.0 m/s";
									}

									// Yaw rate

									var yawRateInDegreesPerSecond = app._irsdk_yawRate * 180f / Math.PI;

									YawRate_Label.Content = $"{yawRateInDegreesPerSecond:F0}°/sec";

									if ( (string) YawRate_Label.Content == "-0°/sec" )
									{
										YawRate_Label.Content = $"0°/sec";
									}

									// Yaw rate factor (instant)

									YawRateFactorInstant_Label.Content = $"{app.FFB_YawRateFactorInstant:F2}";

									if ( (string) YawRateFactorInstant_Label.Content == "-0.00" )
									{
										YawRateFactorInstant_Label.Content = "0.00";
									}

									// Yaw rate factor (average)

									YawRateFactorAverage_Label.Content = $"{app.FFB_YawRateFactorAverage:F2}";

									if ( (string) YawRateFactorAverage_Label.Content == "-0.00" )
									{
										YawRateFactorAverage_Label.Content = "0.00";
									}

									// Oversteer amount

									var oversteerAmount = Math.Abs( app.FFB_OversteerAmount );

									OversteerAmount_Label.Content = $"{oversteerAmount * 100f:F0}%";

									// Understeer amount

									var understeerAmount = Math.Abs( app.FFB_UndersteerAmount );

									UndersteerAmount_Label.Content = $"{understeerAmount * 100f:F0}%";

									// Oversteer graph

									float osRange = app.Settings.OSEndYVelocity - app.Settings.OSStartYVelocity;

									var x = app.FFB_OversteerAmountLinear * IMAGE_WIDTH;
									var y = oversteerAmount * IMAGE_HEIGHT;

									Oversteer_Ellipse.RenderTransform = new TranslateTransform( IMAGE_WIDTH / 2f - x, IMAGE_HEIGHT / 2f - y );

									Oversteer_StartYVelocity_Label.Content = $"{app.Settings.OSStartYVelocity:F1} m/s";
									Oversteer_EndYVelocity_Label.Content = $"{app.Settings.OSEndYVelocity:F1} m/s";

									// Understeer graph

									float usRange = 0f;

									if ( app._irsdk_steeringWheelAngle >= 0 )
									{
										usRange = app.Settings.USStartYawRateFactorLeft - app.Settings.USEndYawRateFactorLeft;
									}
									else
									{
										usRange = app.Settings.USEndYawRateFactorRight - app.Settings.USStartYawRateFactorRight;
									}

									x = app.FFB_UndersteerAmountLinear * IMAGE_WIDTH;
									y = understeerAmount * IMAGE_HEIGHT;

									Understeer_Ellipse.RenderTransform = new TranslateTransform( x - IMAGE_WIDTH / 2f, IMAGE_HEIGHT / 2f - y );

									if ( app._irsdk_steeringWheelAngle >= 0 )
									{
										Understeer_StartYawRateFactor_Label.Content = $"YRF {app.Settings.USStartYawRateFactorLeft}";
										Understeer_EndYawRateFactor_Label.Content = $"YRF {app.Settings.USEndYawRateFactorLeft}";
									}
									else
									{
										Understeer_StartYawRateFactor_Label.Content = $"YRF {app.Settings.USStartYawRateFactorRight}";
										Understeer_EndYawRateFactor_Label.Content = $"YRF {app.Settings.USEndYawRateFactorRight}";
									}
								} );
							}

							// usb device changes

							if ( _win_inputReinitTimer > 0f )
							{
								_win_inputReinitTimer = Math.Max( 0f, _win_inputReinitTimer - deltaTime );

								if ( ( _win_inputReinitTimer == 0f ) && app.Settings.ReinitializeWhenDevicesChanged )
								{
									app.InitializeInputs( _win_windowHandle );
								}
							}
						}
					}
				}
			}
			catch ( Exception exception )
			{
				app.WriteLine( $"Exception caught inside the update loop: {exception.Message.Trim()}", true );
			}

			_win_updateLoopRunning = false;
		}

		private static string GetRecordingIndexAsTime()
		{
			var app = (App) Application.Current;

			var minutes = app._ffb_recordedSteeringWheelTorqueBufferIndex / ( 360 * 60 );
			var seconds = app._ffb_recordedSteeringWheelTorqueBufferIndex % ( 360 * 60 ) / 360f;

			return $"{minutes}:{seconds:00.0}";
		}

		#endregion

		#region Generic text box + slider functions

		[GeneratedRegex( "[^0123456789.]" )]
		private partial Regex NotDecimalNumbersRegex();

		[GeneratedRegex( "[0123456789.]" )]
		private partial Regex DecimalNumbersRegex();

		private void TextBox_GotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
		{
			if ( sender is TextBox textBox )
			{
				textBox.Text = NotDecimalNumbersRegex().Replace( textBox.Text, string.Empty );
			}
		}

		private void TextBox_PreviewTextInput( object sender, TextCompositionEventArgs e )
		{
			if ( sender is TextBox textBox )
			{
				if ( e.Text.Contains( '\r' ) )
				{
					Keyboard.ClearFocus();
				}
				else if ( !DecimalNumbersRegex().IsMatch( e.Text ) || ( e.Text.Contains( '.' ) && textBox.Text.Contains( '.' ) ) )
				{
					e.Handled = true;
				}
			}
		}

		private void TextBox_LostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
		{
			if ( sender is TextBox textBox )
			{
				if ( !float.TryParse( textBox.Text, out var value ) )
				{
					value = 0;
				}

				var sliderObject = GetNextTab( textBox, textBox.Parent, true );

				if ( sliderObject is Slider slider )
				{
					slider.Value = value;
				}

				textBox.GetBindingExpression( TextBox.TextProperty ).UpdateTarget();
			}
		}

		public static DependencyObject? GetNextTab( DependencyObject element, DependencyObject containerElement, bool goDownOnly )
		{
			var keyboardNavigation = typeof( FrameworkElement )?.GetProperty( "KeyboardNavigation", BindingFlags.NonPublic | BindingFlags.Static )?.GetValue( null );

			var method = keyboardNavigation?.GetType()?.GetMethod( "GetNextTab", BindingFlags.NonPublic | BindingFlags.Instance );

			if ( method != null )
			{
				return method.Invoke( keyboardNavigation, [ element, containerElement, goDownOnly ] ) as DependencyObject;
			}

			return null;
		}

		#endregion

		#region Force feedback tab

		private void ForceFeedback_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			var checkBox = (CheckBox) sender;

			if ( checkBox.IsChecked == true )
			{
				app.InitializeForceFeedback( _win_windowHandle );
			}
			else
			{
				app.StopForceFeedback();
			}
		}

		private void FFBDevice_ComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.InitializeForceFeedback( _win_windowHandle );
			}
		}

		private void ForceFeedbackTest_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "ForceFeedbackTest_Button_Click called." );

			_win_sendForceFeedbackTestSignalCounter = 11;
		}

		private void Record_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "Record_Button_Click called." );

			if ( !app._irsdk_connected )
			{
				app.WriteLine( "...the iRacing simulator is not running, so ignoring this." );
			}
			else
			{
				app._ffb_recordedSteeringWheelTorqueBufferIndex = 0;

				var wasRecording = app._ffb_recordingNow;

				app._ffb_playingBackNow = false;
				app._ffb_recordingNow = !app._ffb_recordingNow;

				if ( wasRecording )
				{
					SaveRecording();
				}
				else
				{
					Array.Clear( app._ffb_recordedSteeringWheelTorqueBuffer );
				}

				if ( app._ffb_recordingNow && !app._ffb_drawPrettyGraph )
				{
					TogglePrettyGraph();
				}

				app.WriteLine( $"...recording is now {app._ffb_recordingNow}" );
				app.WriteLine( $"...playback is now {app._ffb_playingBackNow}" );
			}
		}

		private void Play_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "Play_Button_Click called." );

			if ( !app._irsdk_connected )
			{
				app.WriteLine( "...the iRacing simulator is not running, so ignoring this." );
			}
			else
			{
				var wasRecording = app._ffb_recordingNow;

				app._ffb_playingBackNow = !app._ffb_playingBackNow;
				app._ffb_recordingNow = false;

				if ( wasRecording )
				{
					SaveRecording();
				}

				app._ffb_recordedSteeringWheelTorqueBufferIndex = 0;

				if ( app._ffb_playingBackNow && !app._ffb_drawPrettyGraph )
				{
					TogglePrettyGraph();
				}

				app.WriteLine( $"...playback is now {app._ffb_playingBackNow}" );
				app.WriteLine( $"...recording is now {app._ffb_recordingNow}" );
			}
		}

		private void ResetForceFeedback_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "ResetForceFeedback_Button_Click called." );

			app.Settings.ReinitForceFeedbackButtons = ShowMapButtonsWindow( app.Settings.ReinitForceFeedbackButtons );
		}

		private void AutoOverallScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "AutoOverallScale_Button_Click called." );

			app.Settings.AutoOverallScaleButtons = ShowMapButtonsWindow( app.Settings.AutoOverallScaleButtons );
		}

		private void DecreaseOverallScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "DecreaseOverallScale_Button_Click called." );

			app.Settings.DecreaseOverallScaleButtons = ShowMapButtonsWindow( app.Settings.DecreaseOverallScaleButtons );
		}

		private void IncreaseOverallScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "IncreaseOverallScale_Button_Click called." );

			app.Settings.IncreaseOverallScaleButtons = ShowMapButtonsWindow( app.Settings.IncreaseOverallScaleButtons );
		}

		private void DecreaseDetailScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "DecreaseDetailScale_Button_Click called." );

			app.Settings.DecreaseDetailScaleButtons = ShowMapButtonsWindow( app.Settings.DecreaseDetailScaleButtons );
		}

		private void IncreaseDetailScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "IncreaseDetailScale_Button_Click called." );

			app.Settings.IncreaseDetailScaleButtons = ShowMapButtonsWindow( app.Settings.IncreaseDetailScaleButtons );
		}

		private void Frequency_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.ScheduleReinitializeForceFeedback();
			}
		}

		private void TogglePrettyGraph_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "TogglePrettyGraph_Button_Click called." );

			TogglePrettyGraph();
		}

		private static void LoadRecording()
		{
			var app = (App) Application.Current;

			var filePath = Path.Combine( App.DocumentsFolder, "Recording.bin" );

			if ( File.Exists( filePath ) )
			{
				app.WriteLine( "...loading recording..." );

				try
				{
					using var stream = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.None );
					using var reader = new BinaryReader( stream );

					for ( int x = 0; x < app._ffb_recordedSteeringWheelTorqueBuffer.Length; x++ )
					{
						app._ffb_recordedSteeringWheelTorqueBuffer[ x ] = reader.ReadSingle();
					}
				}
				catch ( Exception exception )
				{
					app.WriteLine( $"Failed to load the recording file, exception was: {exception.Message.Trim()}" );
				}
			}
		}

		private static void SaveRecording()
		{
			var app = (App) Application.Current;

			app.WriteLine( "...saving recording..." );

			var filePath = Path.Combine( App.DocumentsFolder, "Recording.bin" );

			using var stream = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None );
			using var writer = new BinaryWriter( stream );

			for ( int x = 0; x < app._ffb_recordedSteeringWheelTorqueBuffer.Length; x++ )
			{
				writer.Write( app._ffb_recordedSteeringWheelTorqueBuffer[ x ] );
			}
		}

		private void TogglePrettyGraph()
		{
			var app = (App) Application.Current;

			if ( app.TogglePrettyGraph() )
			{
				PrettyGraph_Border.Visibility = Visibility.Visible;

				TogglePrettyGraph_Button.Content = "Disable Pretty Graph";
			}
			else
			{
				PrettyGraph_Border.Visibility = Visibility.Collapsed;

				TogglePrettyGraph_Button.Content = "Enable Pretty Graph";
			}
		}

		#endregion

		#region Steering effects tab

		private void USSineWaveBuzz_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.USEffectStyle = 0;
			}
		}

		private void USSawtoothWaveBuzz_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.USEffectStyle = 1;
			}
		}

		private void USConstantForce_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.USEffectStyle = 2;
			}
		}

		private void UndersteerEffect_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "UndersteerEffect_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.UndersteerEffectButtons );
		}

		private void USCurve_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			UpdateImages();
		}

		private void OSSineWaveBuzz_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.OSEffectStyle = 0;
			}
		}

		private void OSSawtoothWaveBuzz_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.OSEffectStyle = 1;
			}
		}

		private void OSConstantForce_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.OSEffectStyle = 2;
			}
		}

		private void OversteerEffect_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "OversteerEffect_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.OversteerEffectButtons );
		}

		private void OSCurve_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			UpdateImages();
		}

		#endregion

		#region LFE to FFB tab

		private void LFEToFFB_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.InitializeLFE();
			}
		}

		private void LFEDevice_ComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.InitializeLFE();
			}
		}

		private void DecreaseLFEScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "DecreaseLFEScale_Button_Click called." );

			app.Settings.DecreaseLFEScaleButtons = ShowMapButtonsWindow( app.Settings.DecreaseLFEScaleButtons );
		}

		private void IncreaseLFEScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "IncreaseLFEScale_Button_Click called." );

			app.Settings.IncreaseLFEScaleButtons = ShowMapButtonsWindow( app.Settings.IncreaseLFEScaleButtons );
		}

		#endregion

		#region Wind simulator tab

		private void Test_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

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

		#region Settings tab - Window tab

		private void TopmostWindow_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			var checkBox = (CheckBox) sender;

			Topmost = checkBox.IsChecked == true;
		}

		#endregion

		#region Settings tab - Save file tab

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

		#region Settings tab - Audio tab

		private void ClickSoundVolume_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.PlayClick();
			}
		}

		#endregion

		#region Settings tab - Voice tab

		private void SpeechSynthesizerVolume_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.UpdateVolume();

				app.Say( app.Settings.SayVoiceVolume, app.Settings.SpeechSynthesizerVolume.ToString(), true, false );
			}
		}

		private void SelectedVoice_ComboBox_SelectionChanged( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.InitializeVoice();

				app.Say( app.Settings.SayHello, null, true, false );
			}
		}

		#endregion

		#region Settings tab - Wheel tab

		private void SetWheelMinValue_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.Settings.WheelMinValue = app.Input_CurrentWheelPosition;
		}

		private void SetWheelCenterValue_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.Settings.WheelCenterValue = app.Input_CurrentWheelPosition;
		}

		private void SetWheelMaxValue_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.Settings.WheelMaxValue = app.Input_CurrentWheelPosition;
		}

		#endregion

		#region Help tab

		private void SeeHelpDocumentation_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "SeeHelpDocumentation_Click called." );

			string url = "https://herboldracing.com/marvins-awesome-iracing-app-maira/";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		private void GoToIRacingForumThread_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "GoToIRacingForumThread_Click called." );

			string url = "https://forums.iracing.com/discussion/72467/marvins-awesome-iracing-app";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		private void SendMarvinYourConsoleLog_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "SendMarvinYourConsoleLog_Click called." );

			var text = Console_TextBox.Text.Replace( "\r\n", "\r\n\t" );

			Clipboard.SetText( $"\r\n\r\n\t{text}\r\n" );

			string url = "https://forums.iracing.com/messages/add/Marvin%20Herbold";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		#endregion

		#region Advanced

		private void Advanced_ToggleSwitch_Toggled( object sender, RoutedEventArgs e )
		{
			if ( sender is ToggleSwitch toggleSwitch )
			{
				var visibility = toggleSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;

				// tabs items

				SteeringEffects_TabItem.Visibility = visibility;
				LFEtoFFB_TabItem.Visibility = visibility;
				WindSimulator_TabItem.Visibility = visibility;
				Spotter_TabItem.Visibility = visibility;
				SkidPad_TabItem.Visibility = visibility;
				Settings_Devices_TabItem.Visibility = visibility;
				Settings_Wheel_TabItem.Visibility = visibility;

				// force feedback tab

				Record_Button.Visibility = visibility;
				Play_Button.Visibility = visibility;
				Target_Label.Visibility = visibility;
				Target_TextBox.Visibility = visibility;
				Target_Slider.Visibility = visibility;
				AutoOverallScale_Button.Visibility = visibility;
				ParkedScale_Grid.Visibility = visibility;
				Frequency_Grid.Visibility = visibility;
				PrettyGraph_StackPanel.Visibility = visibility;

				// settings voice tab

				Settings_Voice_SayLFEScale_Grid.Visibility = visibility;
				Settings_Voice_SpotterCarLeftRight_GroupBox.Visibility = visibility;
				Settings_Voice_SpotterSessionFlags_GroupBox.Visibility = visibility;
			}
		}

		#endregion

		#region Map button window
		
		private Settings.MappedButtons ShowMapButtonsWindow( Settings.MappedButtons mappedButtons )
		{
			var app = (App) Application.Current;

			app.WriteLine( "Showing the map buttons dialog window...", true );

			_win_pauseButtons = true;
			app._win_Show_MapButtonWindow = true;
            var window = new MapButtonWindow
			{
				Owner = this,
				MappedButtons = mappedButtons,
			};

			window.ShowDialog();

			if ( !window.canceled )
			{
				app.WriteLine( "...dialog window was closed..." );

				mappedButtons.Button1 = window.MappedButtons.Button1;
				mappedButtons.Button2 = window.MappedButtons.Button2;

				app.QueueForSerialization();

				app.WriteLine( $"...button mapping was changed." );
				app._win_Show_MapButtonWindow = false;

            }
			else
			{
				app.WriteLine( "...dialog window was closed (canceled)." );
				app._win_Show_MapButtonWindow = false;

            }

			_win_pauseButtons = false;

			return mappedButtons;
		}

		#endregion

		#region Image drawing

		private void UpdateImages()
		{
			var app = (App) Application.Current;

			for ( var x = 0; x < IMAGE_WIDTH; x++ )
			{
				var usStr = (float) x / IMAGE_WIDTH;
				var osStr = 1f - usStr;

				var usPct = (float) Math.Pow( usStr, app.Settings.USCurve );
				var osPct = (float) Math.Pow( osStr, app.Settings.OSCurve );

				var usY0 = (int) ( usPct * IMAGE_HEIGHT ) - 1;
				var usY1 = usY0 + 2;

				var osY0 = (int) ( osPct * IMAGE_HEIGHT ) - 1;
				var osY1 = osY0 + 2;

				var c = (byte) 0;

				for ( var y = 0; y < IMAGE_HEIGHT; y++ )
				{
					var flippedY = IMAGE_HEIGHT - y - 1;

					c = ( ( y < usY0 ) || ( y > usY1 ) ) ? (byte) 0 : (byte) 255;

					_win_understeerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 0 ] = c;
					_win_understeerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 1 ] = c;
					_win_understeerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 2 ] = c;
					_win_understeerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 3 ] = 255;

					c = ( ( y < osY0 ) || ( y > osY1 ) ) ? (byte) 0 : (byte) 255;

					_win_oversteerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 0 ] = c;
					_win_oversteerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 1 ] = c;
					_win_oversteerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 2 ] = c;
					_win_oversteerPixels[ x * IMAGE_BYTES_PER_PIXEL + flippedY * IMAGE_STRIDE + 3 ] = 255;
				}
			}

			_win_understeerBitmap.WritePixels( new Int32Rect( 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT ), _win_understeerPixels, IMAGE_STRIDE, 0, 0 );
			_win_oversteerBitmap.WritePixels( new Int32Rect( 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT ), _win_oversteerPixels, IMAGE_STRIDE, 0, 0 );
		}

		#endregion
	}
}
