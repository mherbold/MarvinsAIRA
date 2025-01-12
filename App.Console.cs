
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

				if ( lastWriteTime.CompareTo( DateTime.Now.AddMinutes( -15 ) ) < 0 )
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
			var messageWithTime = $"{DateTime.Now}   {message}";

			Debug.WriteLine( message );

			if ( _fileStream != null )
			{
				try
				{
					_readerWriterLock.AcquireWriterLock( 250 );

					try
					{
						var bytes = new UTF8Encoding( true ).GetBytes( $"{messageWithTime}\r\n" );

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
					mainWindow.ConsoleTextBox.Text += $"{messageWithTime}\r\n";
					mainWindow.ConsoleTextBox.CaretIndex = mainWindow.ConsoleTextBox.Text.Length;
					mainWindow.ConsoleTextBox.ScrollToEnd();
				}
			} );
		}
	}
}
