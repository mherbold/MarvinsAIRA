
using SharpDX.DirectInput;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly List<Joystick> _input_joystickList = [];
		private readonly List<Keyboard> _input_keyboardList = [];

		private bool _input_shiftIsDown = false;
		private bool _input_ctrlIsDown = false;
		private bool _input_altIsDown = false;

		public class PressedButton
		{
			public required string deviceProductName;
			public Guid deviceInstanceGuid;
			public int buttonNumber;
		}

		private void InitializeInputs( nint windowHandle )
		{
			WriteLine( "" );
			WriteLine( "InitializeInputs called." );
			WriteLine( "...initializing DirectInput (all devices)..." );

			var directInput = new DirectInput();

			SerializableDictionary<Guid, string> ffbDeviceList = [];

			DeviceType[] deviceTypeArray = [ DeviceType.Keyboard, DeviceType.Joystick, DeviceType.Gamepad, DeviceType.Driving, DeviceType.Flight, DeviceType.FirstPerson, DeviceType.ControlDevice, DeviceType.ScreenPointer, DeviceType.Remote, DeviceType.Supplemental ];

			foreach ( var deviceType in deviceTypeArray )
			{
				var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.AttachedOnly );

				foreach ( var joystickDeviceInstance in deviceInstanceList )
				{
					WriteLine( $"...we found the {joystickDeviceInstance.ProductName} device..." );

					var hasForceFeedback = joystickDeviceInstance.ForceFeedbackDriverGuid != Guid.Empty;

					if ( hasForceFeedback )
					{
						ffbDeviceList.Add( joystickDeviceInstance.InstanceGuid, joystickDeviceInstance.ProductName );
					}

					WriteLine( $"...this devices identifies as Type: {joystickDeviceInstance.Type}, Subtype: {joystickDeviceInstance.Subtype}, Has FF: {hasForceFeedback}..." );

					if ( deviceType == DeviceType.Keyboard )
					{
						WriteLine( $"...creating keyboard type interface..." );

						var keyboard = new Keyboard( directInput );

						keyboard.Properties.BufferSize = 128;

						WriteLine( $"...setting the cooperative level (non-exclusive background) on this device..." );

						keyboard.SetCooperativeLevel( windowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Background );

						WriteLine( $"...cooperative level was set..." );
						WriteLine( $"...acquiring this device..." );

						keyboard.Acquire();

						WriteLine( $"...device was acquired..." );

						_input_keyboardList.Add( keyboard );
					}

					WriteLine( $"...creating joystick type interface..." );

					var joystick = new Joystick( directInput, joystickDeviceInstance.InstanceGuid );

					joystick.Properties.BufferSize = 128;

					WriteLine( $"...setting the cooperative level (non-exclusive background) on this device..." );

					joystick.SetCooperativeLevel( windowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Background );

					WriteLine( $"...cooperative level was set..." );
					WriteLine( $"...acquiring this device..." );

					joystick.Acquire();

					WriteLine( $"...device was acquired..." );

					_input_joystickList.Add( joystick );
				}
			}

			Settings.UpdateFFBDeviceList( ffbDeviceList );

			WriteLine( $"...a total of {_input_joystickList.Count} controller devices were found (and {_input_keyboardList.Count} keyboards)." );
		}

		public void UpdateInputs()
		{
			foreach ( var joystick in _input_joystickList )
			{
				joystick.Poll();
			}

			_input_shiftIsDown = false;
			_input_ctrlIsDown = false;
			_input_altIsDown = false;

			foreach ( var keyboard in _input_keyboardList )
			{
				var keyboardState = keyboard.GetCurrentState();

				_input_shiftIsDown |= keyboardState.PressedKeys.Contains( Key.LeftShift ) || keyboardState.PressedKeys.Contains( Key.RightShift );
				_input_ctrlIsDown |= keyboardState.PressedKeys.Contains( Key.LeftControl ) || keyboardState.PressedKeys.Contains( Key.RightControl );
				_input_altIsDown |= keyboardState.PressedKeys.Contains( Key.LeftAlt ) || keyboardState.PressedKeys.Contains( Key.RightAlt );
			}
		}

		public PressedButton? GetAnyPressedButton()
		{
			foreach ( var joystick in _input_joystickList )
			{
				try
				{
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

		private int GetButtonPressCount( JoystickUpdate[] joystickUpdateArray, Settings.MappedButton mappedButton )
		{
			var buttonPressCount = 0;

			foreach ( var joystickUpdate in joystickUpdateArray )
			{
				if ( joystickUpdate.Offset == JoystickOffset.Buttons0 + mappedButton.ButtonNumber )
				{
					if ( ( joystickUpdate.Value != 0 ) && ( !mappedButton.UseShift || _input_shiftIsDown ) && ( !mappedButton.UseCtrl || _input_ctrlIsDown ) && ( !mappedButton.UseAlt || _input_altIsDown ) )
					{
						buttonPressCount++;
					}
				}
			}

			return buttonPressCount;
		}
	}
}
