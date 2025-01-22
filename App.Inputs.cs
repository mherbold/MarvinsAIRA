
using SharpDX.DirectInput;
using System.Windows;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly List<Joystick> _joystickList = [];
		private readonly List<Keyboard> _keyboardList = [];

		private bool _shiftIsDown = false;
		private bool _ctrlIsDown = false;
		private bool _altIsDown = false;

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
			WriteLine( "Initializing DirectInput (all devices)" );

			WriteLine( "...finding all input controller devices..." );

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
						var keyboard = new Keyboard( directInput );

						keyboard.Properties.BufferSize = 128;

						WriteLine( $"...setting the cooperative level (non-exclusive background) on this device..." );

						keyboard.SetCooperativeLevel( windowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Background );

						WriteLine( $"...cooperative level was set..." );
						WriteLine( $"...acquiring this device..." );

						keyboard.Acquire();

						WriteLine( $"...device was acquired..." );

						_keyboardList.Add( keyboard );
					}

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

			Settings.UpdateFFBDeviceList( ffbDeviceList );

			WriteLine( $"...a total of {_joystickList.Count} controller devices were found (and {_keyboardList.Count} keyboards)." );
		}

		public void UpdateInputs()
		{
			foreach ( var joystick in _joystickList )
			{
				joystick.Poll();
			}

			_shiftIsDown = false;
			_ctrlIsDown = false;
			_altIsDown = false;

			foreach ( var keyboard in _keyboardList )
			{
				var keyboardState = keyboard.GetCurrentState();

				_shiftIsDown |= keyboardState.PressedKeys.Contains( Key.LeftShift ) || keyboardState.PressedKeys.Contains( Key.RightShift );
				_ctrlIsDown |= keyboardState.PressedKeys.Contains( Key.LeftControl ) || keyboardState.PressedKeys.Contains( Key.RightControl );
				_altIsDown |= keyboardState.PressedKeys.Contains( Key.LeftAlt ) || keyboardState.PressedKeys.Contains( Key.RightAlt );
			}
		}

		public PressedButton? GetAnyPressedButton()
		{
			foreach ( var joystick in _joystickList )
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
					if ( ( joystickUpdate.Value != 0 ) && ( !mappedButton.UseShift || _shiftIsDown ) && ( !mappedButton.UseCtrl || _ctrlIsDown ) && ( !mappedButton.UseAlt || _altIsDown ) )
					{
						buttonPressCount++;
					}
				}
			}

			return buttonPressCount;
		}
	}
}
