using System.Text;
using System.Windows;

/// </remarks>
namespace MarvinsAIRA
{
	public static class ClipboardHelper
	{
		private const string Header = @"Version:0.9
StartHTML:<<<<<<<<1
EndHTML:<<<<<<<<2
StartFragment:<<<<<<<<3
EndFragment:<<<<<<<<4
StartSelection:<<<<<<<<3
EndSelection:<<<<<<<<4";

		public const string StartFragment = "<!--StartFragment-->";

		public const string EndFragment = @"<!--EndFragment-->";

		private static readonly char[] _byteCount = new char[ 1 ];

		public static DataObject CreateDataObject( string html, string plainText )
		{
			html = html ?? String.Empty;

			var htmlFragment = GetHtmlDataString( html );

			if ( ( Environment.Version.Major < 4 ) && ( html.Length != Encoding.UTF8.GetByteCount( html ) ) )
			{
				htmlFragment = Encoding.Default.GetString( Encoding.UTF8.GetBytes( htmlFragment ) );
			}

			var dataObject = new DataObject();

			dataObject.SetData( DataFormats.Html, htmlFragment );
			dataObject.SetData( DataFormats.Text, plainText );
			dataObject.SetData( DataFormats.UnicodeText, plainText );

			return dataObject;
		}

		public static void CopyToClipboard( string html, string plainText )
		{
			var dataObject = CreateDataObject( html, plainText );

			Clipboard.SetDataObject( dataObject, true );
		}

		private static string GetHtmlDataString( string html )
		{
			var sb = new StringBuilder();

			sb.AppendLine( Header );
			sb.AppendLine( @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">" );

			// if given html already provided the fragments we won't add them
			int fragmentStart, fragmentEnd;
			int fragmentStartIdx = html.IndexOf( StartFragment, StringComparison.OrdinalIgnoreCase );
			int fragmentEndIdx = html.LastIndexOf( EndFragment, StringComparison.OrdinalIgnoreCase );

			// if html tag is missing add it surrounding the given html (critical)
			int htmlOpenIdx = html.IndexOf( "<html", StringComparison.OrdinalIgnoreCase );
			int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf( '>', htmlOpenIdx ) + 1 : -1;
			int htmlCloseIdx = html.LastIndexOf( "</html", StringComparison.OrdinalIgnoreCase );

			if ( fragmentStartIdx < 0 && fragmentEndIdx < 0 )
			{
				int bodyOpenIdx = html.IndexOf( "<body", StringComparison.OrdinalIgnoreCase );
				int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf( '>', bodyOpenIdx ) + 1 : -1;

				if ( htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0 )
				{
					// the given html doesn't contain html or body tags so we need to add them and place start/end fragments around the given html only
					sb.Append( "<html><body>" );
					sb.Append( StartFragment );
					fragmentStart = GetByteCount( sb );
					sb.Append( html );
					fragmentEnd = GetByteCount( sb );
					sb.Append( EndFragment );
					sb.Append( "</body></html>" );
				}
				else
				{
					// insert start/end fragments in the proper place (related to html/body tags if exists) so the paste will work correctly
					int bodyCloseIdx = html.LastIndexOf( "</body", StringComparison.OrdinalIgnoreCase );

					if ( htmlOpenEndIdx < 0 )
						sb.Append( "<html>" );
					else
						sb.Append( html, 0, htmlOpenEndIdx );

					if ( bodyOpenEndIdx > -1 )
						sb.Append( html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0, bodyOpenEndIdx - ( htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0 ) );

					sb.Append( StartFragment );
					fragmentStart = GetByteCount( sb );

					var innerHtmlStart = bodyOpenEndIdx > -1 ? bodyOpenEndIdx : ( htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0 );
					var innerHtmlEnd = bodyCloseIdx > -1 ? bodyCloseIdx : ( htmlCloseIdx > -1 ? htmlCloseIdx : html.Length );
					sb.Append( html, innerHtmlStart, innerHtmlEnd - innerHtmlStart );

					fragmentEnd = GetByteCount( sb );
					sb.Append( EndFragment );

					if ( innerHtmlEnd < html.Length )
						sb.Append( html, innerHtmlEnd, html.Length - innerHtmlEnd );

					if ( htmlCloseIdx < 0 )
						sb.Append( "</html>" );
				}
			}
			else
			{
				// handle html with existing start\end fragments just need to calculate the correct bytes offset (surround with html tag if missing)
				if ( htmlOpenEndIdx < 0 )
					sb.Append( "<html>" );
				int start = GetByteCount( sb );
				sb.Append( html );
				fragmentStart = start + GetByteCount( sb, start, start + fragmentStartIdx ) + StartFragment.Length;
				fragmentEnd = start + GetByteCount( sb, start, start + fragmentEndIdx );
				if ( htmlCloseIdx < 0 )
					sb.Append( "</html>" );
			}

			// Back-patch offsets (scan only the header part for performance)
			sb.Replace( "<<<<<<<<1", Header.Length.ToString( "D9" ), 0, Header.Length );
			sb.Replace( "<<<<<<<<2", GetByteCount( sb ).ToString( "D9" ), 0, Header.Length );
			sb.Replace( "<<<<<<<<3", fragmentStart.ToString( "D9" ), 0, Header.Length );
			sb.Replace( "<<<<<<<<4", fragmentEnd.ToString( "D9" ), 0, Header.Length );

			return sb.ToString();
		}

		private static int GetByteCount( StringBuilder sb, int start = 0, int end = -1 )
		{
			int count = 0;

			end = end > -1 ? end : sb.Length;

			for ( int i = start; i < end; i++ )
			{
				_byteCount[ 0 ] = sb[ i ];

				count += Encoding.UTF8.GetByteCount( _byteCount );
			}

			return count;
		}
	}
}
