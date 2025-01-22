
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
				InitializeSounds();
				InitializeInputs( windowHandle );
				InitializeForceFeedback( windowHandle );
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
