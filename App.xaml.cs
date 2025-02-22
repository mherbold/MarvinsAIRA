
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

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool SetForegroundWindow( IntPtr hWnd );

		public App()
		{
			if ( !_mutex.WaitOne( TimeSpan.Zero, true ) )
			{
				var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				var runningProcesses = Process.GetProcessesByName( assemblyName );

				foreach ( var runningProcess in runningProcesses )
				{
					if ( runningProcess.MainWindowHandle != IntPtr.Zero )
					{
						SetForegroundWindow( runningProcess.MainWindowHandle );
					}
				}

				Environment.Exit( 0 );

				return;
			}
		}

		public void Initialize( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "Initialize called." );

			try
			{
				InitializeConsole();
				InitializeSettings();
				InitializeVoice();
				InitializeSounds();
				InitializeInputs( windowHandle );
				InitializeForceFeedback( windowHandle, true );
				InitializeLFE();
				InitializeIRacingSDK();
				InitializeCurrentCar();
				InitializeWindSimulator();
			}
			catch ( Exception exception )
			{
				WriteLine( "" );
				WriteLine( "Unexpected exception thrown:" );
				WriteLine( exception.Message.Trim() );
			}
		}

		public void Stop()
		{
			WriteLine( "" );
			WriteLine( "Stop called." );

			try
			{
				StopIRacingSDK();
				StopForceFeedback();
				StopLFE();
				StopWindSimulator();
				StopConsole();
			}
			catch ( Exception exception )
			{
				WriteLine( "" );
				WriteLine( "Unexpected exception thrown:" );
				WriteLine( exception.Message.Trim() );
			}
		}
	}
}
