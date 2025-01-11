
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private ReaderWriterLock _readerWriterLock = new();
		private FileStream? _fileStream = null;

		public void InitializeConsole()
		{
			WriteLine( "" );
			WriteLine( "InitializeConsole called." );

			var filePath = Path.Combine( DocumentsFolder, "Console.log" );

			if ( File.Exists( filePath ) )
			{
				var lastWriteTime = File.GetLastWriteTime( filePath );

				if ( lastWriteTime.CompareTo( DateTime.Now.AddHours( -4 ) ) < 0 )
				{
					File.Delete( filePath );
				}
			}

			_fileStream = new FileStream( filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite );
		}

		public void StopConsole()
		{
			WriteLine( "" );
			WriteLine( "StopConsole called." );

			_fileStream?.Close();
			_fileStream?.Dispose();

			_fileStream = null;
		}

		public void WriteLine( string message )
		{
			Debug.WriteLine( message );

			if ( _fileStream != null )
			{
				try
				{
					_readerWriterLock.AcquireWriterLock( 250 );

					try
					{
						var bytes = new UTF8Encoding( true ).GetBytes( $"{DateTime.Now}   {message}\r\n" );

						_fileStream.Write( bytes, 0, bytes.Length );
						_fileStream.Flush();
					}
					finally
					{
						_readerWriterLock.ReleaseWriterLock();
					}
				}
				catch ( ApplicationException )
				{
				}

			}

			Dispatcher.BeginInvoke( () =>
			{
				var mainWindow = (MainWindow) MainWindow;

				if ( mainWindow != null )
				{
					mainWindow.ConsoleTextBox.Text += $"{message}\n";

					mainWindow.ConsoleTextBox.ScrollToEnd();
				}
			} );
		}

		public string[] ReadAllLines()
		{
			WriteLine( "" );
			WriteLine( "ReadAllLines called." );

			StopConsole();

			var filePath = Path.Combine( DocumentsFolder, "Console.log" );

			string[] consoleLog = [ "Console log file was not found!" ];

			if ( File.Exists( filePath ) )
			{
				consoleLog = File.ReadAllLines( filePath );
			}

			InitializeConsole();

			return consoleLog;
		}
	}
}
