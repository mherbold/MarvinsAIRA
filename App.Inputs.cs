﻿
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

		public PressedButton Input_AnyPressedButton { get; private set; } = new();

		private readonly List<Joystick> _input_joystickList = [];

		private int _input_currentWheelPosition = -1;
		private int _input_currentWheelVelocity = 0;

		public int Input_CurrentWheelPosition { get => _input_currentWheelPosition; }
		public int Input_CurrentWheelVelocity { get => _input_currentWheelVelocity; }

		public void InitializeInputs( nint windowHandle )
		{
			WriteLine( "InitializeInputs called.", true );
			WriteLine( "...initializing DirectInput (all devices)..." );

			_input_joystickList.Clear();

			var directInput = new DirectInput();

			SerializableDictionary<Guid, string> ffbDeviceList = [];

			DeviceType[] deviceTypeArray = [ DeviceType.Keyboard, DeviceType.Joystick, DeviceType.Gamepad, DeviceType.Driving, DeviceType.Flight, DeviceType.FirstPerson, DeviceType.ControlDevice, DeviceType.ScreenPointer, DeviceType.Remote, DeviceType.Supplemental ];

			foreach ( var deviceType in deviceTypeArray )
			{
				WriteLine( $"...scanning for {deviceType} devices..." );

				var deviceInstanceList = directInput.GetDevices( deviceType, DeviceEnumerationFlags.AttachedOnly );

				foreach ( var joystickDeviceInstance in deviceInstanceList )
				{
					WriteLine( $"...we found the {joystickDeviceInstance.ProductName} device..." );

					var deviceAlreadyAdded = false;

					foreach ( var otherJoystick in _input_joystickList )
					{
						if ( otherJoystick.Information.InstanceGuid == joystickDeviceInstance.InstanceGuid )
						{
							deviceAlreadyAdded = true;
							break;
						}
					}

					if ( deviceAlreadyAdded )
					{
						WriteLine( $"...this joystick has the same instance GUID as another we've seen already, so skipping this one..." );
					}
					else
					{
						var hasForceFeedback = joystickDeviceInstance.ForceFeedbackDriverGuid != Guid.Empty;

						if ( hasForceFeedback )
						{
							ffbDeviceList.Add( joystickDeviceInstance.InstanceGuid, joystickDeviceInstance.ProductName );
						}

						WriteLine( $"...this devices identifies as Type: {joystickDeviceInstance.Type}, Subtype: {joystickDeviceInstance.Subtype}, Has FF: {hasForceFeedback}, GUID: {joystickDeviceInstance.InstanceGuid}..." );

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
			}

			Settings.UpdateFFBDeviceList( ffbDeviceList );

			WriteLine( $"...a total of {_input_joystickList.Count} controller devices were found." );

			var wheelAxisList = new SerializableDictionary<JoystickOffset, string> {
				{ JoystickOffset.X, "X Axis" },
				{ JoystickOffset.Y, "Y Axis" },
				{ JoystickOffset.Z, "Z Axis" },
				{ JoystickOffset.RotationX, "RX Axis" },
				{ JoystickOffset.RotationY, "RY Axis" },
				{ JoystickOffset.RotationZ, "RZ Axis" },
				{ JoystickOffset.Sliders0, "SX Axis" },
				{ JoystickOffset.Sliders1, "SY Axis" },
			};

			Settings.UpdateWheelAxisList( wheelAxisList );
		}

		public void UpdateInputs( float deltaTime )
		{
			MappedButtons[] mappedButtonsList = [
				Settings.ReinitForceFeedbackButtons,
				Settings.AutoOverallScaleButtons,
				Settings.ClearAutoOverallScaleButtons,
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

			Input_AnyPressedButton.DeviceInstanceGuid = Guid.Empty;
			Input_AnyPressedButton.DeviceProductName = string.Empty;
			Input_AnyPressedButton.ButtonNumber = 0;

			JoystickState joystickState = new();

			foreach ( var joystick in _input_joystickList )
			{
				try
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

					var drivingJoystick = _ffb_drivingJoystick;

					if ( drivingJoystick != null )
					{
						if ( joystick.Information.InstanceGuid == drivingJoystick.Information.InstanceGuid )
						{
							var lastWheelPosition = _input_currentWheelPosition;

							switch ( Settings.SelectedWheelAxis )
							{
								case JoystickOffset.X:
									_input_currentWheelPosition = joystickState.X;
									break;

								case JoystickOffset.Y:
									_input_currentWheelPosition = joystickState.Y;
									break;

								case JoystickOffset.Z:
									_input_currentWheelPosition = joystickState.Z;
									break;

								case JoystickOffset.RotationX:
									_input_currentWheelPosition = joystickState.RotationX;
									break;

								case JoystickOffset.RotationY:
									_input_currentWheelPosition = joystickState.RotationY;
									break;

								case JoystickOffset.RotationZ:
									_input_currentWheelPosition = joystickState.RotationZ;
									break;

								case JoystickOffset.Sliders0:
									_input_currentWheelPosition = joystickState.Sliders[ 0 ];
									break;

								case JoystickOffset.Sliders1:
									_input_currentWheelPosition = joystickState.Sliders[ 1 ];
									break;
							}

							if ( lastWheelPosition != -1 )
							{
								_input_currentWheelVelocity = (int) ( ( _input_currentWheelPosition - lastWheelPosition ) / deltaTime );
							}

							Settings.WheelAxisValueString = _input_currentWheelPosition.ToString();
						}
					}
				}
				catch ( Exception exception )
				{
					WriteLine( $"Exception caught while getting input from {joystick.Information.ProductName}: {exception.Message.Trim()}", true );

					_input_joystickList.Remove( joystick );

					return;
				}
			}

			foreach ( var joystick in _input_joystickList )
			{
				try
				{
					joystick.Poll();

					var joystickUpdateArray = joystick.GetBufferedData();

					if ( joystickUpdateArray.Length > 0 )
					{
						if ( Input_AnyPressedButton.DeviceInstanceGuid == Guid.Empty )
						{
							foreach ( var joystickUpdate in joystickUpdateArray )
							{
								if ( ( joystickUpdate.Offset >= JoystickOffset.PointOfViewControllers0 ) && ( joystickUpdate.Offset <= JoystickOffset.Buttons127 ) )
								{
									if ( joystickUpdate.Value != 0 )
									{
										Input_AnyPressedButton.DeviceInstanceGuid = joystick.Information.InstanceGuid;
										Input_AnyPressedButton.DeviceProductName = joystick.Information.ProductName;
										Input_AnyPressedButton.ButtonNumber = joystickUpdate.Offset - JoystickOffset.Buttons0;

										break;
									}
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
								if ( joystickUpdate.Offset == ( JoystickOffset.Buttons0 + mappedButtons.Button1.ButtonNumber ) )
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
									if ( joystickUpdate.Offset == ( JoystickOffset.Buttons0 + mappedButtons.Button2.ButtonNumber ) )
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
				catch ( Exception exception )
				{
					WriteLine( $"Exception caught while getting input from {joystick.Information.ProductName}: {exception.Message.Trim()}", true );

					_input_joystickList.Remove( joystick );

					return;
				}
			}
		}
	}
}
