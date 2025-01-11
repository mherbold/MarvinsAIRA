
using System.IO.Ports;

namespace MarvinsAIRA
{
	public class Arduino( byte[] handshake )
	{
		private readonly SerialPort? _serialPort = ConnectPort( handshake );

		public SerialPort? SerialPort { get => _serialPort; }

		public void Close()
		{
			if ( _serialPort != null )
			{
				if ( _serialPort.IsOpen )
				{
					_serialPort.Close();
				}
			}
		}

		public void SendMessage( string message )
		{
			if ( _serialPort != null )
			{
				if ( _serialPort.IsOpen )
				{
					_serialPort.Write( message );
				}
			}
		}

		private static SerialPort? ConnectPort( byte[] handshake )
		{
			foreach ( var portName in SerialPort.GetPortNames() )
			{
				var serialPort = new SerialPort( portName );

				if ( !serialPort.IsOpen )
				{
					try
					{
						serialPort.BaudRate = 9600;
						serialPort.WriteTimeout = 500;
						serialPort.ReadTimeout = 500;

						serialPort.Open();
						serialPort.Write( handshake, 0, handshake.Length );

						var response = new byte[ handshake.Length ];
						var count = 0;
						var timedOut = false;

						while ( count < handshake.Length )
						{
							try
							{
								count += serialPort.Read( response, count, response.Length - count );
							}
							catch ( TimeoutException )
							{
								timedOut = true;
								break;
							}
						}

						if ( !timedOut && ( count == handshake.Length ) )
						{
							var handshakeMatches = true;

							for ( var i = 0; i < response.Length; i++ )
							{
								if ( response[ i ] != handshake[ i ] )
								{
									handshakeMatches = false;
									break;
								}
							}

							if ( handshakeMatches )
							{
								return serialPort;
							}
						}
					}
					catch
					{
					}

					if ( serialPort.IsOpen )
					{
						serialPort.Close();
					}
				}
			}

			return null;
		}
	}
}
