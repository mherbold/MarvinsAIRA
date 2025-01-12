
using System.IO;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		public const string APP_FOLDER_NAME = "MarvinsAIRA";

		public static string DocumentsFolder { get; private set; } = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), APP_FOLDER_NAME );

		public void Initialize( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "Initialize called." );

			try
			{
				InitializeConsole();
				InitializeSettings();
				InitializeVoice();
				InitializeJoysticks( windowHandle );
				InitializeForceFeedback( windowHandle );
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
				StopForceFeedbackThread();
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
