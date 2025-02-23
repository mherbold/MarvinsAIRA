
using System.Runtime.InteropServices;

namespace MarvinsAIRA
{
	public static class WinApi
	{
		[StructLayout( LayoutKind.Sequential )]
		public struct PROCESS_POWER_THROTTLING_STATE
		{
			public UInt32 Version;
			public UInt32 ControlMask;
			public UInt32 StateMask;
		}

		public const int ProcessPowerThrottling = 4;
		public const UInt32 PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION = 4;

		public delegate void TimerEventHandler( UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2 );

		[DllImport( "kernel32.dll", SetLastError = true )]
		public static extern bool SetProcessInformation( IntPtr hProcess, int ProcessInformationClass, IntPtr ProcessInformation, UInt32 ProcessInformationSize );

		[DllImport( "winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent" )]
		public static extern UInt32 TimeSetEvent( UInt32 msDelay, UInt32 msResolution, TimerEventHandler handler, ref UInt32 userCtx, UInt32 eventType );

		[DllImport( "winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent" )]
		public static extern void TimeKillEvent( UInt32 uTimerId );

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool SetForegroundWindow( IntPtr hWnd );

		[DllImport( "user32.dll", SetLastError = true )]
		public static extern IntPtr RegisterDeviceNotification( IntPtr hRecipient, IntPtr NotificationFilter, uint Flags );

		[DllImport( "user32.dll" )]
		public static extern uint UnregisterDeviceNotification( IntPtr Handle );
	}
}
