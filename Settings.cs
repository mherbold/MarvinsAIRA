
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using SharpDX.DirectInput;

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
					var app = (App) Application.Current;

					app.WriteLine( $"ForceFeedbackEnabled changed - before {_forceFeedbackEnabled} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SelectedFFBDeviceGuid changed - before {_selectedFFBDeviceGuid} now {value}" );

					_selectedFFBDeviceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		/* Reset force feedback */

		private MappedButtons _reinitForceFeedbackButtons = new();

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
				value = Math.Clamp( value, 2, 50 );

				if ( _wheelMaxForce != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"WheelMaxForce changed - before {_wheelMaxForce} now {value}" );

					_wheelMaxForce = value;

					OnPropertyChanged();
				}

				WheelMaxForceString = $"!{_wheelMaxForce:F1} N⋅m";
			}
		}

		private string _wheelMaxForceString = "10 N⋅m";

		public string WheelMaxForceString
		{
			get => _wheelMaxForceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _wheelMaxForceString != value )
					{
						_wheelMaxForceString = value;

						OnPropertyChanged();
					}
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
				value = Math.Clamp( value, 2, 50 );

				if ( _targetForce != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"TargetForce changed - before {_targetForce} now {value}" );

					_targetForce = value;

					OnPropertyChanged();
				}

				TargetForceString = $"!{_targetForce:F1} N⋅m";
			}
		}

		private string _targetForceString = "5 N⋅m";

		public string TargetForceString
		{
			get => _targetForceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _targetForceString != value )
					{
						_targetForceString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Auto overall scale */

		private MappedButtons _autoOverallScaleButtons = new();

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
				value = Math.Clamp( value, 1, 250 );

				if ( _overallScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OverallScale changed - before {_overallScale} now {value}" );

					_overallScale = value;

					OnPropertyChanged();
				}

				OverallScaleString = $"!{_overallScale}%";
			}
		}

		private string _overallScaleString = "10%";

		public string OverallScaleString
		{
			get => _overallScaleString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _overallScaleString != value )
					{
						_overallScaleString = value;

						OnPropertyChanged();
					}
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
				value = Math.Clamp( value, 0, 500 );

				if ( _detailScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"DetailScale changed - before {_detailScale} now {value}" );

					_detailScale = value;

					OnPropertyChanged();
				}

				DetailScaleString = $"!{_detailScale}%";
			}
		}

		private string _detailScaleString = "100%";

		public string DetailScaleString
		{
			get => _detailScaleString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _detailScaleString != value )
					{
						_detailScaleString = value;

						OnPropertyChanged();
					}
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

		private int _parkedScale = 25;

		public int ParkedScale
		{
			get => _parkedScale;

			set
			{
				value = Math.Clamp( value, 0, 100 );

				if ( _parkedScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"ParkedScale changed - before {_parkedScale} now {value}" );

					_parkedScale = value;

					OnPropertyChanged();
				}

				ParkedScaleString = $"!{_parkedScale}%";
			}
		}

		private string _parkedScaleString = "25%";

		public string ParkedScaleString
		{
			get => _parkedScaleString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _parkedScaleString != value )
					{
						_parkedScaleString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Frequency */

		private int _frequency = 16;

		public int Frequency
		{
			get => _frequency;

			set
			{
				value = Math.Clamp( value, 2, 16 );

				if ( _frequency != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"Frequency changed - before {_frequency} now {value}" );

					_frequency = value;

					OnPropertyChanged();
				}

				FrequencyString = $"{1000.0f / ( 18 - _frequency ):F2} Hz";
			}
		}

		private string _frequencyString = "500 Hz";

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

		#region Steering effects tab

		/* Steering effects enabled */

		private bool _steeringEffectsEnabled = false;

		public bool SteeringEffectsEnabled
		{
			get => _steeringEffectsEnabled;

			set
			{
				if ( _steeringEffectsEnabled != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SteeringEffectsEnabled changed - before {_steeringEffectsEnabled} now {value}" );

					_steeringEffectsEnabled = value;

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
					var app = (App) Application.Current;

					app.WriteLine( $"USEffectStyle changed - before {_usEffectStyle} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"USEffectStyleInvert changed - before {_usEffectStyleInvert} now {value}" );

					_usEffectStyleInvert = value;

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
				value = Math.Clamp( value, 0, 100 );

				if ( _usEffectStrength != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USEffectStrength changed - before {_usEffectStrength} now {value}" );

					_usEffectStrength = value;

					OnPropertyChanged();
				}

				USEffectStrengthString = $"!{_usEffectStrength}%";
			}
		}

		private string _usEffectStrengthString = "0%";

		public string USEffectStrengthString
		{
			get => _usEffectStrengthString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usEffectStrengthString != value )
					{
						_usEffectStrengthString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect yaw rate factor (left) */

		private int _usYawRateFactorLeft = 0;

		public int USYawRateFactorLeft
		{
			get => _usYawRateFactorLeft;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _usYawRateFactorLeft != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USYawRateFactorLeft changed - before {_usYawRateFactorLeft} now {value}" );

					_usYawRateFactorLeft = value;

					OnPropertyChanged();
				}

				USYawRateFactorLeftString = $"!{_usYawRateFactorLeft}";
			}
		}

		private string _usYawRateFactorLeftString = "0";

		public string USYawRateFactorLeftString
		{
			get => _usYawRateFactorLeftString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usYawRateFactorLeftString != value )
					{
						_usYawRateFactorLeftString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect yaw rate factor (right) */

		private int _usYawRateFactorRight = 0;

		public int USYawRateFactorRight
		{
			get => _usYawRateFactorRight;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _usYawRateFactorRight != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USYawRateFactorRight changed - before {_usYawRateFactorRight} now {value}" );

					_usYawRateFactorRight = value;

					OnPropertyChanged();
				}

				USYawRateFactorRightString = $"!{_usYawRateFactorRight}";
			}
		}

		private string _usYawRateFactorRightString = "0";

		public string USYawRateFactorRightString
		{
			get => _usYawRateFactorRightString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usYawRateFactorRightString != value )
					{
						_usYawRateFactorRightString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect tolernce */

		private int _usTolerance = 20;

		public int USTolerance
		{
			get => _usTolerance;

			set
			{
				value = Math.Clamp( value, 1, 60 );

				if ( _usTolerance != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USTolerance changed - before {_usTolerance} now {value}" );

					_usTolerance = value;

					OnPropertyChanged();
				}

				USToleranceString = $"!{_usTolerance}°/sec";
			}
		}

		private string _usToleranceString = "20°/sec";

		public string USToleranceString
		{
			get => _usToleranceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usToleranceString != value )
					{
						_usToleranceString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect curve */

		private float _usCurve = 1.5f;

		public float USCurve
		{
			get => _usCurve;

			set
			{
				value = Math.Clamp( value, 0.25f, 4f );

				if ( _usCurve != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USCurve changed - before {_usCurve} now {value}" );

					_usCurve = value;

					OnPropertyChanged();
				}

				USCurveString = $"!{_usCurve:F2}";
			}
		}

		private string _usCurveString = "1.5";

		public string USCurveString
		{
			get => _usCurveString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usCurveString != value )
					{
						_usCurveString = value;

						OnPropertyChanged();
					}
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

		/* Oversteer effect style */

		public bool OSEffectStyleSineWaveBuzz { get; set; } = true;
		public bool OSEffectStyleSawtoothWaveBuzz { get; set; } = false;
		public bool OSEffectStyleConstantForce { get; set; } = false;

		private int _osEffectStyle = 0;

		public int OSEffectStyle
		{
			get => _osEffectStyle;

			set
			{
				if ( _osEffectStyle != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSEffectStyle changed - before {_osEffectStyle} now {value}" );

					_osEffectStyle = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _osEffectStyleInvert = false;

		public bool OSEffectStyleInvert
		{
			get => _osEffectStyleInvert;

			set
			{
				if ( _osEffectStyleInvert != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSEffectStyleInvert changed - before {_osEffectStyleInvert} now {value}" );

					_osEffectStyleInvert = value;

					OnPropertyChanged();
				}
			}
		}

		/* Oversteer effect strength */

		private int _osEffectStrength = 0;

		public int OSEffectStrength
		{
			get => _osEffectStrength;

			set
			{
				value = Math.Clamp( value, 0, 100 );

				if ( _osEffectStrength != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSEffectStrength changed - before {_osEffectStrength} now {value}" );

					_osEffectStrength = value;

					OnPropertyChanged();
				}

				OSEffectStrengthString = $"!{_osEffectStrength}%";
			}
		}

		private string _osEffectStrengthString = "0%";

		public string OSEffectStrengthString
		{
			get => _osEffectStrengthString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osEffectStrengthString != value )
					{
						_osEffectStrengthString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Oversteer effect yaw rate factor (left) */

		private int _osYawRateFactorLeft = 0;

		public int OSYawRateFactorLeft
		{
			get => _osYawRateFactorLeft;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _osYawRateFactorLeft != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSYawRateFactorLeft changed - before {_osYawRateFactorLeft} now {value}" );

					_osYawRateFactorLeft = value;

					OnPropertyChanged();
				}

				OSYawRateFactorLeftString = $"!{_osYawRateFactorLeft}";
			}
		}

		private string _osYawRateFactorLeftString = "0";

		public string OSYawRateFactorLeftString
		{
			get => _osYawRateFactorLeftString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osYawRateFactorLeftString != value )
					{
						_osYawRateFactorLeftString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Oversteer effect yaw rate factor (right) */

		private int _osYawRateFactorRight = 0;

		public int OSYawRateFactorRight
		{
			get => _osYawRateFactorRight;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _osYawRateFactorRight != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSYawRateFactorRight changed - before {_osYawRateFactorRight} now {value}" );

					_osYawRateFactorRight = value;

					OnPropertyChanged();
				}

				OSYawRateFactorRightString = $"!{_osYawRateFactorRight}";
			}
		}

		private string _osYawRateFactorRightString = "0";

		public string OSYawRateFactorRightString
		{
			get => _osYawRateFactorRightString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osYawRateFactorRightString != value )
					{
						_osYawRateFactorRightString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Oversteer effect tolernce */

		private int _osTolerance = 20;

		public int OSTolerance
		{
			get => _osTolerance;

			set
			{
				value = Math.Clamp( value, 1, 60 );

				if ( _osTolerance != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSTolerance changed - before {_osTolerance} now {value}" );

					_osTolerance = value;

					OnPropertyChanged();
				}

				OSToleranceString = $"!{_osTolerance}°/sec";
			}
		}

		private string _osToleranceString = "20°/sec";

		public string OSToleranceString
		{
			get => _osToleranceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osToleranceString != value )
					{
						_osToleranceString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Oversteer effect curve */

		private float _osCurve = 1.5f;

		public float OSCurve
		{
			get => _osCurve;

			set
			{
				value = Math.Clamp( value, 0.25f, 4f );

				if ( _osCurve != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSCurve changed - before {_osCurve} now {value}" );

					_osCurve = value;

					OnPropertyChanged();
				}

				OSCurveString = $"!{_osCurve:F2}";
			}
		}

		private string _osCurveString = "1.5";

		public string OSCurveString
		{
			get => _osCurveString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osCurveString != value )
					{
						_osCurveString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Oversteer effect button */

		private MappedButtons _oversteerEffectButtons = new();

		public MappedButtons OversteerEffectButtons
		{
			get => _oversteerEffectButtons;

			set
			{
				if ( _oversteerEffectButtons != value )
				{
					_oversteerEffectButtons = value;

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
					var app = (App) Application.Current;

					app.WriteLine( $"LFEToFFBEnabled changed - before {_lfeToFFBEnabled} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SelectedLFEDeviceGuid changed - before {_selectedLFEDeviceGuid} now {value}" );

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
				value = Math.Clamp( value, 0, 100 );

				if ( _lfeScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"LFEScale changed - before {_lfeScale} now {value}" );

					_lfeScale = value;

					OnPropertyChanged();
				}

				LFEScaleString = $"!{_lfeScale}%";
			}
		}

		private string _lfeScaleString = "50%";

		public string LFEScaleString
		{
			get => _lfeScaleString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _lfeScaleString != value )
					{
						_lfeScaleString = value;

						OnPropertyChanged();
					}
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
		}

		public List<ForceFeedbackSettings> ForceFeedbackSettingsList { get; private set; } = [];

		/* Per-car steering effects settings */

		public class SteeringEffectsSettings
		{
			public string CarName = "";

			public int USYawRateFactorLeft = 0;
			public int USYawRateFactorRight = 0;

			public int OSYawRateFactorLeft = 0;
			public int OSYawRateFactorRight = 0;
		}

		public List<SteeringEffectsSettings> SteeringEffectsSettingsList { get; private set; } = [];

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
					var app = (App) Application.Current;

					app.WriteLine( $"WindSimulatorEnabled changed - before {_windSimulatorEnabled} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"StartMinimized changed - before {_startMinimized} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"TopmostWindow changed - before {_topmostWindow} now {value}" );

					_topmostWindow = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Save file tab

		private bool _saveSettingsPerWheel = false;

		public bool SaveSettingsPerWheel
		{
			get => _saveSettingsPerWheel;

			set
			{
				if ( _saveSettingsPerWheel != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SaveSettingsPerWheel changed - before {_saveSettingsPerWheel} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SaveSettingsPerCar changed - before {_saveSettingsPerCar} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SaveSettingsPerTrack changed - before {_saveSettingsPerTrack} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SaveSettingsPerTrackConfig changed - before {_saveSettingsPerTrackConfiguration} now {value}" );

					_saveSettingsPerTrackConfiguration = value;

					OnPropertyChanged();
				}
			}
		}


		#endregion

		#region Settings tab - Audio tab

		/* Enable click sound */

		private bool _enableClickSound = false;

		public bool EnableClickSound
		{
			get => _enableClickSound;

			set
			{
				if ( _enableClickSound != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"EnableClickSound changed - before {_enableClickSound} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"ClickSoundVolume changed - before {_clickSoundVolume} now {value}" );

					_clickSoundVolume = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Voice tab

		/* Enable speech synthesizer */

		private bool _enableSpeechSynthesizer = false;

		public bool EnableSpeechSynthesizer
		{
			get => _enableSpeechSynthesizer;

			set
			{
				if ( _enableSpeechSynthesizer != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"EnableSpeechSynthesizer changed - before {_enableSpeechSynthesizer} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SpeechSynthesizerVolume changed - before {_speechSynthesizerVolume} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"SelectedVoice changed - before {_selectedVoice} now {value}" );

					_selectedVoice = value;

					OnPropertyChanged();
				}
			}
		}

		/* Various translations */

		private string _sayHello = "Hello!";
		private string _sayConnected = "We are connected to the iRacing simulator.";
		private string _sayDisconnected = "We have been disconnected from the iRacing simulator.";
		private string _sayVoiceVolume = "My voice is now at :value: percent.";
		private string _sayCarName = "You are driving a :value:.";
		private string _sayTrackName = "You are racing at :value:.";
		private string _sayTrackConfigName = ":value:.";
		private string _sayOverallScale = "The overall scale is now :value: percent.";
		private string _sayDetailScale = "The detail scale is now :value: percent.";
		private string _sayLFEScale = "The LFE scale is now :value: percent.";
		private string _sayScalesReset = "This is the first time you have driven this combination, so we have reset the overall and detail scale.";
		private string _sayLoadOverallScale = "The overall scale has been restored to :value: percent";
		private string _sayLoadDetailScale = "The detail scale has been restored to :value: percent.";
		private string _sayUSLeftYawRateFactor = "The understeer effect left yaw rate factor has been set to :value:.";
		private string _sayUSRightYawRateFactor = "The understeer effect right yaw rate factor has been set to :value:.";
		private string _sayOSLeftYawRateFactor = "The oversteer effect left yaw rate factor has been set to :value:.";
		private string _sayOSRightYawRateFactor = "The oversteer effect right yaw rate factor has been set to :value:.";

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
		public string SayUSLeftYawRateFactor { get => _sayUSLeftYawRateFactor; set { if ( _sayUSLeftYawRateFactor != value ) { _sayUSLeftYawRateFactor = value; OnPropertyChanged(); } } }
		public string SayUSRightYawRateFactor { get => _sayUSRightYawRateFactor; set { if ( _sayUSRightYawRateFactor != value ) { _sayUSRightYawRateFactor = value; OnPropertyChanged(); } } }
		public string SayOSLeftYawRateFactor { get => _sayOSLeftYawRateFactor; set { if ( _sayOSLeftYawRateFactor != value ) { _sayOSLeftYawRateFactor = value; OnPropertyChanged(); } } }
		public string SayOSRightYawRateFactor { get => _sayOSRightYawRateFactor; set { if ( _sayOSRightYawRateFactor != value ) { _sayOSRightYawRateFactor = value; OnPropertyChanged(); } } }

		#endregion

		#region Settings tab - Devices

		/* Reinitialize when devices changed */

		private bool _reinitializeWhenDevicesChanged = true;

		public bool ReinitializeWhenDevicesChanged
		{
			get => _reinitializeWhenDevicesChanged;

			set
			{
				if ( _reinitializeWhenDevicesChanged != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"ReinitializeWhenDevicesChanged changed - before {_reinitializeWhenDevicesChanged} now {value}" );

					_reinitializeWhenDevicesChanged = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Wheel tab

		private bool _autoCenterWheel = true;

		public bool AutoCenterWheel
		{
			get => _autoCenterWheel;

			set
			{
				if ( _autoCenterWheel != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"AutoCenterWheel changed - before {_autoCenterWheel} now {value}" );

					_autoCenterWheel = value;

					OnPropertyChanged();
				}
			}
		}

		private JoystickOffset _selectedWheelAxis = JoystickOffset.X;

		public JoystickOffset SelectedWheelAxis
		{
			get => _selectedWheelAxis;

			set
			{
				if ( _selectedWheelAxis != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SelectedWheelAxis changed - before {_selectedWheelAxis} now {value}" );

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
					var app = (App) Application.Current;

					app.WriteLine( $"WheelMinValue changed - before {_wheelMinValue} now {value}" );

					_wheelMinValue = value;

					OnPropertyChanged();
				}

				WheelMinValueString = _wheelMinValue.ToString();
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
					var app = (App) Application.Current;

					app.WriteLine( $"WheelCenterValue changed - before {_wheelCenterValue} now {value}" );

					_wheelCenterValue = value;

					OnPropertyChanged();
				}

				WheelCenterValueString = _wheelCenterValue.ToString();
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
					var app = (App) Application.Current;

					app.WriteLine( $"WheelMaxValue changed - before {_wheelMaxValue} now {value}" );

					_wheelMaxValue = value;

					OnPropertyChanged();
				}

				WheelMaxValueString = _wheelMaxValue.ToString();
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

		private int _autoCenterWheelStrength = 25;

		public int AutoCenterWheelStrength
		{
			get => _autoCenterWheelStrength;

			set
			{
				value = Math.Clamp( value, 1, 100 );

				if ( _autoCenterWheelStrength != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"AutoCenterWheelStrength changed - before {_autoCenterWheelStrength} now {value}" );

					_autoCenterWheelStrength = value;

					OnPropertyChanged();
				}

				AutoCenterWheelStrengthString = $"!{_autoCenterWheelStrength}%";
			}
		}

		private string _autoCenterWheelStrengthString = "25%";

		public string AutoCenterWheelStrengthString
		{
			get => _autoCenterWheelStrengthString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _autoCenterWheelStrengthString != value )
					{
						_autoCenterWheelStrengthString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		private int _autoCenterWheelType = 0;

		public int AutoCenterWheelType
		{
			get => _autoCenterWheelType;

			set
			{
				if ( _autoCenterWheelType != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"AutoCenterWheelType changed - before {_autoCenterWheelType} now {value}" );

					_autoCenterWheelType = value;

					OnPropertyChanged();
				}
			}
		}

		private bool _enableSoftLock = true;

		public bool EnableSoftLock
		{
			get => _enableSoftLock;

			set
			{
				if ( _enableSoftLock != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"EnableSoftLock changed - before {_enableSoftLock} now {value}" );

					_enableSoftLock = value;

					OnPropertyChanged();
				}
			}
		}

		private int _softLockStrength = 35;

		public int SoftLockStrength
		{
			get => _softLockStrength;

			set
			{
				value = Math.Clamp( value, 0, 100 );

				if ( _softLockStrength != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SoftLockStrength changed - before {_softLockStrength} now {value}" );

					_softLockStrength = value;

					OnPropertyChanged();
				}

				SoftLockStrengthString = $"!{_softLockStrength}%";
			}
		}

		private string _softLockStrengthString = "35%";

		public string SoftLockStrengthString
		{
			get => _softLockStrengthString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _softLockStrengthString != value )
					{
						_softLockStrengthString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		private int _softLockMargin = 20;

		public int SoftLockMargin
		{
			get => _softLockMargin;

			set
			{
				value = Math.Clamp( value, 1, 90 );

				if ( _softLockMargin != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SoftLockMargin changed - before {_softLockMargin} now {value}" );

					_softLockMargin = value;

					OnPropertyChanged();
				}

				SoftLockMarginString = $"!{_softLockMargin}°";
			}
		}

		private string _softLockMarginString = "20°";

		public string SoftLockMarginString
		{
			get => _softLockMarginString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _softLockMarginString != value )
					{
						_softLockMarginString = value;

						OnPropertyChanged();
					}
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
					var app = (App) Application.Current;

					app.WriteLine( $"EnableCrashProtection changed - before {_enableCrashProtection} now {value}" );

					_enableCrashProtection = value;

					OnPropertyChanged();
				}
			}
		}

		private float _gForce = 2f;

		public float GForce
		{
			get => _gForce;

			set
			{
				value = Math.Clamp( value, 1, 10 );

				if ( _gForce != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"GForce changed - before {_gForce} now {value}" );

					_gForce = value;

					OnPropertyChanged();
				}

				GForceString = $"!{(float) _gForce:F1} G";
			}
		}

		private string _gForceString = "2.0 G";

		public string GForceString
		{
			get => _gForceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _gForceString != value )
					{
						_gForceString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		private float _crashDuration = 2f;

		public float CrashDuration
		{
			get => _crashDuration;

			set
			{
				value = Math.Clamp( value, 1, 10 );

				if ( _crashDuration != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"CrashDuration changed - before {_crashDuration} now {value}" );

					_crashDuration = value;

					OnPropertyChanged();
				}

				CrashDurationString = $"!{(float) _crashDuration:F1} sec";
			}
		}

		private string _crashDurationString = "2.0 sec";

		public string CrashDurationString
		{
			get => _crashDurationString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _crashDurationString != value )
					{
						_crashDurationString = value;

						OnPropertyChanged();
					}
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
					var app = (App) Application.Current;

					app.WriteLine( $"PlaybackSendToDevice changed - before {_playbackSendToDevice} now {value}" );

					_playbackSendToDevice = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Advanced

		private bool _advanced = false;

		public bool Advanced
		{
			get => _advanced;

			set
			{
				if ( _advanced != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"Advanced changed - before {_advanced} now {value}" );

					_advanced = value;

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
