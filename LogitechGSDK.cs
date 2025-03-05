
using System.Runtime.InteropServices;

namespace MarvinsAIRA
{
	public class LogitechGSDK
	{
		[DllImport( "LogitechSteeringWheelEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl )]
		public static extern bool LogiPlayLedsDInput( nint deviceHandle, float currentRPM, float rpmFirstLedTurnsOn, float rpmRedLine );

	}
}
