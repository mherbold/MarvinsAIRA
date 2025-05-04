
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private bool _logitech_disabled = false;

		public void UpdateLogitech()
		{
			if ( Settings.ControlRPMLights )
			{
				if ( !_logitech_disabled && _irsdk_connected && ( _ffb_drivingJoystick != null ) )
				{
					try
					{
						if ( !LogitechGSDK.LogiPlayLedsDInput( _ffb_drivingJoystick.NativePointer, _irsdk_rpm, _irsdk_shiftLightsFirstRPM, _irsdk_shiftLightsBlinkRPM ) )
						{
							_logitech_disabled = true;
						}
					}
					catch ( Exception )
					{
						_logitech_disabled = true;
					}

					if ( _logitech_disabled )
					{
						WriteLine( "The Logitech G SDK doesn't seem to be working, so we are disabling Logitech shift lights support." );
					}
				}
			}
		}
	}
}
