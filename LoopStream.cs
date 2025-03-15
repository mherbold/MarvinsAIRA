using NAudio.Wave;

namespace MarvinsAIRA
{
	public class LoopStream( WaveStream waveStream ) : WaveStream
	{
		readonly WaveStream waveStream = waveStream;

		public override WaveFormat WaveFormat
		{
			get { return waveStream.WaveFormat; }
		}

		public override long Length
		{
			get { return waveStream.Length; }
		}

		public override long Position
		{
			get { return waveStream.Position; }
			set { waveStream.Position = value; }
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			var totalBytesRead = 0;

			while ( totalBytesRead < count )
			{
				var bytesRead = waveStream.Read( buffer, offset + totalBytesRead, count - totalBytesRead );

				if ( bytesRead == 0 )
				{
					if ( waveStream.Position == 0 )
					{
						break;
					}

					waveStream.Position = 0;
				}

				totalBytesRead += bytesRead;
			}

			return totalBytesRead;
		}
	}
}
