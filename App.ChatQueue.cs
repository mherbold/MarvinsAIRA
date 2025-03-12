
using System.Text.RegularExpressions;
using System.Windows;

using IRSDKSharper;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly List<string> _chatQueue_messageList = [];

		private bool _chatQueue_windowOpened = false;

		[GeneratedRegex( "[{}()+^&~[\\]]" )]
		private static partial Regex _chatQueue_regex();

		private void Chat( string message )
		{
			if ( Settings.EnableChat && _irsdk_connected && ( _irsdk_playerCarNumber != string.Empty ) )
			{
				message = _chatQueue_regex().Replace( message, "{($1)}" );

				_chatQueue_messageList.Add( $"/{_irsdk_playerCarNumber} {message}\r" );
			}
		}

		public void ProcessChatMessageQueue()
		{
			if ( _chatQueue_messageList.Count > 0 )
			{
				if ( _chatQueue_windowOpened )
				{
					if ( _irsdk_windowHandle != null )
					{
						string chatMessage = _chatQueue_messageList[ 0 ];

						foreach ( var ch in chatMessage )
						{
							WinApi.PostMessage( (IntPtr) _irsdk_windowHandle, 0x0102, ch, 0 );
						}
					}

					_chatQueue_messageList.RemoveAt( 0 );

					if ( _chatQueue_messageList.Count > 0 )
					{
						_chatQueue_windowOpened = false;
					}
				}
				else
				{
					_irsdk.ChatComand( IRacingSdkEnum.ChatCommandMode.BeginChat, 0 );

					_chatQueue_windowOpened = true;
				}
			}
			else
			{
				if ( _chatQueue_windowOpened )
				{
					_irsdk.ChatComand( IRacingSdkEnum.ChatCommandMode.Cancel, 0 );

					_chatQueue_windowOpened = false;
				}
			}
		}
	}
}
