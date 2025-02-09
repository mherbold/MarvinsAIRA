
using System.Windows;

using SharpDX.DirectInput;

using static MarvinsAIRA.Settings;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		public class PressedButton
		{
			public Guid DeviceInstanceGuid { get; set; } = Guid.Empty;
			public string DeviceProductName { get; set; } = string.Empty;
			public int ButtonNumber { get; set; } = 0;
		}

		public PressedButton AnyPressedButton { get; private set; } = new();

		private readonly List<Joystick> _input_joystickList = [];

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

			WriteLine( $"...a total of {_input_joystickList.Count} controller devices were found." );
		}

		public void UpdateInputs()
		{
			MappedButtons[] mappedButtonsList = [
				Settings.ReinitForceFeedbackButtons,
				Settings.AutoOverallScaleButtons,
				Settings.IncreaseOverallScaleButtons,
				Settings.DecreaseOverallScaleButtons,
				Settings.IncreaseDetailScaleButtons,
				Settings.DecreaseDetailScaleButtons,
				Settings.IncreaseLFEScaleButtons,
				Settings.DecreaseLFEScaleButtons,
				Settings.UndersteerEffectButtons
			];

			foreach ( var mappedButton in mappedButtonsList )
			{
				mappedButton.Button1Held = false;
				mappedButton.ClickCount = 0;
			}

			AnyPressedButton.DeviceInstanceGuid = Guid.Empty;
			AnyPressedButton.DeviceProductName = string.Empty;
			AnyPressedButton.ButtonNumber = 0;

			JoystickState joystickState = new();

			foreach ( var joystick in _input_joystickList )
			{
				joystick.GetCurrentState( ref joystickState );

				foreach ( var mappedButtons in mappedButtonsList )
				{
					if ( mappedButtons.Button2.DeviceInstanceGuid != Guid.Empty )
					{
						if ( mappedButtons.Button1.DeviceInstanceGuid == joystick.Information.InstanceGuid )
						{
							if ( joystickState.Buttons[ mappedButtons.Button1.ButtonNumber ] )
							{
								mappedButtons.Button1Held = true;
							}
						}
					}
				}
			}

			foreach ( var joystick in _input_joystickList )
			{
				try
				{
					joystick.Poll();

					var joystickUpdateArray = joystick.GetBufferedData();

					if ( ( joystickUpdateArray.Length > 0 ) && ( AnyPressedButton.DeviceInstanceGuid == Guid.Empty ) )
					{
						foreach ( var joystickUpdate in joystickUpdateArray )
						{
							if ( ( joystickUpdateArray[ 0 ].Offset >= JoystickOffset.Buttons0 ) && ( joystickUpdateArray[ 0 ].Offset >= JoystickOffset.Buttons127 ) )
							{
								if ( joystickUpdate.Value != 0 )
								{
									AnyPressedButton.DeviceInstanceGuid = joystick.Information.InstanceGuid;
									AnyPressedButton.DeviceProductName = joystick.Information.ProductName;
									AnyPressedButton.ButtonNumber = joystickUpdateArray[ 0 ].Offset - JoystickOffset.Buttons0;

									break;
								}
							}
						}
					}

					foreach ( var mappedButtons in mappedButtonsList )
					{
						if ( ( mappedButtons.Button1.DeviceInstanceGuid == joystick.Information.InstanceGuid ) && ( mappedButtons.Button2.DeviceInstanceGuid == Guid.Empty ) )
						{
							foreach ( var joystickUpdate in joystickUpdateArray )
							{
								if ( joystickUpdate.Offset == JoystickOffset.Buttons0 + mappedButtons.Button1.ButtonNumber )
								{
									if ( joystickUpdate.Value != 0 )
									{
										mappedButtons.ClickCount++;
									}
								}
							}
						}
						else if ( mappedButtons.Button2.DeviceInstanceGuid == joystick.Information.InstanceGuid )
						{
							if ( mappedButtons.Button1Held )
							{
								foreach ( var joystickUpdate in joystickUpdateArray )
								{
									if ( joystickUpdate.Offset == JoystickOffset.Buttons0 + mappedButtons.Button2.ButtonNumber )
									{
										if ( joystickUpdate.Value != 0 )
										{
											mappedButtons.ClickCount++;
										}
									}
								}
							}
						}
					}
				}
				catch ( Exception )
				{
					joystick.Acquire();
				}
			}
		}
	}
}
