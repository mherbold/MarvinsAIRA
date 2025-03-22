
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

		[StructLayout( LayoutKind.Sequential )]
		public struct MARGINS
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		public const int GWL_WNDPROC = -4;
		public const int GWL_HINSTANCE = -6;
		public const int GWL_HWNDPARENT = -8;
		public const int GWL_ID = -12;
		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;
		public const int GWL_USERDATA = -21;

		public const uint WS_BORDER = 0x800000;
		public const uint WS_CAPTION = 0xc00000;
		public const uint WS_CHILD = 0x40000000;
		public const uint WS_CLIPCHILDREN = 0x2000000;
		public const uint WS_CLIPSIBLINGS = 0x4000000;
		public const uint WS_DISABLED = 0x8000000;
		public const uint WS_DLGFRAME = 0x400000;
		public const uint WS_GROUP = 0x20000;
		public const uint WS_HSCROLL = 0x100000;
		public const uint WS_MAXIMIZE = 0x1000000;
		public const uint WS_MAXIMIZEBOX = 0x10000;
		public const uint WS_MINIMIZE = 0x20000000;
		public const uint WS_MINIMIZEBOX = 0x20000;
		public const uint WS_OVERLAPPED = 0x0;
		public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
		public const uint WS_POPUP = 0x80000000;
		public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
		public const uint WS_SIZEFRAME = 0x40000;
		public const uint WS_SYSMENU = 0x80000;
		public const uint WS_TABSTOP = 0x10000;
		public const uint WS_VISIBLE = 0x10000000;
		public const uint WS_VSCROLL = 0x200000;

		public const uint WS_EX_ACCEPTFILES = 0x00000010;
		public const uint WS_EX_APPWINDOW = 0x00040000;
		public const uint WS_EX_CLIENTEDGE = 0x00000200;
		public const uint WS_EX_COMPOSITED = 0x02000000;
		public const uint WS_EX_CONTEXTHELP = 0x00000400;
		public const uint WS_EX_CONTROLPARENT = 0x00010000;
		public const uint WS_EX_DLGMODALFRAME = 0x00000001;
		public const uint WS_EX_LAYERED = 0x00080000;
		public const uint WS_EX_LAYOUTRTL = 0x00400000;
		public const uint WS_EX_LEFT = 0x00000000;
		public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
		public const uint WS_EX_LTRREADING = 0x00000000;
		public const uint WS_EX_MDICHILD = 0x00000040;
		public const uint WS_EX_NOACTIVATE = 0x08000000;
		public const uint WS_EX_NOINHERITLAYOUT = 0x00100000;
		public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
		public const uint WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
		public const uint WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;
		public const uint WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
		public const uint WS_EX_RIGHT = 0x00001000;
		public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;
		public const uint WS_EX_RTLREADING = 0x00002000;
		public const uint WS_EX_STATICEDGE = 0x00020000;
		public const uint WS_EX_TOOLWINDOW = 0x00000080;
		public const uint WS_EX_TOPMOST = 0x00000008;
		public const uint WS_EX_TRANSPARENT = 0x00000020;
		public const uint WS_EX_WINDOWEDGE = 0x00000100;

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

		[DllImport( "user32.dll", SetLastError = true )]
		public static extern IntPtr FindWindow( string? lpClassName, string lpWindowName );

		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32.dll", SetLastError = true, CharSet = CharSet.Auto )]
		public static extern bool PostMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

		[DllImport( "dwmapi.dll" )]
		public static extern int DwmExtendFrameIntoClientArea( IntPtr hwnd, ref MARGINS margins );

		[DllImport( "user32.dll" )]
		public static extern uint SetWindowLong( IntPtr hWnd, int nIndex, uint dwNewLong );
	}
}
