
using System.ComponentModel;
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

using WK.Libraries.BootMeUpNS;

using ModernWpf.Controls;

using Hardcodet.Wpf.TaskbarNotification;

using Brushes = System.Windows.Media.Brushes;

namespace MarvinsAIRA
{
	public partial class MainWindow : Window
	{
		#region Properties

		private const int IMAGE_WIDTH = 440;
		private const int IMAGE_HEIGHT = 160;
		private const int IMAGE_BYTES_PER_PIXEL = 4;
		private const int IMAGE_DPI = 96;
		private const int IMAGE_STRIDE = IMAGE_WIDTH * IMAGE_BYTES_PER_PIXEL;

		private bool _win_initialized = false;
		private bool _win_updateLoopRunning = false;
		private bool _win_pauseInputProcessing = false;
		private bool _win_absTestPlaying = false;
		private bool _win_dieNow = false;

		private nint _win_windowHandle = 0;
		private IntPtr _win_originalWindowStyle = 0;
		private IntPtr _win_originalWindowExStyle = 0;

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

		private BootMeUp _win_bootMeUp = new();

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

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "MainWindow.Window_Closing called.", true );

			if ( app.Settings.CloseToSystemTray )
			{
				if ( !_win_dieNow )
				{
					e.Cancel = true;

					Hide();

					ShowInTaskbar = false;

					if ( !app.Settings.HideTrayAlert )
					{
						MARIA_TaskbarIcon.ShowBalloonTip( "Hello there!", "Marvin's Awesome iRacing App is over here now.", BalloonIcon.None );
					}
				}
			}
		}

		private void Window_Closed( object sender, EventArgs e )
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
				_win_originalWindowStyle = WinApi.GetWindowLongPtr( _win_windowHandle, WinApi.GWL_STYLE );
				_win_originalWindowExStyle = WinApi.GetWindowLongPtr( _win_windowHandle, WinApi.GWL_EXSTYLE );

				app.Initialize( _win_windowHandle );

				UpdateWindowTransparency( false );

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
					if ( app.Settings.CloseToSystemTray )
					{
						app.WriteLine( "Minimizing the window to the system tray." );

						Close();
					}
					else
					{
						app.WriteLine( "Minimizing the window to the taskbar." );

						UpdateWindowTransparency( true );

						WindowState = WindowState.Minimized;
					}
				}

				Advanced_ToggleSwitch_Toggled( Advanced_ToggleSwitch, new RoutedEventArgs() );

				_win_bootMeUp.UseAlternativeOnFail = true;
				_win_bootMeUp.BootArea = BootMeUp.BootAreas.Registry;
				_win_bootMeUp.TargetUser = BootMeUp.TargetUsers.CurrentUser;
				_win_bootMeUp.Enabled = app.Settings.StartWithWindows;

				// supporters textbox

				string[] supporterList = [ "Bruno Cerdeira Santos", "Chad McNeese", "Diogo Xavier Olivera", "Robert JL Henry", "Alberto Rama", "Andreas Mauchle", "Camron Frederick", "Christian Scherf", "Erin Andrusak", "f1iq.bsky.social", "Flavio A Mendes", "G. Balla", "Joachim Osbeck", "Jonathan E Marshall", "Maksym Palazov", "Panayiotis Papaioannou", "Paolo", "Rick", "Robert Perry", "Roy Medawar", "Shane Cochran", "Simon A Todd", "Stuart H Ware", "V A Parnell Jr MD", "Yafar Abdala.", "Ari D Sherwood", "Ba Anh Vu", "Brendan Hobbart", "Dan", "Dave Cam", "M C Hastrich", "Mitchell Bowen", "Ryan Feltrin", "Steven Slater", "Alan King", "Jakub Trinkl", "Robert Watson", "Sascha", "Travis Rhoads", "John Ebersole", "Rowly", "@fuelpodcast", "@G83MIKE", "@sidgeracing", "Ade", "Alan", "Binesh Lad", "Carlos Mancuso", "Chad Buchanan", "Christian Elsinger", "Daniel König", "Didier Porte", "Hugo", "João Carriço", "Kevin Burke", "Kevin J Fanning", "Marco", "Marcus Iglesias de Souza Oliveira", "Marin Marinov", "Mark G", "Mark R McLewee", "Marshall", "Michael Buckley", "Mikey Polard", "MR A M HALL", "Patrick Rochadel", "Sean Symes", "Shawn L Parrish", "Spieler Ralf", "Stephane THIBAUT", "T", "William Daily Jr", "Yamine Taieb", "Matt Swift", "Adrian G Rubio", "Ahmad El Baba", "Alberto", "Alexander Socher", "ARMracing", "Austin Elliott", "Chase", "Crispin Williamson", "Grimaldi Jean", "Grimax Racing", "GUIDOTTI", "Halan Williams", "Jeroni Fajardo", "John Millet", "Karl Thoroddsen", "Lufino", "Markus Kathan", "Massimo Martiglia", "Michie", "Mr Kurt Nicholson", "Nicholas Williams", "Ole", "Olly", "Omar Carlet", "parmand", "Rene Vorwerk", "Serge Montembault", "Shane Gleeson", "Stanislav Boldyryev", "Steven Barker", "Tom Weston", "Tripp Lanier", "Joy Perez", "Rodrigo Ribeiro R S lima", "Sebastian", "Yann LE DOUSSAL", "Jeremy", "Knosby", "Luka", "Miguel Angel Casado González", "and to all of the others who have chosen to remain anonymous." ];

				Supporters_TextBox.Text = "Thank you to " + string.Join( ", ", supporterList );

				//

				_win_initialized = true;
			}
		}

		private void Window_StateChanged( object sender, EventArgs e )
		{
			if ( WindowState == WindowState.Normal )
			{
				UpdateWindowTransparency( false );
			}
		}

		private void Window_SourceInitialized( object sender, EventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "Registering custom window message handler...", true );

			var source = PresentationSource.FromVisual( this ) as HwndSource;

			source?.AddHook( WndProc );

			app.WriteLine( "...custom window message handler registered." );

			RegisterDeviceChangeNotification();
		}

		private void RegisterDeviceChangeNotification()
		{
			var app = (App) Application.Current;

			app.WriteLine( "Registering device change notification...", true );

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

			app.WriteLine( "...device change notification registered." );
		}

		private void UnregisterDeviceChangeNotification()
		{
			_ = WinApi.UnregisterDeviceNotification( _win_deviceChangeNotificationHandle );
		}

		protected IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			switch ( msg )
			{
				case WinApi.WM_SYSCOMMAND:
				{
					if ( _win_initialized )
					{
						if ( ( wParam & 0xFFF0 ) == WinApi.SC_MINIMIZE )
						{
							UpdateWindowTransparency( true );
						}
					}

					break;
				}

				case WinApi.WM_DEVICECHANGE:
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

					break;
				}
			}

			return IntPtr.Zero;
		}

		private void UpdateWindowTransparency( bool forceOpaque )
		{
			var app = (App) Application.Current;

			if ( ( app.Settings.WindowOpacity == 100 ) || forceOpaque )
			{
				_ = WinApi.SetWindowLong( _win_windowHandle, WinApi.GWL_STYLE, (uint) _win_originalWindowStyle );
				_ = WinApi.SetWindowLong( _win_windowHandle, WinApi.GWL_EXSTYLE, (uint) _win_originalWindowExStyle );

				Opacity = 1f;
			}
			else
			{
				_ = WinApi.SetWindowLong( _win_windowHandle, WinApi.GWL_STYLE, WinApi.WS_POPUP | WinApi.WS_VISIBLE );
				_ = WinApi.SetWindowLong( _win_windowHandle, WinApi.GWL_EXSTYLE, WinApi.WS_EX_LAYERED | WinApi.WS_EX_TRANSPARENT );

				Opacity = app.Settings.WindowOpacity / 100f;
			}
		}

		#endregion

		#region Taskbar Icon

		private void TaskbarIcon_TrayLeftMouseDown( object sender, RoutedEventArgs e )
		{
			ShowApp_MenuItem_Click( sender, e );
		}

		private void ShowApp_MenuItem_Click( object sender, RoutedEventArgs e )
		{
			Show();

			if ( WindowState == WindowState.Minimized )
			{
				WindowState = WindowState.Normal;
			}

			WinApi.SetForegroundWindow( _win_windowHandle );

			ShowInTaskbar = true;
		}

		private void ExitApp_MenuItem_Click( object sender, RoutedEventArgs e )
		{
			_win_dieNow = true;

			Close();
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

							if ( !_win_pauseInputProcessing )
							{
								app.UpdateInputs( deltaTime );
							}

							app.UpdateForceFeedback( deltaTime, _win_pauseInputProcessing, _win_windowHandle );
							app.UpdateWindSimulator();
							app.UpdateSpotter( deltaTime );
							app.UpdateLogitech();
							app.UpdateTelemetry();
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
								_win_guiUpdateTimer = 0.033f;

								Dispatcher.BeginInvoke( () =>
								{
									// iRacing FFB warning

									ForceFeedbackWarning_Label.Visibility = app._irsdk_steeringFFBEnabled ? Visibility.Visible : Visibility.Collapsed;

									// Auto overall scale button

									AutoOverallScale_Button.Content = $"{app.FFB_AutoOverallScalePeakForceInNewtonMeters:F1} N⋅m";

									if ( app.FFB_AutoOverallScaleIsReady )
									{
										AutoOverallScale_Button.IsEnabled = true;
										AutoOverallScale_Button.BorderBrush = Brushes.Green;
									}
									else
									{
										AutoOverallScale_Button.IsEnabled = false;
										AutoOverallScale_Button.BorderBrush = Brushes.DarkRed;
									}

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

									// Spotter voice synth off warning

									if ( app.Settings.EnableSpeechSynthesizer )
									{
										SpotterVoiceSynthOffWarning_Label.Visibility = Visibility.Collapsed;
									}
									else
									{
										SpotterVoiceSynthOffWarning_Label.Visibility = Visibility.Visible;
									}

									// Steering wheel angle

									var steeringWheelAngleInDegrees = app._irsdk_steeringWheelAngle * 180f / MathF.PI;

									SteeringWheel_Image.RenderTransform = new RotateTransform( -steeringWheelAngleInDegrees );

									SteeringWheel_Label.Content = $"{steeringWheelAngleInDegrees:F0}°";

									if ( (string) SteeringWheel_Label.Content == "-0°" )
									{
										SteeringWheel_Label.Content = "0°";
									}

									// Clutch, brake, and throttle minimums

									ClutchMin_Rectangle.Visibility = ( app._irsdk_clutch < 1f ) ? Visibility.Visible : Visibility.Hidden;
									BrakeMin_Rectangle.Visibility = ( app._irsdk_brake > 0f ) ? Visibility.Visible : Visibility.Hidden;
									ThrottleMin_Rectangle.Visibility = ( app._irsdk_throttle > 0f ) ? Visibility.Visible : Visibility.Hidden;

									// Clutch, brake, and throttle

									var clutchHeight = Math.Clamp( 90f - app._irsdk_clutch * 90f, 0f, 90f );

									Clutch_Rectangle.Height = clutchHeight;
									Clutch_Rectangle.Margin = new Thickness( 0, 90f - clutchHeight, 0f, 0f );

									var brakeHeight = Math.Clamp( app._irsdk_brake * 90f, 0f, 90f );

									Brake_Rectangle.Height = brakeHeight;
									Brake_Rectangle.Margin = new Thickness( 0, 90f - brakeHeight, 0f, 0f );

									var throttleHeight = Math.Clamp( app._irsdk_throttle * 90f, 0f, 90f );

									Throttle_Rectangle.Height = throttleHeight;
									Throttle_Rectangle.Margin = new Thickness( 0, 90f - throttleHeight, 0f, 0f );

									// Clutch, brake, and throttle maximums

									ClutchMax_Rectangle.Visibility = ( app._irsdk_clutch == 0f ) ? Visibility.Visible : Visibility.Hidden;
									BrakeMax_Rectangle.Visibility = ( app._irsdk_brake == 1f ) ? Visibility.Visible : Visibility.Hidden;
									ThrottleMax_Rectangle.Visibility = ( app._irsdk_throttle == 1f ) ? Visibility.Visible : Visibility.Hidden;

									// Gear

									if ( app._irsdk_gear == -1 )
									{
										Gear_Label.Content = "R";
									}
									else if ( app._irsdk_gear == 0 )
									{
										Gear_Label.Content = "N";
									}
									else
									{
										Gear_Label.Content = app._irsdk_gear.ToString();
									}

									// ABS

									if ( app._irsdk_brakeABSactive )
									{
										ABS_Label.Foreground = Brushes.Orange;
									}
									else
									{
										ABS_Label.Foreground = Brushes.Gray;
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

									var yawRateInDegreesPerSecond = app._irsdk_yawRate * 180f / MathF.PI;

									YawRate_Label.Content = $"{yawRateInDegreesPerSecond:F0}°/sec";

									if ( (string) YawRate_Label.Content == "-0°/sec" )
									{
										YawRate_Label.Content = $"0°/sec";
									}

									// Yaw rate factor (instant)

									YawRateFactorInstant_Label.Content = $"{app.FFB_YawRateFactorInstant:F0}";

									if ( (string) YawRateFactorInstant_Label.Content == "-0" )
									{
										YawRateFactorInstant_Label.Content = "0";
									}

									// Yaw rate factor (average)

									YawRateFactorAverage_Label.Content = $"{app.FFB_YawRateFactorAverage:F0}";

									if ( (string) YawRateFactorAverage_Label.Content == "-0" )
									{
										YawRateFactorAverage_Label.Content = "0";
									}

									// Peak G force (last 2 seconds)

									PeakGForce_Label.Content = $"{app.FFB_PeakGForce:F1} G";

									// Oversteer amount

									var oversteerAmount = MathF.Abs( app.FFB_OversteerAmount );

									OversteerAmount_Label.Content = $"{oversteerAmount * 100f:F0}%";

									// Understeer amount

									var understeerAmount = MathF.Abs( app.FFB_UndersteerAmount );

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

				throw;
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

		private void Slider_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			e.Handled = true;
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

		private void ResetForceFeedbackMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "ResetForceFeedbackMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.ReinitForceFeedbackButtons );
		}

		private void AutoOverallScale_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "AutoOverallScale_Button_Click called." );

			app.DoAutoOverallScaleNow();
		}

		private void AutoOverallScale_Button_MouseRightButtonDown( object sender, MouseButtonEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "AutoOverallScale_Button_MouseRightButtonDown called." );

			app.ResetAutoOverallScaleMetrics();
		}

		private void AutoOverallScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "AutoOverallScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.AutoOverallScaleButtons );
		}

		private void ClearAutoOverallScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "ClearAutoOverallScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.ClearAutoOverallScaleButtons );
		}

		private void DecreaseOverallScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "DecreaseOverallScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.DecreaseOverallScaleButtons );
		}

		private void IncreaseOverallScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "IncreaseOverallScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.IncreaseOverallScaleButtons );
		}

		private void DecreaseDetailScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "DecreaseDetailScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.DecreaseDetailScaleButtons );
		}

		private void IncreaseDetailScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "IncreaseDetailScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.IncreaseDetailScaleButtons );
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

		private void PrettyGraph_Grid_MouseDown( object sender, MouseButtonEventArgs e )
		{
			var visibility = ( TogglePrettyGraph_Button.Visibility == Visibility.Visible ) ? Visibility.Collapsed : Visibility.Visible;

			ForceFeedback_StackPanel.Visibility = visibility;
			TogglePrettyGraph_Button.Visibility = visibility;
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

		private void USConstantForceInverted_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.USEffectStyle = 3;
			}
		}

		private void UndersteerEffectMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "UndersteerEffectMap_Button_Click called." );

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

		private void OSConstantForceInverted_RadioButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.Settings.OSEffectStyle = 3;
			}
		}

		private void OSCurve_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			UpdateImages();
		}

		public void FixRangeSliders()
		{
			var app = (App) Application.Current;

			Dispatcher.BeginInvoke( () =>
			{
				USYawRateFactorLeft_RangeSlider.LowerValue = app.Settings.USStartYawRateFactorLeft;
				USYawRateFactorLeft_RangeSlider.HigherValue = app.Settings.USEndYawRateFactorLeft;
				USYawRateFactorLeft_RangeSlider.LowerValue = app.Settings.USStartYawRateFactorLeft;
				USYawRateFactorLeft_RangeSlider.HigherValue = app.Settings.USEndYawRateFactorLeft;

				USYawRateFactorRight_RangeSlider.LowerValue = app.Settings.USStartYawRateFactorRight;
				USYawRateFactorRight_RangeSlider.HigherValue = app.Settings.USEndYawRateFactorRight;
				USYawRateFactorRight_RangeSlider.LowerValue = app.Settings.USStartYawRateFactorRight;
				USYawRateFactorRight_RangeSlider.HigherValue = app.Settings.USEndYawRateFactorRight;

				OSYVelocity_RangeSlider.LowerValue = app.Settings.OSStartYVelocity;
				OSYVelocity_RangeSlider.HigherValue = app.Settings.OSEndYVelocity;
				OSYVelocity_RangeSlider.LowerValue = app.Settings.OSStartYVelocity;
				OSYVelocity_RangeSlider.HigherValue = app.Settings.OSEndYVelocity;
			} );
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

		private void DecreaseLFEScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "DecreaseLFEScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.DecreaseLFEScaleButtons );
		}

		private void IncreaseLFEScaleMap_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "IncreaseLFEScaleMap_Button_Click called." );

			ShowMapButtonsWindow( app.Settings.IncreaseLFEScaleButtons );
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

		#region Settings tab - App tab

		private void StartWithWindows_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var checkBox = (CheckBox) sender;

			var startWithWindows = checkBox.IsChecked == true;

			_win_bootMeUp.Enabled = startWithWindows;
		}

		private void TopmostWindow_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			var checkBox = (CheckBox) sender;

			Topmost = checkBox.IsChecked == true;
		}

		private void WindowOpacity_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _win_initialized )
			{
				UpdateWindowTransparency( false );
			}
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

		private void SaveForEachWetDryCondition_CheckBox_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.UpdateWetDryConditionSaveName();
			app.QueueForSerialization();
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

		#region Settings tab - Audio tab

		private void ClickSoundVolume_Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( _win_initialized )
			{
				var app = (App) Application.Current;

				app.PlayClick();
			}
		}

		private void ABSTest_Button_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			var app = (App) Application.Current;

			if ( !_win_absTestPlaying )
			{
				_win_absTestPlaying = true;

				app.PlayABS();
			}
		}

		private void ABSTest_Button_PreviewMouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			var app = (App) Application.Current;

			if ( _win_absTestPlaying )
			{
				_win_absTestPlaying = false;

				app.StopABS();
			}
		}

		private void ABSTest_Button_MouseLeave( object sender, MouseEventArgs e )
		{
			var app = (App) Application.Current;

			if ( _win_absTestPlaying )
			{
				_win_absTestPlaying = false;

				app.StopABS();
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

		#region Contribute tab

		private void GoToGitHub_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "GoToGitHub_Button_Click called." );

			string url = "https://github.com/mherbold/MarvinsAIRA";

			var processStartInfo = new ProcessStartInfo( "cmd", $"/c start {url}" )
			{
				CreateNoWindow = true
			};

			Process.Start( processStartInfo );
		}

		private void GoToBuyMeACoffee_Button_Click( object sender, RoutedEventArgs e )
		{
			var app = (App) Application.Current;

			app.WriteLine( "GoToBuyMeACoffee_Button_Click called." );

			string url = "https://buymeacoffee.com/marvinherbold";

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

				// force feedback tab

				Record_Button.Visibility = visibility;
				Play_Button.Visibility = visibility;
				ParkedScale_Grid.Visibility = visibility;
				Frequency_Grid.Visibility = visibility;
				PrettyGraph_StackPanel.Visibility = visibility;

				// settings tab

				Settings_Devices_TabItem.Visibility = visibility;
				Settings_ForceFeedback_TabItem.Visibility = visibility;
				Settings_SteeringEffects_TabItem.Visibility = visibility;
				Settings_AutoCenterWheel_TabItem.Visibility = visibility;
				Settings_CrashProtection_TabItem.Visibility = visibility;
				Settings_SoftLock_TabItem.Visibility = visibility;
				Settings_Voice_SayLFEScale_Grid.Visibility = visibility;
				Settings_Voice_SpotterCarLeftRight_GroupBox.Visibility = visibility;
				Settings_Voice_SpotterSessionFlags_GroupBox.Visibility = visibility;
			}
		}

		#endregion

		#region Map button window

		private void ShowMapButtonsWindow( Settings.MappedButtons mappedButtons )
		{
			var app = (App) Application.Current;

			app.WriteLine( "Showing the map buttons dialog window...", true );

			_win_pauseInputProcessing = true;

			var window = new MapButtonWindow( mappedButtons )
			{
				Owner = this
			};

			window.ShowDialog();

			if ( !window.canceled )
			{
				app.WriteLine( "...dialog window was closed..." );

				app.QueueForSerialization();

				app.WriteLine( $"...button mapping was changed." );
			}
			else
			{
				app.WriteLine( "...dialog window was closed (canceled)." );
			}

			_win_pauseInputProcessing = false;
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

				var usPct = MathF.Pow( usStr, app.Settings.USCurve );
				var osPct = MathF.Pow( osStr, app.Settings.OSCurve );

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
