
using SharpDX.DirectInput;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly List<Joystick> _joystickList = [];

		public class PressedButton
		{
			public required string deviceProductName;
			public Guid deviceInstanceGuid;
			public int buttonNumber;
		}

		private void InitializeJoysticks( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "InitializeJoysticks called." );
			WriteLine( "Initializing DirectInput (all devices)" );

			WriteLine( "...finding all devices..." );

			var directInput = new DirectInput();

			SerializableDictionary<Guid, string> deviceList = [];

			DeviceType[] deviceTypeArray = [ DeviceType.Joystick, DeviceType.Gamepad, DeviceType.Driving, DeviceType.Flight, DeviceType.FirstPerson, DeviceType.ControlDevice, DeviceType.ScreenPointer, DeviceType.Remote, DeviceType.Supplemental ];

			foreach ( var deviceType in deviceTypeArray )
			{
				var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.AttachedOnly );

				foreach ( var joystickDeviceInstance in deviceInstanceList )
				{
					WriteLine( $"...we found the {joystickDeviceInstance.ProductName} device..." );

					var hasForceFeedback = joystickDeviceInstance.ForceFeedbackDriverGuid != Guid.Empty;

					if ( hasForceFeedback )
					{
						deviceList.Add( joystickDeviceInstance.InstanceGuid, joystickDeviceInstance.ProductName );
					}

					WriteLine( $"...this devices identifies as Type: {joystickDeviceInstance.Type}, Subtype: {joystickDeviceInstance.Subtype}, Has FF: {hasForceFeedback}..." );

					var joystick = new Joystick( directInput, joystickDeviceInstance.InstanceGuid );

					joystick.Properties.BufferSize = 128;

					WriteLine( $"...setting the cooperative level (non-exclusive background) on this device..." );

					joystick.SetCooperativeLevel( windowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Background );

					WriteLine( $"...cooperative level was set..." );
					WriteLine( $"...acquiring this device..." );

					joystick.Acquire();

					WriteLine( $"...device was acquired..." );

					_joystickList.Add( joystick );
				}
			}

			Settings.UpdateDeviceList( deviceList );

			if ( _joystickList.Count > 0 )
			{
				WriteLine( $"...a total of {_joystickList.Count} devices were found." );
			}
			else
			{
				WriteLine( $"...no devices were found!" );
			}
		}

		public PressedButton? GetAnyPressedButton()
		{
			foreach ( var joystick in _joystickList )
			{
				try
				{
					joystick.Poll();

					var joystickUpdateArray = joystick.GetBufferedData();

					foreach ( var joystickUpdate in joystickUpdateArray )
					{
						if ( ( joystickUpdate.Offset >= JoystickOffset.Buttons0 ) && ( joystickUpdate.Offset <= JoystickOffset.Buttons127 ) )
						{
							if ( joystickUpdate.Value != 0 )
							{
								var pressedButton = new PressedButton
								{
									deviceProductName = joystick.Information.ProductName,
									deviceInstanceGuid = joystick.Information.InstanceGuid,
									buttonNumber = joystickUpdate.Offset - JoystickOffset.Buttons0
								};

								return pressedButton;
							}
						}
					}
				}
				catch ( Exception )
				{
					joystick.Acquire();
				}
			}

			return null;
		}

	}
}
