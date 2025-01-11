
using System.IO;
using System.Xml.Serialization;

namespace MarvinsAIRA
{
	public static class Serializer
	{
		public static object? Load( string filePath, Type type )
		{
			var xmlSerializer = new XmlSerializer( type );

			using var fileStream = new FileStream( filePath, FileMode.Open );

			object? data = null;

			try
			{
				data = xmlSerializer.Deserialize( fileStream );
			}
			catch ( Exception )
			{

			}

			fileStream.Close();

			return data;
		}

		public static void Save( string filePath, object data )
		{
			var directoryName = Path.GetDirectoryName( filePath );

			if ( directoryName != null )
			{
				Directory.CreateDirectory( directoryName );
			}

			var xmlSerializer = new XmlSerializer( data.GetType() );

			using var streamWriter = new StreamWriter( filePath );

			xmlSerializer.Serialize( streamWriter, data );

			streamWriter.Close();
		}
	}
}
