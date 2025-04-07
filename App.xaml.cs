
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		public const string APP_FOLDER_NAME = "MarvinsAIRA";

		public static string DocumentsFolder { get; private set; } = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), APP_FOLDER_NAME );

		public const float MPS_TO_MPH = 2.23694f;
		public const float MPS_TO_KPH = 3.6f;

		private static Mutex _mutex = new( true, "MarvinsAIRA Mutex" );

		public App()
		{
			try
			{
				if ( !_mutex.WaitOne( TimeSpan.Zero, true ) )
				{
					var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					var runningProcesses = Process.GetProcessesByName( assemblyName );

					foreach ( var runningProcess in runningProcesses )
					{
						if ( runningProcess.MainWindowHandle != IntPtr.Zero )
						{
							WinApi.SetForegroundWindow( runningProcess.MainWindowHandle );
						}
					}

					Environment.Exit( 0 );

					return;
				}
			}
			catch
			{
			}

			DisableThrottling();
		}

		public void Initialize( nint windowHandle )
		{
			WriteLine( "Initialize called.", true );

			try
			{
				if ( !Directory.Exists( DocumentsFolder ) )
				{
					Directory.CreateDirectory( DocumentsFolder );
				}

				InitializeConsole();
				InitializeSettings();
				InitializeService();
				InitializeVoice();
				InitializeSounds();
				InitializeInputs( windowHandle );
				InitializeForceFeedback( windowHandle, true );
				InitializeLFE();
				InitializeIRacingSDK();
				InitializeWindSimulator();
				InitializeTelemetry();
			}
			catch ( Exception exception )
			{
				WriteLine( "Unexpected exception thrown:", true );
				WriteLine( exception.Message.Trim() );

				throw;
			}
		}

		public void Stop()
		{
			WriteLine( "Stop called.", true );

			try
			{
				StopTelemetry();
				StopIRacingSDK();
				StopForceFeedback();
				StopLFE();
				StopWindSimulator();
				StopConsole();
			}
			catch ( Exception exception )
			{
				WriteLine( "Unexpected exception thrown:", true );
				WriteLine( exception.Message.Trim() );
			}
		}

		private static int DisableThrottling()
		{
			var sz = Marshal.SizeOf( typeof( WinApi.PROCESS_POWER_THROTTLING_STATE ) );

			var pwrInfo = new WinApi.PROCESS_POWER_THROTTLING_STATE()
			{
				Version = 1,
				ControlMask = WinApi.PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION,
				StateMask = 0
			};

			var pwrInfoPtr = Marshal.AllocHGlobal( sz );

			Marshal.StructureToPtr( pwrInfo, pwrInfoPtr, false );

			var processHandle = Process.GetCurrentProcess().Handle;

			var result = WinApi.SetProcessInformation( processHandle, WinApi.ProcessPowerThrottling, pwrInfoPtr, (uint) sz );

			Marshal.FreeHGlobal( pwrInfoPtr );

			return result ? 0 : Marshal.GetLastWin32Error();
		}
	}
}
