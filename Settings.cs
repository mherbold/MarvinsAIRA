
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MarvinsAIRA
{
	public class Settings : INotifyPropertyChanged
	{
		public class MappedButton
		{
			public bool IsKeyboard = false;
			public bool UseShift = false;
			public bool UseCtrl = false;
			public bool UseAlt = false;
			public Guid DeviceInstanceGuid = Guid.Empty;
			public string DeviceProductName = string.Empty;
			public int ButtonNumber = 0;
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

		/* Device lists */

		private SerializableDictionary<Guid, string> _ffbDeviceList = [];

		public SerializableDictionary<Guid, string> FFBDeviceList { get => _ffbDeviceList; }

		private SerializableDictionary<Guid, string> _lfeDeviceList = [];

		public SerializableDictionary<Guid, string> LFEDeviceList { get => _lfeDeviceList; }

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

		public MappedButton _setForegroundWindow = new();

		public MappedButton SetForegroundWindow
		{
			get => _setForegroundWindow;

			set
			{
				if ( _setForegroundWindow != value )
				{
					_setForegroundWindow = value;

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

		public MappedButton _autoOverallScale = new();

		public MappedButton AutoOverallScale
		{
			get => _autoOverallScale;

			set
			{
				if ( _autoOverallScale != value )
				{
					_autoOverallScale = value;

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

		private MappedButton _decreaseOverallScale = new();

		public MappedButton DecreaseOverallScale
		{
			get => _decreaseOverallScale;

			set
			{
				if ( _decreaseOverallScale != value )
				{
					_decreaseOverallScale = value;

					OnPropertyChanged();
				}
			}
		}

		/* Increase overall scale */

		private MappedButton _increaseOverallScale = new();

		public MappedButton IncreaseOverallScale
		{
			get => _increaseOverallScale;

			set
			{
				if ( _increaseOverallScale != value )
				{
					_increaseOverallScale = value;

					OnPropertyChanged();
				}
			}
		}

		/* Decrease detail scale */

		private MappedButton _decreaseDetailScale = new();

		public MappedButton DecreaseDetailScale
		{
			get => _decreaseDetailScale;

			set
			{
				if ( _decreaseDetailScale != value )
				{
					_decreaseDetailScale = value;

					OnPropertyChanged();
				}
			}
		}

		/* Increase detail scale */

		private MappedButton _increaseDetailScale = new();

		public MappedButton IncreaseDetailScale
		{
			get => _increaseDetailScale;

			set
			{
				if ( _increaseDetailScale != value )
				{
					_increaseDetailScale = value;

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

		/* Understeer effect strength */

		private int _usEffectStrength = 0;

		public int USEffectStrength
		{
			get => _usEffectStrength;

			set
			{
				value = Math.Max( 0, Math.Min( 100, value ) );

				if ( _usEffectStrength != value )
				{
					_usEffectStrength = value;

					if ( _usEffectStrength == 0 )
					{
						USEffectStrengthString = "OFF";
					}
					else
					{
						USEffectStrengthString = $"{_usEffectStrength}%";
					}

					OnPropertyChanged();
				}
			}
		}

		private string _usEffectStrengthString = "OFF";

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

		/* Understeer effect yaw rate factor */

		private int _usYawRateFactor = 50;

		public int USYawRateFactor
		{
			get => _usYawRateFactor;

			set
			{
				value = Math.Max( 0, Math.Min( 100, value ) );

				if ( _usYawRateFactor != value )
				{
					_usYawRateFactor = value;

					USYawRateFactorString = $"{_usYawRateFactor}";

					OnPropertyChanged();
				}
			}
		}

		private string _usYawRateFactorString = "50";

		public string USYawRateFactorString
		{
			get => _usYawRateFactorString;

			set
			{
				if ( _usYawRateFactorString != value )
				{
					_usYawRateFactorString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect lateral force factor */

		private int _usLateralForceFactor = 50;

		public int USLateralForceFactor
		{
			get => _usLateralForceFactor;

			set
			{
				value = Math.Max( 0, Math.Min( 100, value ) );

				if ( _usLateralForceFactor != value )
				{
					_usLateralForceFactor = value;

					USLateralForceFactorString = $"{_usLateralForceFactor}";

					OnPropertyChanged();
				}
			}
		}

		private string _usLateralForceFactorString = "50";

		public string USLateralForceFactorString
		{
			get => _usLateralForceFactorString;

			set
			{
				if ( _usLateralForceFactorString != value )
				{
					_usLateralForceFactorString = value;

					OnPropertyChanged();
				}
			}
		}

		/* Understeer effect steering wheel offset */

		private int _usSteeringWheelOffset = 0;

		public int USSteeringWheelOffset
		{
			get => _usSteeringWheelOffset;

			set
			{
				value = Math.Max( -100, Math.Min( 100, value ) );

				if ( _usSteeringWheelOffset != value )
				{
					_usSteeringWheelOffset = value;

					USSteeringWheelOffsetString = $"{_usSteeringWheelOffset}%";

					OnPropertyChanged();
				}
			}
		}

		private string _usSteeringWheelOffsetString = "50%";

		public string USSteeringWheelOffsetString
		{
			get => _usSteeringWheelOffsetString;

			set
			{
				if ( _usSteeringWheelOffsetString != value )
				{
					_usSteeringWheelOffsetString = value;

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

		private MappedButton _decreaseLFEScale = new();

		public MappedButton DecreaseLFEScale
		{
			get => _decreaseLFEScale;

			set
			{
				if ( _decreaseLFEScale != value )
				{
					_decreaseLFEScale = value;

					OnPropertyChanged();
				}
			}
		}

		/* Increase LFE scale */

		private MappedButton _increaseLFEScale = new();

		public MappedButton IncreaseLFEScale
		{
			get => _increaseLFEScale;

			set
			{
				if ( _increaseLFEScale != value )
				{
					_increaseLFEScale = value;

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
			public int USYawRateFactor = 0;
			public int USLateralForceFactor = 0;
			public int USSteeringWheelOffset = 0;
		}

		public List<ForceFeedbackSettings> ForceFeedbackSettingsList { get; private set; } = [];

		#endregion

		#region Wind simulator tab

		/* Wind simulator enabled */

		private bool _windSimulatorEnabled = true;

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

		#region General settings tab

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

		/* Force feedback settings save options*/

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

		#endregion

		#region Device lists

		public void UpdateFFBDeviceList( SerializableDictionary<Guid, string> ffbDeviceList )
		{
			_ffbDeviceList = ffbDeviceList;

			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( FFBDeviceList ) ) );
		}

		public void UpdateLFEDeviceList( SerializableDictionary<Guid, string> lfeDeviceList )
		{
			_lfeDeviceList = lfeDeviceList;

			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( LFEDeviceList ) ) );
		}

		#endregion

		#region Notification functions

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged( [CallerMemberName] string? name = null )
		{
			var app = (App) Application.Current;

			app.QueueForSerialization();

			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		#endregion
	}
}
