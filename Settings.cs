
using SharpDX.DirectInput;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MarvinsAIRA
{
	public class Settings : INotifyPropertyChanged
	{
		public class MappedButton
		{
			public Guid DeviceInstanceGuid { get; set; } = Guid.Empty;
			public string DeviceProductName { get; set; } = string.Empty;
			public int ButtonNumber { get; set; } = 0;
		}

		public class MappedButtons
		{
			public MappedButton Button1 { get; set; } = new();
			public MappedButton Button2 { get; set; } = new();
			public bool Button1Held { get; set; } = false;
			public int ClickCount { get; set; } = 0;
		}

		#region Force feedback tab

		/* Force feedback enabled */

		private bool _forceFeedbackEnabled = true;

		public bool ForceFeedbackEnabled
		{
			get => _forceFeedbackEnabled;

			set
			{
				if ( _forceFeedbackEnabled != value )
				{
					_forceFeedbackEnabled = value;

					OnPropertyChanged();
				}
			}
		}

		/* Selected FFB device */

		private Guid _selectedFFBDeviceGuid = Guid.Empty;

		public Guid SelectedFFBDeviceGuid
		{
			get => _selectedFFBDeviceGuid;

			set
			{
				if ( _selectedFFBDeviceGuid != value )
				{
					_selectedFFBDeviceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		/* Reset force feedback */

		public MappedButtons _reinitForceFeedbackButtons = new();

		public MappedButtons ReinitForceFeedbackButtons
		{
			get => _reinitForceFeedbackButtons;

			set
			{
				if ( _reinitForceFeedbackButtons != value )
				{
					_reinitForceFeedbackButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Wheel max force */

		private float _wheelMaxForce = 10f;

		public float WheelMaxForce
		{
			get => _wheelMaxForce;

			set
			{
				value = Math.Max( 1, Math.Min( 500, value ) );

				if ( _wheelMaxForce != value )
				{
					_wheelMaxForce = value;

					WheelMaxForceString = $"{_wheelMaxForce:F1} N⋅m";

					OnPropertyChanged();
				}
			}
		}

		private string _wheelMaxForceString = "10 N⋅m";

		public string WheelMaxForceString
		{
			get => _wheelMaxForceString;

			set
			{
				if ( _wheelMaxForceString != value )
				{
					_wheelMaxForceString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Target force */

		private float _targetForce = 5f;

		public float TargetForce
		{
			get => _targetForce;

			set
			{
				value = Math.Max( 1, Math.Min( 250, value ) );

				if ( _targetForce != value )
				{
					_targetForce = value;

					TargetForceString = $"{_targetForce:F1} N⋅m";

					OnPropertyChanged();
				}
			}
		}

		private string _targetForceString = "5 N⋅m";

		public string TargetForceString
		{
			get => _targetForceString;

			set
			{
				if ( _targetForceString != value )
				{
					_targetForceString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Auto overall scale */

		public MappedButtons _autoOverallScaleButtons = new();

		public MappedButtons AutoOverallScaleButtons
		{
			get => _autoOverallScaleButtons;

			set
			{
				if ( _autoOverallScaleButtons != value )
				{
					_autoOverallScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Overall scale */

		private int _overallScale = 10;

		public int OverallScale
		{
			get => _overallScale;

			set
			{
				value = Math.Max( 1, Math.Min( 250, value ) );

				if ( _overallScale != value )
				{
					_overallScale = value;

					OverallScaleString = $"{_overallScale}%";

					OnPropertyChanged();
				}
			}
		}

		private string _overallScaleString = "10%";

		public string OverallScaleString
		{
			get => _overallScaleString;

			set
			{
				if ( _overallScaleString != value )
				{
					_overallScaleString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Detail scale */

		private int _detailScale = 100;

		public int DetailScale
		{
			get => _detailScale;

			set
			{
				value = Math.Max( 0, Math.Min( 500, value ) );

				if ( _detailScale != value )
				{
					_detailScale = value;

					DetailScaleString = $"{_detailScale}%";

					OnPropertyChanged();
				}
			}
		}

		private string _detailScaleString = "100%";

		public string DetailScaleString
		{
			get => _detailScaleString;

			set
			{
				if ( _detailScaleString != value )
				{
					_detailScaleString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Decrease overall scale */

		private MappedButtons _decreaseOverallScaleButtons = new();

		public MappedButtons DecreaseOverallScaleButtons
		{
			get => _decreaseOverallScaleButtons;

			set
			{
				if ( _decreaseOverallScaleButtons != value )
				{
					_decreaseOverallScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Increase overall scale */

		private MappedButtons _increaseOverallScaleButtons = new();

		public MappedButtons IncreaseOverallScaleButtons
		{
			get => _increaseOverallScaleButtons;

			set
			{
				if ( _increaseOverallScaleButtons != value )
				{
					_increaseOverallScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Decrease detail scale */

		private MappedButtons _decreaseDetailScaleButtons = new();

		public MappedButtons DecreaseDetailScaleButtons
		{
			get => _decreaseDetailScaleButtons;

			set
			{
				if ( _decreaseDetailScaleButtons != value )
				{
					_decreaseDetailScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Increase detail scale */

		private MappedButtons _increaseDetailScaleButtons = new();

		public MappedButtons IncreaseDetailScaleButtons
		{
			get => _increaseDetailScaleButtons;

			set
			{
				if ( _increaseDetailScaleButtons != value )
				{
					_increaseDetailScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Parked scale */

		private int _parkedScale = 10;

		public int ParkedScale
		{
			get => _parkedScale;

			set
			{
				value = Math.Max( 0, Math.Min( 100, value ) );

				if ( _parkedScale != value )
				{
					_parkedScale = value;

					ParkedScaleString = $"{_parkedScale}%";

					OnPropertyChanged();
				}
			}
		}

		private string _parkedScaleString = "10%";

		public string ParkedScaleString
		{
			get => _parkedScaleString;

			set
			{
				if ( _parkedScaleString != value )
				{
					_parkedScaleString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Frequency */

		private int _frequency = 14;

		public int Frequency
		{
			get => _frequency;

			set
			{
				value = Math.Max( 2, Math.Min( 16, value ) );

				if ( _frequency != value )
				{
					_frequency = value;

					FrequencyString = $"{1000.0f / ( 18 - _frequency ):F2} Hz";

					OnPropertyChanged();
				}
			}
		}

		private string _frequencyString = "250 Hz";

		public string FrequencyString
		{
			get => _frequencyString;

			set
			{
				if ( _frequencyString != value )
				{
					_frequencyString = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Understeer effect tab

		/* Understeer effect enabled */

		private bool _understeerEffectEnabled = false;

		public bool UndersteerEffectEnabled
		{
			get => _understeerEffectEnabled;

			set
			{
				if ( _understeerEffectEnabled != value )
				{
					_understeerEffectEnabled = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect style */

		public bool USEffectStyleSineWaveBuzz { get; set; } = true;
		public bool USEffectStyleSawtoothWaveBuzz { get; set; } = false;
		public bool USEffectStyleConstantForce { get; set; } = false;

		private int _usEffectStyle = 0;

		public int USEffectStyle
		{
			get => _usEffectStyle;

			set
			{
				if ( _usEffectStyle != value )
				{
					_usEffectStyle = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _usEffectStyleInvert = false;

		public bool USEffectStyleInvert
		{
			get => _usEffectStyleInvert;

			set
			{
				if ( _usEffectStyleInvert != value )
				{
					_usEffectStyleInvert = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect strength */

		private int _usEffectStrength = 10;

		public int USEffectStrength
		{
			get => _usEffectStrength;

			set
			{
				value = Math.Max( 0, Math.Min( 100, value ) );

				if ( _usEffectStrength != value )
				{
					_usEffectStrength = value;

					USEffectStrengthString = $"{_usEffectStrength}%";

					OnPropertyChanged();
				}
			}
		}

		private string _usEffectStrengthString = "10%";

		public string USEffectStrengthString
		{
			get => _usEffectStrengthString;

			set
			{
				if ( _usEffectStrengthString != value )
				{
					_usEffectStrengthString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect yaw rate factor (left) */

		private int _usYawRateFactorLeft = 80;

		public int USYawRateFactorLeft
		{
			get => _usYawRateFactorLeft;

			set
			{
				value = Math.Max( 0, Math.Min( 200, value ) );

				if ( _usYawRateFactorLeft != value )
				{
					_usYawRateFactorLeft = value;

					USYawRateFactorLeftString = $"{_usYawRateFactorLeft}";

					OnPropertyChanged();
				}
			}
		}

		private string _usYawRateFactorLeftString = "80";

		public string USYawRateFactorLeftString
		{
			get => _usYawRateFactorLeftString;

			set
			{
				if ( _usYawRateFactorLeftString != value )
				{
					_usYawRateFactorLeftString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect yaw rate factor (right) */

		private int _usYawRateFactorRight = 80;

		public int USYawRateFactorRight
		{
			get => _usYawRateFactorRight;

			set
			{
				value = Math.Max( 0, Math.Min( 200, value ) );

				if ( _usYawRateFactorRight != value )
				{
					_usYawRateFactorRight = value;

					USYawRateFactorRightString = $"{_usYawRateFactorRight}";

					OnPropertyChanged();
				}
			}
		}

		private string _usYawRateFactorRightString = "80";

		public string USYawRateFactorRightString
		{
			get => _usYawRateFactorRightString;

			set
			{
				if ( _usYawRateFactorRightString != value )
				{
					_usYawRateFactorRightString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect button */

		private MappedButtons _understeerEffectButtons = new();

		public MappedButtons UndersteerEffectButtons
		{
			get => _understeerEffectButtons;

			set
			{
				if ( _understeerEffectButtons != value )
				{
					_understeerEffectButtons = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region LFE to FFB tab

		/* LFE to FFB enabled */

		private bool _lfeToFFBEnabled = false;

		public bool LFEToFFBEnabled
		{
			get => _lfeToFFBEnabled;

			set
			{
				if ( _lfeToFFBEnabled != value )
				{
					_lfeToFFBEnabled = value;

					OnPropertyChanged();
				}
			}
		}

		/* Selected LFE device */

		private Guid _selectedLFEDeviceGuid = Guid.Empty;

		public Guid SelectedLFEDeviceGuid
		{
			get => _selectedLFEDeviceGuid;

			set
			{
				if ( _selectedLFEDeviceGuid != value )
				{
					_selectedLFEDeviceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		/* LFE scale */

		private int _lfeScale = 50;

		public int LFEScale
		{
			get => _lfeScale;

			set
			{
				value = Math.Max( 0, Math.Min( 250, value ) );

				if ( _lfeScale != value )
				{
					_lfeScale = value;

					LFEScaleString = $"{_lfeScale}%";

					OnPropertyChanged();
				}
			}
		}

		private string _lfeScaleString = "50%";

		public string LFEScaleString
		{
			get => _lfeScaleString;

			set
			{
				if ( _lfeScaleString != value )
				{
					_lfeScaleString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Decrease LFE scale */

		private MappedButtons _decreaseLFEScaleButtons = new();

		public MappedButtons DecreaseLFEScaleButtons
		{
			get => _decreaseLFEScaleButtons;

			set
			{
				if ( _decreaseLFEScaleButtons != value )
				{
					_decreaseLFEScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Increase LFE scale */

		private MappedButtons _increaseLFEScaleButtons = new();

		public MappedButtons IncreaseLFEScaleButtons
		{
			get => _increaseLFEScaleButtons;

			set
			{
				if ( _increaseLFEScaleButtons != value )
				{
					_increaseLFEScaleButtons = value;

					OnPropertyChanged();
				}
			}
		}

		/* Per-car/track/track config force feedback settings */

		public class ForceFeedbackSettings
		{
			public string WheelName = "";
			public string CarName = "";
			public string TrackName = "";
			public string TrackConfigName = "";

			public int OverallScale = 10;
			public int DetailScale = 100;

			public int USEffectStrength = 0;
			public int USYawRateFactorLeft = 0;
			public int USYawRateFactorRight = 0;
		}

		public List<ForceFeedbackSettings> ForceFeedbackSettingsList { get; private set; } = [];

		#endregion

		#region Wind simulator tab

		/* Wind simulator enabled */

		private bool _windSimulatorEnabled = false;

		public bool WindSimulatorEnabled
		{
			get => _windSimulatorEnabled;

			set
			{
				if ( _windSimulatorEnabled != value )
				{
					_windSimulatorEnabled = value;

					OnPropertyChanged();
				}
			}
		}

		/* Car speed */

		private int _carSpeed1 = 0;
		private int _carSpeed2 = 5;
		private int _carSpeed3 = 25;
		private int _carSpeed4 = 50;
		private int _carSpeed5 = 75;
		private int _carSpeed6 = 100;
		private int _carSpeed7 = 150;
		private int _carSpeed8 = 200;

		public int CarSpeed1
		{
			get => _carSpeed1;

			set
			{
				if ( value != _carSpeed1 )
				{
					_carSpeed1 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed2
		{
			get => _carSpeed2;

			set
			{
				if ( value != _carSpeed2 )
				{
					_carSpeed2 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed3
		{
			get => _carSpeed3;

			set
			{
				if ( value != _carSpeed3 )
				{
					_carSpeed3 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed4
		{
			get => _carSpeed4;

			set
			{
				if ( value != _carSpeed4 )
				{
					_carSpeed4 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed5
		{
			get => _carSpeed5;

			set
			{
				if ( value != _carSpeed5 )
				{
					_carSpeed5 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed6
		{
			get => _carSpeed6;

			set
			{
				if ( value != _carSpeed6 )
				{
					_carSpeed6 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed7
		{
			get => _carSpeed7;

			set
			{
				if ( value != _carSpeed7 )
				{
					_carSpeed7 = value;

					OnPropertyChanged();
				}
			}
		}

		public int CarSpeed8
		{
			get => _carSpeed8;

			set
			{
				if ( value != _carSpeed8 )
				{
					_carSpeed8 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Wind force */

		private float _windForce1 = 0;
		private float _windForce2 = 3.5f;
		private float _windForce3 = 4.5f;
		private float _windForce4 = 8f;
		private float _windForce5 = 15f;
		private float _windForce6 = 35f;
		private float _windForce7 = 70f;
		private float _windForce8 = 100f;

		public float WindForce1
		{
			get => _windForce1;

			set
			{
				if ( value != _windForce1 )
				{
					_windForce1 = value;

					WindForceString1 = $"{_windForce1:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce2
		{
			get => _windForce2;

			set
			{
				if ( value != _windForce2 )
				{
					_windForce2 = value;

					WindForceString2 = $"{_windForce2:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce3
		{
			get => _windForce3;

			set
			{
				if ( value != _windForce3 )
				{
					_windForce3 = value;

					WindForceString3 = $"{_windForce3:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce4
		{
			get => _windForce4;

			set
			{
				if ( value != _windForce4 )
				{
					_windForce4 = value;

					WindForceString4 = $"{_windForce4:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce5
		{
			get => _windForce5;

			set
			{
				if ( value != _windForce5 )
				{
					_windForce5 = value;

					WindForceString5 = $"{_windForce5:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce6
		{
			get => _windForce6;

			set
			{
				if ( value != _windForce6 )
				{
					_windForce6 = value;

					WindForceString6 = $"{_windForce6:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce7
		{
			get => _windForce7;

			set
			{
				if ( value != _windForce7 )
				{
					_windForce7 = value;

					WindForceString7 = $"{_windForce7:F1}%";

					OnPropertyChanged();
				}
			}
		}

		public float WindForce8
		{
			get => _windForce8;

			set
			{
				if ( value != _windForce8 )
				{
					_windForce8 = value;

					WindForceString8 = $"{_windForce8:F1}%";

					OnPropertyChanged();
				}
			}
		}

		/* Wind force string */

		private string _windForceString1 = "0%";
		private string _windForceString2 = "3.5%";
		private string _windForceString3 = "4.5%";
		private string _windForceString4 = "8%";
		private string _windForceString5 = "15%";
		private string _windForceString6 = "35%";
		private string _windForceString7 = "70%";
		private string _windForceString8 = "100%";

		public string WindForceString1
		{
			get => _windForceString1;

			set
			{
				if ( value != _windForceString1 )
				{
					_windForceString1 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString2
		{
			get => _windForceString2;

			set
			{
				if ( value != _windForceString2 )
				{
					_windForceString2 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString3
		{
			get => _windForceString3;

			set
			{
				if ( value != _windForceString3 )
				{
					_windForceString3 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString4
		{
			get => _windForceString4;

			set
			{
				if ( value != _windForceString4 )
				{
					_windForceString4 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString5
		{
			get => _windForceString5;

			set
			{
				if ( value != _windForceString5 )
				{
					_windForceString5 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString6
		{
			get => _windForceString6;

			set
			{
				if ( value != _windForceString6 )
				{
					_windForceString6 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString7
		{
			get => _windForceString7;

			set
			{
				if ( value != _windForceString7 )
				{
					_windForceString7 = value;

					OnPropertyChanged();
				}
			}
		}

		public string WindForceString8
		{
			get => _windForceString8;

			set
			{
				if ( value != _windForceString8 )
				{
					_windForceString8 = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Window tab

		/* Start minimized */

		private bool _startMinimized = false;

		public bool StartMinimized
		{
			get => _startMinimized;

			set
			{
				if ( _startMinimized != value )
				{
					_startMinimized = value;

					OnPropertyChanged();
				}
			}
		}

		/* Topmost window */

		private bool _topmostWindow = false;

		public bool TopmostWindow
		{
			get => _topmostWindow;

			set
			{
				if ( _topmostWindow != value )
				{
					_topmostWindow = value;

					OnPropertyChanged();
				}
			}
		}


		#endregion

		#region Settings tab - Save file tab

		private bool _saveSettingsPerWheel = true;

		public bool SaveSettingsPerWheel
		{
			get => _saveSettingsPerWheel;

			set
			{
				if ( _saveSettingsPerWheel != value )
				{
					_saveSettingsPerWheel = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _saveSettingsPerCar = true;

		public bool SaveSettingsPerCar
		{
			get => _saveSettingsPerCar;

			set
			{
				if ( _saveSettingsPerCar != value )
				{
					_saveSettingsPerCar = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _saveSettingsPerTrack = false;

		public bool SaveSettingsPerTrack
		{
			get => _saveSettingsPerTrack;

			set
			{
				if ( _saveSettingsPerTrack != value )
				{
					_saveSettingsPerTrack = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _saveSettingsPerTrackConfiguration = false;

		public bool SaveSettingsPerTrackConfig
		{
			get => _saveSettingsPerTrackConfiguration;

			set
			{
				if ( _saveSettingsPerTrackConfiguration != value )
				{
					_saveSettingsPerTrackConfiguration = value;

					OnPropertyChanged();
				}
			}
		}


		#endregion

		#region Settings tab - Audio tab

		/* Enable click sound */

		private bool _enableClickSound = true;

		public bool EnableClickSound
		{
			get => _enableClickSound;

			set
			{
				if ( _enableClickSound != value )
				{
					_enableClickSound = value;

					OnPropertyChanged();
				}
			}
		}

		/* Click sound volume */

		private int _clickSoundVolume = 75;

		public int ClickSoundVolume
		{
			get => _clickSoundVolume;

			set
			{
				if ( _clickSoundVolume != value )
				{
					_clickSoundVolume = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Voice tab

		/* Enable speech synthesizer */

		private bool _enableSpeechSynthesizer = true;

		public bool EnableSpeechSynthesizer
		{
			get => _enableSpeechSynthesizer;

			set
			{
				if ( _enableSpeechSynthesizer != value )
				{
					_enableSpeechSynthesizer = value;

					OnPropertyChanged();
				}
			}
		}

		/* Speech synthesizer volume */

		private int _speechSynthesizerVolume = 100;

		public int SpeechSynthesizerVolume
		{
			get => _speechSynthesizerVolume;

			set
			{
				if ( _speechSynthesizerVolume != value )
				{
					_speechSynthesizerVolume = value;

					OnPropertyChanged();
				}
			}
		}

		/* Speech synthesizer gender */

		private string _selectedVoice = string.Empty;

		public string SelectedVoice
		{
			get => _selectedVoice;

			set
			{
				if ( _selectedVoice != value )
				{
					_selectedVoice = value;

					OnPropertyChanged();
				}
			}
		}

		/* Various translations */

		public string _sayHello = "Hello!";
		public string _sayConnected = "We are connected to the iRacing simulator.";
		public string _sayDisconnected = "We have been disconnected from the iRacing simulator.";
		public string _sayVoiceVolume = "My voice is now at :value: percent.";
		public string _sayCarName = "You are driving a :value:.";
		public string _sayTrackName = "You are racing at :value:.";
		public string _sayTrackConfigName = ":value:.";
		public string _sayOverallScale = "The overall scale is now :value: percent.";
		public string _sayDetailScale = "The detail scale is now :value: percent.";
		public string _sayLFEScale = "The LFE scale is now :value: percent.";
		public string _sayScalesReset = "This is the first time you have driven this combination, so we have reset the overall and detail scale.";
		public string _sayLoadOverallScale = "The overall scale has been restored to :value: percent";
		public string _sayLoadDetailScale = "The detail scale has been restored to :value: percent.";
		public string _sayLeftYawRateFactor = "The understeer effect left yaw rate factor has been set to :value:.";
		public string _sayRightYawRateFactor = "The understeer effect right yaw rate factor has been set to :value:.";

		public string SayHello { get => _sayHello; set { if ( _sayHello != value ) { _sayHello = value; OnPropertyChanged(); } } }
		public string SayConnected { get => _sayConnected; set { if ( _sayConnected != value ) { _sayConnected = value; OnPropertyChanged(); } } }
		public string SayDisconnected { get => _sayDisconnected; set { if ( _sayDisconnected != value ) { _sayDisconnected = value; OnPropertyChanged(); } } }
		public string SayVoiceVolume { get => _sayVoiceVolume; set { if ( _sayVoiceVolume != value ) { _sayVoiceVolume = value; OnPropertyChanged(); } } }
		public string SayCarName { get => _sayCarName; set { if ( _sayCarName != value ) { _sayCarName = value; OnPropertyChanged(); } } }
		public string SayTrackName { get => _sayTrackName; set { if ( _sayTrackName != value ) { _sayTrackName = value; OnPropertyChanged(); } } }
		public string SayTrackConfigName { get => _sayTrackConfigName; set { if ( _sayTrackConfigName != value ) { _sayTrackConfigName = value; OnPropertyChanged(); } } }
		public string SayOverallScale { get => _sayOverallScale; set { if ( _sayOverallScale != value ) { _sayOverallScale = value; OnPropertyChanged(); } } }
		public string SayDetailScale { get => _sayDetailScale; set { if ( _sayDetailScale != value ) { _sayDetailScale = value; OnPropertyChanged(); } } }
		public string SayLFEScale { get => _sayLFEScale; set { if ( _sayLFEScale != value ) { _sayLFEScale = value; OnPropertyChanged(); } } }
		public string SayScalesReset { get => _sayScalesReset; set { if ( _sayScalesReset != value ) { _sayScalesReset = value; OnPropertyChanged(); } } }
		public string SayLoadOverallScale { get => _sayLoadOverallScale; set { if ( _sayLoadOverallScale != value ) { _sayLoadOverallScale = value; OnPropertyChanged(); } } }
		public string SayLoadDetailScale { get => _sayLoadDetailScale; set { if ( _sayLoadDetailScale != value ) { _sayLoadDetailScale = value; OnPropertyChanged(); } } }
		public string SayLeftYawRateFactor { get => _sayLeftYawRateFactor; set { if ( _sayLeftYawRateFactor != value ) { _sayLeftYawRateFactor = value; OnPropertyChanged(); } } }
		public string SayRightYawRateFactor { get => _sayRightYawRateFactor; set { if ( _sayRightYawRateFactor != value ) { _sayRightYawRateFactor = value; OnPropertyChanged(); } } }

		#endregion

		#region Settings tab - Wheel tab

		private JoystickOffset _selectedWheelAxis = JoystickOffset.X;

		public JoystickOffset SelectedWheelAxis
		{
			get => _selectedWheelAxis;

			set
			{
				if ( _selectedWheelAxis != value )
				{
					_selectedWheelAxis = value;

					OnPropertyChanged();
				}
			}
		}

		private string _wheelAxisValueString = "0";

		public string WheelAxisValueString
		{
			get => _wheelAxisValueString;

			set
			{
				_wheelAxisValueString = value;

				OnPropertyChanged( false );
			}
		}

		private int _wheelMinValue = 0;

		public int WheelMinValue
		{
			get => _wheelMinValue;

			set
			{
				if ( _wheelMinValue != value )
				{
					_wheelMinValue = value;

					WheelMinValueString = _wheelMinValue.ToString();

					OnPropertyChanged();
				}
			}
		}

		private string _wheelMinValueString = "0";

		public string WheelMinValueString
		{
			get => _wheelMinValueString;

			set
			{
				if ( _wheelMinValueString != value )
				{
					_wheelMinValueString = value;

					OnPropertyChanged();
				}
			}
		}

		private int _wheelCenterValue = 0;

		public int WheelCenterValue
		{
			get => _wheelCenterValue;

			set
			{
				if ( _wheelCenterValue != value )
				{
					_wheelCenterValue = value;

					WheelCenterValueString = _wheelCenterValue.ToString();

					OnPropertyChanged();
				}
			}
		}

		private string _wheelCenterValueString = "0";

		public string WheelCenterValueString
		{
			get => _wheelCenterValueString;

			set
			{
				if ( _wheelCenterValueString != value )
				{
					_wheelCenterValueString = value;

					OnPropertyChanged();
				}
			}
		}

		private int _wheelMaxValue = 0;

		public int WheelMaxValue
		{
			get => _wheelMaxValue;

			set
			{
				if ( _wheelMaxValue != value )
				{
					_wheelMaxValue = value;

					WheelMaxValueString = _wheelMaxValue.ToString();

					OnPropertyChanged();
				}
			}
		}

		private string _wheelMaxValueString = "0";

		public string WheelMaxValueString
		{
			get => _wheelMaxValueString;

			set
			{
				if ( _wheelMaxValueString != value )
				{
					_wheelMaxValueString = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _autoCenterWheel = true;

		public bool AutoCenterWheel
		{
			get => _autoCenterWheel;

			set
			{
				if ( _autoCenterWheel != value )
				{
					_autoCenterWheel = value;

					OnPropertyChanged();
				}
			}
		}

		private int _autoCenterWheelStrength = 15;

		public int AutoCenterWheelStrength
		{
			get => _autoCenterWheelStrength;

			set
			{
				value = Math.Max( 1, Math.Min( 100, value ) );

				if ( _autoCenterWheelStrength != value )
				{
					_autoCenterWheelStrength = value;

					AutoCenterWheelStrengthString = $"{_autoCenterWheelStrength}%";

					OnPropertyChanged();
				}
			}
		}

		private string _autoCenterWheelStrengthString = "15%";

		public string AutoCenterWheelStrengthString
		{
			get => _autoCenterWheelStrengthString;

			set
			{
				if ( _autoCenterWheelStrengthString != value )
				{
					_autoCenterWheelStrengthString = value;

					OnPropertyChanged();
				}
			}
		}

		private int _autoCenterWheelType = 1;
		
		public int AutoCenterWheelType
		{
			get => _autoCenterWheelType;

			set
			{
				if ( _autoCenterWheelType != value )
				{
					_autoCenterWheelType = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _playbackSendToDevice = true;

		public bool PlaybackSendToDevice
		{
			get => _playbackSendToDevice;

			set
			{
				if ( _playbackSendToDevice != value )
				{
					_playbackSendToDevice = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _enableCrashProtection = false;

		public bool EnableCrashProtection
		{
			get => _enableCrashProtection;

			set
			{
				if ( _enableCrashProtection != value )
				{
					_enableCrashProtection = value;

					OnPropertyChanged();
				}
			}
		}

		private int _gForce = 20;

		public int GForce
		{
			get => _gForce;

			set
			{
				if ( _gForce != value )
				{
					_gForce = value;

					GForceString = $"{(float) _gForce / 10:F1} G";

					OnPropertyChanged();
				}
			}
		}

		private string _gForceString = "2.0 G";

		public string GForceString
		{
			get => _gForceString;

			set
			{
				if ( _gForceString != value )
				{
					_gForceString = value;

					OnPropertyChanged();
				}
			}
		}

		private int _crashDuration = 20;

		public int CrashDuration
		{
			get => _crashDuration;

			set
			{
				if ( _crashDuration != value )
				{
					_crashDuration = value;

					CrashDurationString = $"{(float) _crashDuration / 10:F1} sec";

					OnPropertyChanged();
				}
			}
		}

		private string _crashDurationString = "2.0 sec";

		public string CrashDurationString
		{
			get => _crashDurationString;

			set
			{
				if ( _crashDurationString != value )
				{
					_crashDurationString = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Lists

		private SerializableDictionary<Guid, string> _ffbDeviceList = [];

		public SerializableDictionary<Guid, string> FFBDeviceList { get => _ffbDeviceList; }

		public void UpdateFFBDeviceList( SerializableDictionary<Guid, string> ffbDeviceList )
		{
			_ffbDeviceList = ffbDeviceList;

			OnPropertyChanged( false, nameof( FFBDeviceList ) );
		}

		private SerializableDictionary<Guid, string> _lfeDeviceList = [];

		public SerializableDictionary<Guid, string> LFEDeviceList { get => _lfeDeviceList; }

		public void UpdateLFEDeviceList( SerializableDictionary<Guid, string> lfeDeviceList )
		{
			_lfeDeviceList = lfeDeviceList;

			OnPropertyChanged( false, nameof( LFEDeviceList ) );
		}

		private SerializableDictionary<string, string> _voiceList = [];

		public SerializableDictionary<string, string> VoiceList { get => _voiceList; }

		public void UpdateVoiceList( SerializableDictionary<string, string> voiceList )
		{
			_voiceList = voiceList;

			OnPropertyChanged( false, nameof( VoiceList ) );
		}

		private SerializableDictionary<JoystickOffset, string> _wheelAxisList = [];

		public SerializableDictionary<JoystickOffset, string> WheelAxisList { get => _wheelAxisList; }

		public void UpdateWheelAxisList( SerializableDictionary<JoystickOffset, string> wheelAxisList )
		{
			_wheelAxisList = wheelAxisList;

			OnPropertyChanged( false, nameof( WheelAxisList ) );
		}

		public SerializableDictionary<int, string> AutoCenterWheelTypeList { get; } = new SerializableDictionary<int, string> { { 0, "Slow and Steady" }, { 1, "Springy" } };

		#endregion

		#region Notification functions

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged( bool queueForSerialization = true, [CallerMemberName] string? name = null )
		{
			var app = (App) Application.Current;

			if ( queueForSerialization )
			{
				app.QueueForSerialization();
			}

			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		#endregion
	}
}
