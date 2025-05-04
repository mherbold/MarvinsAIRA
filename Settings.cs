﻿
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

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

			[XmlIgnore]
			public bool Button1Held { get; set; } = false;
			[XmlIgnore]
			public int ClickCount { get; set; } = 0;
		}

		#region General

		private float _windowPositionX = -1f;

		public float WindowPositionX
		{
			get => _windowPositionX;

			set
			{
				if ( _windowPositionX != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"WindowPositionX changed - before {_windowPositionX} now {value}" );

					_windowPositionX = value;

					OnPropertyChanged();
				}
			}
		}

		private float _windowPositionY = -1f;

		public float WindowPositionY
		{
			get => _windowPositionY;

			set
			{
				if ( _windowPositionY != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"WindowPositionY changed - before {_windowPositionY} now {value}" );

					_windowPositionY = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

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
				value = Math.Clamp( value, 2f, 50f );

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

		[XmlIgnore]
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

		/* Overall scale */

		private float _overallScale = 10f;

		public float OverallScale
		{
			get => _overallScale;

			set
			{
				value = Math.Clamp( value, 1f, 100f );

				if ( _overallScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OverallScale changed - before {_overallScale:F1} now {value:F1}" );

					_overallScale = value;

					OnPropertyChanged();
				}

				if ( _overallScale < 10f )
				{
					OverallScaleString = $"!{_overallScale:F1}%";
				}
				else
				{
					OverallScaleString = $"!{_overallScale:F0}%";
				}
			}
		}

		private string _overallScaleString = "10%";

		[XmlIgnore]
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

		private MappedButtons _clearAutoOverallScaleButtons = new();

		public MappedButtons ClearAutoOverallScaleButtons
		{
			get => _clearAutoOverallScaleButtons;

			set
			{
				if ( _clearAutoOverallScaleButtons != value )
				{
					_clearAutoOverallScaleButtons = value;

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

		/* Detail scale */

		private float _detailScale = 100f;

		public float DetailScale
		{
			get => _detailScale;

			set
			{
				value = Math.Clamp( value, 0f, 500f );

				if ( _detailScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"DetailScale changed - before {_detailScale:F1} now {value:F1}" );

					_detailScale = value;

					OnPropertyChanged();
				}

				if ( _detailScale < 10f )
				{
					DetailScaleString = $"!{_detailScale:F1}%";
				}
				else
				{
					DetailScaleString = $"!{_detailScale:F0}%";
				}
			}
		}

		private string _detailScaleString = "100%";

		[XmlIgnore]
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

		[XmlIgnore]
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

		/* Min force */

		private float _minForce = 0f;

		public float MinForce
		{
			get => _minForce;

			set
			{
				value = Math.Clamp( value, 0f, 2f );

				if ( _minForce != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"MinForce changed - before {_minForce} now {value}" );

					_minForce = value;

					OnPropertyChanged();
				}

				MinForceString = $"!{_minForce:F2} N⋅m";
			}
		}

		private string _minForceString = "0 N⋅m";

		[XmlIgnore]
		public string MinForceString
		{
			get => _minForceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _minForceString != value )
					{
						_minForceString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Max force */

		private float _maxForce = 50f;

		public float MaxForce
		{
			get => _maxForce;

			set
			{
				value = Math.Clamp( value, 3f, 50f );

				if ( _maxForce != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"MaxForce changed - before {_maxForce} now {value}" );

					_maxForce = value;

					OnPropertyChanged();
				}

				MaxForceString = $"!{_maxForce:F2} N⋅m";
			}
		}

		private string _maxForceString = "50 N⋅m";

		[XmlIgnore]
		public string MaxForceString
		{
			get => _maxForceString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _maxForceString != value )
					{
						_maxForceString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* FFB curve */

		private float _ffbCurve = 1f;

		public float FFBCurve
		{
			get => _ffbCurve;

			set
			{
				value = Math.Clamp( value, 0.5f, 2f );

				if ( _ffbCurve != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"FFBCurve changed - before {_ffbCurve} now {value}" );

					_ffbCurve = value;

					OnPropertyChanged();
				}

				FFBCurveString = $"!{_ffbCurve:F2}";
			}
		}

		private string _ffbCurveString = "1.00";

		[XmlIgnore]
		public string FFBCurveString
		{
			get => _ffbCurveString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _ffbCurveString != value )
					{
						_ffbCurveString = value;

						OnPropertyChanged();
					}
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
		public bool USEffectStyleConstantForceInverted { get; set; } = false;
		public bool USEffectStylePedalHaptics { get; set; } = false;

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

		private int _usEffectStrength = 15;

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

		private string _usEffectStrengthString = "15%";

		[XmlIgnore]
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

		/* Understeer effect yaw rate factor (start left) */

		private int _usStartYawRateFactorLeft = 120;

		public int USStartYawRateFactorLeft
		{
			get => _usStartYawRateFactorLeft;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _usStartYawRateFactorLeft != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USStartYawRateFactorLeft changed - before {_usStartYawRateFactorLeft} now {value}" );

					_usStartYawRateFactorLeft = value;

					OnPropertyChanged();
				}

				USStartYawRateFactorLeftString = $"!{_usStartYawRateFactorLeft}";
			}
		}

		private string _usStartYawRateFactorLeftString = "120";

		[XmlIgnore]
		public string USStartYawRateFactorLeftString
		{
			get => _usStartYawRateFactorLeftString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usStartYawRateFactorLeftString != value )
					{
						_usStartYawRateFactorLeftString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect yaw rate factor (end left) */

		private int _usEndYawRateFactorLeft = 180;

		public int USEndYawRateFactorLeft
		{
			get => _usEndYawRateFactorLeft;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _usEndYawRateFactorLeft != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USEndYawRateFactorLeft changed - before {_usEndYawRateFactorLeft} now {value}" );

					_usEndYawRateFactorLeft = value;

					OnPropertyChanged();
				}

				USEndYawRateFactorLeftString = $"!{_usEndYawRateFactorLeft}";
			}
		}

		private string _usEndYawRateFactorLeftString = "180";

		[XmlIgnore]
		public string USEndYawRateFactorLeftString
		{
			get => _usEndYawRateFactorLeftString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usEndYawRateFactorLeftString != value )
					{
						_usEndYawRateFactorLeftString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect yaw rate factor (start right) */

		private int _usStartYawRateFactorRight = 120;

		public int USStartYawRateFactorRight
		{
			get => _usStartYawRateFactorRight;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _usStartYawRateFactorRight != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USStartYawRateFactorRight changed - before {_usStartYawRateFactorRight} now {value}" );

					_usStartYawRateFactorRight = value;

					OnPropertyChanged();
				}

				USStartYawRateFactorRightString = $"!{_usStartYawRateFactorRight}";
			}
		}

		private string _usStartYawRateFactorRightString = "120";

		[XmlIgnore]
		public string USStartYawRateFactorRightString
		{
			get => _usStartYawRateFactorRightString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usStartYawRateFactorRightString != value )
					{
						_usStartYawRateFactorRightString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Understeer effect yaw rate factor (end right) */

		private int _usEndYawRateFactorRight = 180;

		public int USEndYawRateFactorRight
		{
			get => _usEndYawRateFactorRight;

			set
			{
				value = Math.Clamp( value, 0, 200 );

				if ( _usEndYawRateFactorRight != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"USEndYawRateFactorRight changed - before {_usEndYawRateFactorRight} now {value}" );

					_usEndYawRateFactorRight = value;

					OnPropertyChanged();
				}

				USEndYawRateFactorRightString = $"!{_usEndYawRateFactorRight}";
			}
		}

		private string _usEndYawRateFactorRightString = "180";

		[XmlIgnore]
		public string USEndYawRateFactorRightString
		{
			get => _usEndYawRateFactorRightString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _usEndYawRateFactorRightString != value )
					{
						_usEndYawRateFactorRightString = value;

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

		private string _usCurveString = "1.50";

		[XmlIgnore]
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

		public bool OSEffectStyleSineWaveBuzz { get; set; } = false;
		public bool OSEffectStyleSawtoothWaveBuzz { get; set; } = false;
		public bool OSEffectStyleConstantForce { get; set; } = false;
		public bool OSEffectStyleConstantForceInverted { get; set; } = true;
		public bool OSEffectStylePedalHaptics {  get; set; } = false;

		private int _osEffectStyle = 3;

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

		private bool _osEffectStyleInvert = true;

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

		private int _osEffectStrength = 30;

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

		private string _osEffectStrengthString = "30%";

		[XmlIgnore]
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

		/* Oversteer effect y velocity (start) */

		private float _osStartYVelocity = 3f;

		public float OSStartYVelocity
		{
			get => _osStartYVelocity;

			set
			{
				value = Math.Clamp( value, 0f, 50f );

				if ( _osStartYVelocity != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSStartYVelocity changed - before {_osStartYVelocity} now {value}" );

					_osStartYVelocity = value;

					OnPropertyChanged();
				}

				OSStartYVelocityString = $"!{_osStartYVelocity:F1}";
			}
		}

		private string _osStartYVelocityString = "3.0";

		[XmlIgnore]
		public string OSStartYVelocityString
		{
			get => _osStartYVelocityString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osStartYVelocityString != value )
					{
						_osStartYVelocityString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Oversteer effect y velocity (end) */

		private float _osEndYVelocity = 8f;

		public float OSEndYVelocity
		{
			get => _osEndYVelocity;

			set
			{
				value = Math.Clamp( value, 0f, 50f );

				if ( _osEndYVelocity != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSEndYVelocity changed - before {_osEndYVelocity} now {value}" );

					_osEndYVelocity = value;

					OnPropertyChanged();
				}

				OSEndYVelocityString = $"!{_osEndYVelocity:F1}";
			}
		}

		private string _osEndYVelocityString = "8.0";

		[XmlIgnore]
		public string OSEndYVelocityString
		{
			get => _osEndYVelocityString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osEndYVelocityString != value )
					{
						_osEndYVelocityString = value;

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

		private string _osCurveString = "1.50";

		[XmlIgnore]
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

		/* Oversteer softness */

		private float _osSoftness = 90f;

		public float OSSoftness
		{
			get => _osSoftness;

			set
			{
				value = Math.Clamp( value, 1f, 180f );

				if ( _osSoftness != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"OSSoftness changed - before {_osSoftness} now {value}" );

					_osSoftness = value;

					OnPropertyChanged();
				}

				if ( _osSoftness > 179f )
				{
					OSSoftnessString = "!OFF";
				}
				else
				{
					OSSoftnessString = $"!{_osSoftness:F0}°";
				}
			}
		}

		private string _osSoftnessString = "90°";

		[XmlIgnore]
		public string OSSoftnessString
		{
			get => _osSoftnessString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _osSoftnessString != value )
					{
						_osSoftnessString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Per-car/track/track config force feedback settings */

		public class ForceFeedbackSettings
		{
			public string WheelName = App.ALL_WHEELS_SAVE_NAME;
			public string CarName = App.ALL_CARS_SAVE_NAME;
			public string TrackName = App.ALL_TRACKS_SAVE_NAME;
			public string TrackConfigName = App.ALL_TRACK_CONFIGS_SAVE_NAME;
			public string WetDryConditionName = App.ALL_WETDRYCONDITIONS_SAVE_NAME;

			public float OverallScale = 10f;
			public float DetailScale = 100f;
		}

		public List<ForceFeedbackSettings> ForceFeedbackSettingsList { get; private set; } = [];

		/* Per-car steering effects settings */

		public class SteeringEffectsSettings
		{
			public string CarName = string.Empty;

			public bool SteeringEffectsEnabled = false;

			public int USStartYawRateFactorLeft = 120;
			public int USEndYawRateFactorLeft = 180;
			public int USStartYawRateFactorRight = 120;
			public int USEndYawRateFactorRight = 180;


			public float OSStartYVelocity = 3f;
			public float OSEndYVelocity = 8f;
			public float OSSoftness = 90f;
		}

		public List<SteeringEffectsSettings> SteeringEffectsSettingsList { get; private set; } = [];

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

		private float _lfeScale = 50f;

		public float LFEScale
		{
			get => _lfeScale;

			set
			{
				value = Math.Clamp( value, 0f, 200f );

				if ( _lfeScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"LFEScale changed - before {_lfeScale} now {value}" );

					_lfeScale = value;

					OnPropertyChanged();
				}

				if ( _lfeScale < 10f )
				{
					LFEScaleString = $"!{_lfeScale:F1}%";
				}
				else
				{
					LFEScaleString = $"!{_lfeScale:F0}%";
				}
			}
		}

		private string _lfeScaleString = "50%";

		[XmlIgnore]
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

		#endregion

		#region Pedal haptics

		/* Pedal haptics enabled */

		private bool _pedalHapticsEnabled = false;

		public bool PedalHapticsEnabled
		{
			get => _pedalHapticsEnabled;

			set
			{
				if ( _pedalHapticsEnabled != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsEnabled changed - before {_pedalHapticsEnabled} now {value}" );

					_pedalHapticsEnabled = value;

					OnPropertyChanged();
				}
			}
		}

		/* Clutch effect 1 */

		private int _pedalHapticsClutchEffect1 = 1;

		public int PedalHapticsClutchEffect1
		{
			get => _pedalHapticsClutchEffect1;

			set
			{
				if ( _pedalHapticsClutchEffect1 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsClutchEffect1 changed - before {_pedalHapticsClutchEffect1} now {value}" );

					_pedalHapticsClutchEffect1 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Clutch effect strength 1 */

		private float _pedalHapticsClutchEffectStrength1 = 1f;

		public float PedalHapticsClutchEffectStrength1
		{
			get => _pedalHapticsClutchEffectStrength1;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsClutchEffectStrength1 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsClutchEffectStrength1 changed - before {_pedalHapticsClutchEffectStrength1} now {value}" );

					_pedalHapticsClutchEffectStrength1 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Clutch effect 2 */

		private int _pedalHapticsClutchEffect2 = 8;

		public int PedalHapticsClutchEffect2
		{
			get => _pedalHapticsClutchEffect2;

			set
			{
				if ( _pedalHapticsClutchEffect2 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsClutchEffect2 changed - before {_pedalHapticsClutchEffect2} now {value}" );

					_pedalHapticsClutchEffect2 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Clutch effect strength 2 */

		private float _pedalHapticsClutchEffectStrength2 = 1f;

		public float PedalHapticsClutchEffectStrength2
		{
			get => _pedalHapticsClutchEffectStrength2;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsClutchEffectStrength2 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsClutchEffectStrength2 changed - before {_pedalHapticsClutchEffectStrength2} now {value}" );

					_pedalHapticsClutchEffectStrength2 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Clutch effect 3 */

		private int _pedalHapticsClutchEffect3 = 5;

		public int PedalHapticsClutchEffect3
		{
			get => _pedalHapticsClutchEffect3;

			set
			{
				if ( _pedalHapticsClutchEffect3 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsClutchEffect3 changed - before {_pedalHapticsClutchEffect3} now {value}" );

					_pedalHapticsClutchEffect3 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Clutch effect strength 3 */

		private float _pedalHapticsClutchEffectStrength3 = 1f;

		public float PedalHapticsClutchEffectStrength3
		{
			get => _pedalHapticsClutchEffectStrength3;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsClutchEffectStrength3 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsClutchEffectStrength3 changed - before {_pedalHapticsClutchEffectStrength3} now {value}" );

					_pedalHapticsClutchEffectStrength3 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Brake effect 1 */

		private int _pedalHapticsBrakeEffect1 = 2;

		public int PedalHapticsBrakeEffect1
		{
			get => _pedalHapticsBrakeEffect1;

			set
			{
				if ( _pedalHapticsBrakeEffect1 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsBrakeEffect1 changed - before {_pedalHapticsBrakeEffect1} now {value}" );

					_pedalHapticsBrakeEffect1 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Brake effect strength 1 */

		private float _pedalHapticsBrakeEffectStrength1 = 1f;

		public float PedalHapticsBrakeEffectStrength1
		{
			get => _pedalHapticsBrakeEffectStrength1;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsBrakeEffectStrength1 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsBrakeEffectStrength1 changed - before {_pedalHapticsBrakeEffectStrength1} now {value}" );

					_pedalHapticsBrakeEffectStrength1 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Brake effect 2 */

		private int _pedalHapticsBrakeEffect2 = 6;

		public int PedalHapticsBrakeEffect2
		{
			get => _pedalHapticsBrakeEffect2;

			set
			{
				if ( _pedalHapticsBrakeEffect2 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsBrakeEffect2 changed - before {_pedalHapticsBrakeEffect2} now {value}" );

					_pedalHapticsBrakeEffect2 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Brake effect strength 2 */

		private float _pedalHapticsBrakeEffectStrength2 = 1f;

		public float PedalHapticsBrakeEffectStrength2
		{
			get => _pedalHapticsBrakeEffectStrength2;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsBrakeEffectStrength2 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsBrakeEffectStrength2 changed - before {_pedalHapticsBrakeEffectStrength2} now {value}" );

					_pedalHapticsBrakeEffectStrength2 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Brake effect 3 */

		private int _pedalHapticsBrakeEffect3 = 5;

		public int PedalHapticsBrakeEffect3
		{
			get => _pedalHapticsBrakeEffect3;

			set
			{
				if ( _pedalHapticsBrakeEffect3 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsBrakeEffect3 changed - before {_pedalHapticsBrakeEffect3} now {value}" );

					_pedalHapticsBrakeEffect3 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Brake effect strength 3 */

		private float _pedalHapticsBrakeEffectStrength3 = 1f;

		public float PedalHapticsBrakeEffectStrength3
		{
			get => _pedalHapticsBrakeEffectStrength3;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsBrakeEffectStrength3 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsBrakeEffectStrength3 changed - before {_pedalHapticsBrakeEffectStrength3} now {value}" );

					_pedalHapticsBrakeEffectStrength3 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Throttle effect 1 */

		private int _pedalHapticsThrottleEffect1 = 7;

		public int PedalHapticsThrottleEffect1
		{
			get => _pedalHapticsThrottleEffect1;

			set
			{
				if ( _pedalHapticsThrottleEffect1 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsThrottleEffect1 changed - before {_pedalHapticsThrottleEffect1} now {value}" );

					_pedalHapticsThrottleEffect1 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Throttle effect strength 1 */

		private float _pedalHapticsThrottleEffectStrength1 = 1f;

		public float PedalHapticsThrottleEffectStrength1
		{
			get => _pedalHapticsThrottleEffectStrength1;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsThrottleEffectStrength1 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsThrottleEffectStrength1 changed - before {_pedalHapticsThrottleEffectStrength1} now {value}" );

					_pedalHapticsThrottleEffectStrength1 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Throttle effect 2 */

		private int _pedalHapticsThrottleEffect2 = 3;

		public int PedalHapticsThrottleEffect2
		{
			get => _pedalHapticsThrottleEffect2;

			set
			{
				if ( _pedalHapticsThrottleEffect2 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsThrottleEffect2 changed - before {_pedalHapticsThrottleEffect2} now {value}" );

					_pedalHapticsThrottleEffect2 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Throttle effect strength 2 */

		private float _pedalHapticsThrottleEffectStrength2 = 1f;

		public float PedalHapticsThrottleEffectStrength2
		{
			get => _pedalHapticsThrottleEffectStrength2;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsThrottleEffectStrength2 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsThrottleEffectStrength2 changed - before {_pedalHapticsThrottleEffectStrength2} now {value}" );

					_pedalHapticsThrottleEffectStrength2 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Throttle effect 3 */

		private int _pedalHapticsThrottleEffect3 = 0;

		public int PedalHapticsThrottleEffect3
		{
			get => _pedalHapticsThrottleEffect3;

			set
			{
				if ( _pedalHapticsThrottleEffect3 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsThrottleEffect3 changed - before {_pedalHapticsThrottleEffect3} now {value}" );

					_pedalHapticsThrottleEffect3 = value;

					OnPropertyChanged();
				}
			}
		}

		/* Throttle effect strength 3 */

		private float _pedalHapticsThrottleEffectStrength3 = 1f;

		public float PedalHapticsThrottleEffectStrength3
		{
			get => _pedalHapticsThrottleEffectStrength3;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _pedalHapticsThrottleEffectStrength3 != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PedalHapticsThrottleEffectStrength3 changed - before {_pedalHapticsThrottleEffectStrength3} now {value}" );

					_pedalHapticsThrottleEffectStrength3 = value;

					OnPropertyChanged();
				}
			}
		}

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

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlIgnore]
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

		[XmlIgnore]
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

		#region Spotter tab

		/* Spotter enabled */

		private bool _spotterEnabled = false;

		public bool SpotterEnabled
		{
			get => _spotterEnabled;

			set
			{
				if ( _spotterEnabled != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SpotterEnabled changed - before {_spotterEnabled} now {value}" );

					_spotterEnabled = value;

					OnPropertyChanged();
				}
			}
		}

		/* callout frequency */

		private int _spotterCalloutFrequency = 20;

		public int SpotterCalloutFrequency
		{
			get => _spotterCalloutFrequency;

			set
			{
				value = Math.Clamp( value, 1, 60 );

				if ( _spotterCalloutFrequency != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SpotterCalloutFrequency changed - before {_spotterCalloutFrequency} now {value}" );

					_spotterCalloutFrequency = value;

					OnPropertyChanged();
				}

				SpotterCalloutFrequencyString = $"!{_spotterCalloutFrequency}/min";
			}
		}

		private string _spotterCalloutFrequencyString = "20/min";

		[XmlIgnore]
		public string SpotterCalloutFrequencyString
		{
			get => _spotterCalloutFrequencyString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _spotterCalloutFrequencyString != value )
					{
						_spotterCalloutFrequencyString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		#endregion

		#region Settings tab - App tab

		/* Start with Windows */

		private bool _startWithWindows = false;

		public bool StartWithWindows
		{
			get => _startWithWindows;

			set
			{
				if ( _startWithWindows != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"StartWithWindows changed - before {_startWithWindows} now {value}" );

					_startWithWindows = value;

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
					var app = (App) Application.Current;

					app.WriteLine( $"StartMinimized changed - before {_startMinimized} now {value}" );

					_startMinimized = value;

					OnPropertyChanged();
				}
			}
		}

		/* Minimize to tray */

		private bool _closeToSystemTray = false;

		public bool CloseToSystemTray
		{
			get => _closeToSystemTray;

			set
			{
				if ( _closeToSystemTray != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"CloseToSystemTray changed - before {_closeToSystemTray} now {value}" );

					_closeToSystemTray = value;

					OnPropertyChanged();
				}
			}
		}

		/* Minimize to tray */

		private bool _hideTrayAlert = false;

		public bool HideTrayAlert
		{
			get => _hideTrayAlert;

			set
			{
				if ( _hideTrayAlert != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"HideTrayAlert changed - before {_hideTrayAlert} now {value}" );

					_hideTrayAlert = value;

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

		/* Window opacity */

		private int _windowOpacity = 100;

		public int WindowOpacity
		{
			get => _windowOpacity;

			set
			{
				value = Math.Clamp( value, 25, 100 );

				if ( _windowOpacity != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"WindowOpacity changed - before {_windowOpacity} now {value}" );

					_windowOpacity = value;

					OnPropertyChanged();
				}

				WindowOpacityString = $"!{_windowOpacity}%";
			}
		}

		private string _windowOpacityString = "100%";

		[XmlIgnore]
		public string WindowOpacityString
		{
			get => _windowOpacityString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _windowOpacityString != value )
					{
						_windowOpacityString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Check for updates */

		private bool _checkForUpdates = true;

		public bool CheckForUpdates
		{
			get => _checkForUpdates;

			set
			{
				if ( _checkForUpdates != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"CheckForUpdates changed - before {_checkForUpdates} now {value}" );

					_checkForUpdates = value;

					OnPropertyChanged();
				}
			}
		}

		/* Automatically download updates */

		private bool _automaticallyDownloadUpdates = false;

		public bool AutomaticallyDownloadUpdates
		{
			get => _automaticallyDownloadUpdates;

			set
			{
				if ( _automaticallyDownloadUpdates != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"AutomaticallyDownloadUpdates changed - before {_automaticallyDownloadUpdates} now {value}" );

					_automaticallyDownloadUpdates = value;

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

		private bool _saveSettingsPerWetDryCondition = false;

		public bool SaveSettingsPerWetDryCondition
		{
			get => _saveSettingsPerWetDryCondition;

			set
			{
				if ( _saveSettingsPerWetDryCondition != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"SaveSettingsPerWetDryCondition changed - before {_saveSettingsPerWetDryCondition} now {value}" );

					_saveSettingsPerWetDryCondition = value;

					OnPropertyChanged();
				}
			}
		}

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

		#region Settings tab - Auto centering tab

		private bool _autoCenterWheel = false;

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

		[XmlIgnore]
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

				WheelMinValueString = $"!{_wheelMinValue}";
			}
		}

		private string _wheelMinValueString = "0";

		[XmlIgnore]
		public string WheelMinValueString
		{
			get => _wheelMinValueString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _wheelMinValueString != value )
					{
						_wheelMinValueString = value;

						OnPropertyChanged();
					}
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

				WheelCenterValueString = $"!{_wheelCenterValue}";

			}
		}

		private string _wheelCenterValueString = "0";

		[XmlIgnore]
		public string WheelCenterValueString
		{
			get => _wheelCenterValueString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _wheelCenterValueString != value )
					{
						_wheelCenterValueString = value;

						OnPropertyChanged();
					}
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

				WheelMaxValueString = $"!{_wheelMaxValue}";
			}
		}

		private string _wheelMaxValueString = "0";

		[XmlIgnore]
		public string WheelMaxValueString
		{
			get => _wheelMaxValueString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _wheelMaxValueString != value )
					{
						_wheelMaxValueString = value;

						OnPropertyChanged();
					}
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

		[XmlIgnore]
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

		#endregion

		#region Settings tab - Crash protection tab

		private bool _enableCrashProtection = true;

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

		private float _gForce = 6f;

		public float GForce
		{
			get => _gForce;

			set
			{
				value = Math.Clamp( value, 1f, 10f );

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

		private string _gForceString = "6.0 G";

		[XmlIgnore]
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

		private float _crashDuration = 1f;

		public float CrashDuration
		{
			get => _crashDuration;

			set
			{
				value = Math.Clamp( value, 0.1f, 10f );

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

		private string _crashDurationString = "1.0 sec";

		[XmlIgnore]
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

		/* Crash protection overall scale */

		private float _crashProtectionOverallScale = 100f;

		public float CrashProtectionOverallScale
		{
			get => _crashProtectionOverallScale;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _crashProtectionOverallScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"CrashProtectionOverallScale changed - before {_crashProtectionOverallScale} now {value}" );

					_crashProtectionOverallScale = value;

					OnPropertyChanged();
				}

				CrashProtectionOverallScaleString = $"!{_crashProtectionOverallScale:F0}%";
			}
		}

		private string _crashProtectionOverallScaleString = "100%";

		[XmlIgnore]
		public string CrashProtectionOverallScaleString
		{
			get => _crashProtectionOverallScaleString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _crashProtectionOverallScaleString != value )
					{
						_crashProtectionOverallScaleString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		#endregion

		#region Settings tab - Curb protection tab

		private bool _enableCurbProtection = false;

		public bool EnableCurbProtection
		{
			get => _enableCurbProtection;

			set
			{
				if ( _enableCurbProtection != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"EnableCurbProtection changed - before {_enableCurbProtection} now {value}" );

					_enableCurbProtection = value;

					OnPropertyChanged();
				}
			}
		}

		private float _shockVelocity = 0.5f;

		public float ShockVelocity
		{
			get => _shockVelocity;

			set
			{
				value = Math.Clamp( value, 0.01f, 2f );

				if ( _shockVelocity != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"ShockVelocity changed - before {_shockVelocity} now {value}" );

					_shockVelocity = value;

					OnPropertyChanged();
				}

				ShockVelocityString = $"!{(float) _shockVelocity:F2} m/s";
			}
		}

		private string _shockVelocityString = "0.50 m/s";

		[XmlIgnore]
		public string ShockVelocityString
		{
			get => _shockVelocityString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _shockVelocityString != value )
					{
						_shockVelocityString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		private float _curbProtectionDuration = 0.5f;

		public float CurbProtectionDuration
		{
			get => _curbProtectionDuration;

			set
			{
				value = Math.Clamp( value, 0.1f, 1f );

				if ( _curbProtectionDuration != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"CurbProtectionDuration changed - before {_curbProtectionDuration} now {value}" );

					_curbProtectionDuration = value;

					OnPropertyChanged();
				}

				CurbProtectionDurationString = $"!{(float) _curbProtectionDuration:F1} sec";
			}
		}

		private string _curbProtectionDurationString = "0.5 sec";

		[XmlIgnore]
		public string CurbProtectionDurationString
		{
			get => _curbProtectionDurationString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _curbProtectionDurationString != value )
					{
						_curbProtectionDurationString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Curb protection detail scale */

		private float _curbProtectionDetailScale = 80f;

		public float CurbProtectionDetailScale
		{
			get => _curbProtectionDetailScale;

			set
			{
				value = Math.Clamp( value, 0f, 100f );

				if ( _curbProtectionDetailScale != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"CurbProtectionDetailScale changed - before {_curbProtectionDetailScale} now {value}" );

					_curbProtectionDetailScale = value;

					OnPropertyChanged();
				}

				CurbProtectionDetailScaleString = $"!{_curbProtectionDetailScale:F0}%";
			}
		}

		private string _curbProtectionDetailScaleString = "80%";

		[XmlIgnore]
		public string CurbProtectionDetailScaleString
		{
			get => _curbProtectionDetailScaleString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _curbProtectionDetailScaleString != value )
					{
						_curbProtectionDetailScaleString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		#endregion

		#region Settings tab - Soft lock

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

		[XmlIgnore]
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

		[XmlIgnore]
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

		#endregion

		#region Settings tab - Force feedback tab

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

				FrequencyString = $"!{1000.0f / ( 18 - _frequency ):F2} Hz";
			}
		}

		private string _frequencyString = "500 Hz";

		[XmlIgnore]
		public string FrequencyString
		{
			get => _frequencyString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _frequencyString != value )
					{
						_frequencyString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		/* Pause when iRacing is not running */

		private bool _pauseWhenSimulatorIsNotRunning = true;

		public bool PauseWhenSimulatorIsNotRunning
		{
			get => _pauseWhenSimulatorIsNotRunning;

			set
			{
				if ( _pauseWhenSimulatorIsNotRunning != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"PauseWhenSimulatorIsNotRunning changed - before {_pauseWhenSimulatorIsNotRunning} now {value}" );

					_pauseWhenSimulatorIsNotRunning = value;

					OnPropertyChanged();
				}
			}
		}

		/* Send playback to device */

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

		/* Auto overall scale clip limit */

		private int _autoOverallScaleClipLimit = 25;

		public int AutoOverallScaleClipLimit
		{
			get => _autoOverallScaleClipLimit;

			set
			{
				value = Math.Clamp( value, 0, 100 );

				if ( _autoOverallScaleClipLimit != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"AutoOverallScaleClipLimit changed - before {_autoOverallScaleClipLimit} now {value}" );

					_autoOverallScaleClipLimit = value;

					OnPropertyChanged();
				}

				AutoOverallScaleClipLimitString = $"!{_autoOverallScaleClipLimit}%";
			}
		}

		private string _autoOverallScaleClipLimitString = "25%";

		[XmlIgnore]
		public string AutoOverallScaleClipLimitString
		{
			get => _autoOverallScaleClipLimitString;

			set
			{
				if ( ( value.Length > 0 ) && ( value[ 0 ] == '!' ) )
				{
					value = value[ 1.. ];

					if ( _autoOverallScaleClipLimitString != value )
					{
						_autoOverallScaleClipLimitString = value;

						OnPropertyChanged();
					}
				}
			}
		}

		#endregion

		#region Settings tab - Steering effects tab

		private bool _limitToCentripetalTrack = false;

		public bool LimitToCentripetalTrack
		{
			get => _limitToCentripetalTrack;

			set
			{
				if ( _limitToCentripetalTrack != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"LimitToCentripetalTrack changed - before {_limitToCentripetalTrack} now {value}" );

					_limitToCentripetalTrack = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Logitech tab


		/* Control RPM lights */

		private bool _controlRPMLights = true;

		public bool ControlRPMLights
		{
			get => _controlRPMLights;

			set
			{
				if ( _controlRPMLights != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"ControlRPMLights changed - before {_controlRPMLights} now {value}" );

					_controlRPMLights = value;

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

		/* Enable ABS sound */

		private bool _enableABSSound = false;

		public bool EnableABSSound
		{
			get => _enableABSSound;

			set
			{
				if ( _enableABSSound != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"EnableABSSound changed - before {_enableABSSound} now {value}" );

					_enableABSSound = value;

					OnPropertyChanged();
				}
			}
		}

		/* ABS sound volume */

		private int _absSoundVolume = 35;

		public int ABSSoundVolume
		{
			get => _absSoundVolume;

			set
			{
				if ( _absSoundVolume != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"ABSSoundVolume changed - before {_absSoundVolume} now {value}" );

					_absSoundVolume = value;

					OnPropertyChanged();
				}
			}
		}

		/* ABS sound pitch */

		private float _absSoundPitch = 1f;

		public float ABSSoundPitch
		{
			get => _absSoundPitch;

			set
			{
				if ( _absSoundPitch != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"ABSSoundPitch changed - before {_absSoundPitch} now {value}" );

					_absSoundPitch = value;

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

		#endregion

		#region Settings tab - Chat

		private bool _enableChat = false;

		public bool EnableChat
		{
			get => _enableChat;

			set
			{
				if ( _enableChat != value )
				{
					var app = (App) Application.Current;

					app.WriteLine( $"EnableChat changed - before {_enableChat} now {value}" );

					_enableChat = value;

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Settings tab - Messages

		private string _sayHello = "Hello";
		private string _sayConnected = "Connected";
		private string _sayDisconnected = "Disconnected";
		private string _sayVoiceVolume = ":value:";

		private string _sayCarName = ":value:";
		private string _sayTrackName = ":value:";
		private string _sayTrackConfigName = ":value:";
		private string _sayTrackWet = "The track is wet";
		private string _sayTrackDry = "";
		private string _sayFFBWarning = "Force feedback is enabled in iRacing, please turn it off.";

		private string _sayOverallScale = "Overall :value:";
		private string _sayDetailScale = "Detail :value:";
		private string _sayLFEScale = "LFE :value:";
		private string _sayScalesReset = "Scales were reset";
		private string _sayLoadOverallScale = "Overall :value:";
		private string _sayLoadDetailScale = "Detail :value:";
		private string _sayClipping = "";
		private string _sayCrashProtectionOn = "FFB crash protection engaged";
		private string _sayCrashProtectionOff = "FFB crash protection standing by";

		private string _sayClear = "Clear";
		private string _sayCarLeft = "Car left";
		private string _sayCarRight = "Car right";
		private string _sayTwoCarsLeft = "Two left";
		private string _sayTwoCarsRight = "Two right";
		private string _sayThreeWide = "Three wide";

		private string _sayCheckeredFlag = "Checkered flag";
		private string _sayWhiteFlag = "White flag - last lap";
		private string _sayGreenFlag = "Green flag";
		private string _sayYellowFlag = "Yellow flag";
		private string _sayRedFlag = "Red flag";
		private string _sayBlueFlag = "Blue flag";
		private string _sayDebrisFlag = "Watch for debris";
		private string _sayCrossedFlag = "Crossed flag";
		private string _sayYellowWavingFlag = "Yellow flag";
		private string _sayOneLapToGreenFlag = "One lap to green";
		private string _sayGreenHeldFlag = string.Empty;
		private string _sayTenToGoFlag = "Ten to go";
		private string _sayFiveToGoFlag = "Five to go";
		private string _sayRandomWavingFlag = string.Empty;
		private string _sayCautionFlag = string.Empty;
		private string _sayCautionWavingFlag = "Caution flag";
		private string _sayBlackFlag = "Black flag";
		private string _sayDisqualifyFlag = "You've been disqualified";
		private string _sayServicibleFlag = string.Empty;
		private string _sayFurledFlag = "Furled black flag";
		private string _sayRepairFlag = "Meatball flag";
		private string _sayStartHiddenFlag = string.Empty;
		private string _sayStartReadyFlag = "Get ready";
		private string _sayStartSetFlag = string.Empty;
		private string _sayStartGoFlag = "Go Go Go";

		public string SayHello { get => _sayHello; set { SetSayString( "SayHello", ref _sayHello, value ); } }
		public string SayConnected { get => _sayConnected; set { SetSayString( "SayConnected", ref _sayConnected, value ); } }
		public string SayDisconnected { get => _sayDisconnected; set { SetSayString( "SayDisconnected", ref _sayDisconnected, value ); } }
		public string SayVoiceVolume { get => _sayVoiceVolume; set { SetSayString( "SayVoiceVolume", ref _sayVoiceVolume, value ); } }

		public string SayCarName { get => _sayCarName; set { SetSayString( "SayCarName", ref _sayCarName, value ); } }
		public string SayTrackName { get => _sayTrackName; set { SetSayString( "SayTrackName", ref _sayTrackName, value ); } }
		public string SayTrackConfigName { get => _sayTrackConfigName; set { SetSayString( "SayTrackConfigName", ref _sayTrackConfigName, value ); } }
		public string SayTrackWet { get => _sayTrackWet; set { SetSayString( "SayTrackWet", ref _sayTrackWet, value ); } }
		public string SayTrackDry { get => _sayTrackDry; set { SetSayString( "SayTrackDry", ref _sayTrackDry, value ); } }
		public string SayFFBWarning { get => _sayFFBWarning; set { SetSayString( "SayFFBWarning", ref _sayFFBWarning, value ); } }

		public string SayOverallScale { get => _sayOverallScale; set { SetSayString( "SayOverallScale", ref _sayOverallScale, value ); } }
		public string SayDetailScale { get => _sayDetailScale; set { SetSayString( "SayDetailScale", ref _sayDetailScale, value ); } }
		public string SayLFEScale { get => _sayLFEScale; set { SetSayString( "SayLFEScale", ref _sayLFEScale, value ); } }
		public string SayScalesReset { get => _sayScalesReset; set { SetSayString( "SayScalesReset", ref _sayScalesReset, value ); } }
		public string SayLoadOverallScale { get => _sayLoadOverallScale; set { SetSayString( "SayLoadOverallScale", ref _sayLoadOverallScale, value ); } }
		public string SayLoadDetailScale { get => _sayLoadDetailScale; set { SetSayString( "SayLoadDetailScale", ref _sayLoadDetailScale, value ); } }
		public string SayClipping { get => _sayClipping; set { SetSayString( "SayClipping", ref _sayClipping, value ); } }
		public string SayCrashProtectionOn { get => _sayCrashProtectionOn; set { SetSayString( "SayCrashProtectionOn", ref _sayCrashProtectionOn, value ); } }
		public string SayCrashProtectionOff { get => _sayCrashProtectionOff; set { SetSayString( "SayCrashProtectionOff", ref _sayCrashProtectionOff, value ); } }

		public string SayClear { get => _sayClear; set { SetSayString( "SayClear", ref _sayClear, value ); } }
		public string SayCarLeft { get => _sayCarLeft; set { SetSayString( "SayCarLeft", ref _sayCarLeft, value ); } }
		public string SayCarRight { get => _sayCarRight; set { SetSayString( "SayCarRight", ref _sayCarRight, value ); } }
		public string SayTwoCarsLeft { get => _sayTwoCarsLeft; set { SetSayString( "SayTwoCarsLeft", ref _sayTwoCarsLeft, value ); } }
		public string SayTwoCarsRight { get => _sayTwoCarsRight; set { SetSayString( "SayTwoCarsRight", ref _sayTwoCarsRight, value ); } }
		public string SayThreeWide { get => _sayThreeWide; set { SetSayString( "SayThreeWide", ref _sayThreeWide, value ); } }

		public string SayCheckeredFlag { get => _sayCheckeredFlag; set { SetSayString( "SayCheckeredFlag", ref _sayCheckeredFlag, value ); } }
		public string SayWhiteFlag { get => _sayWhiteFlag; set { SetSayString( "SayWhiteFlag", ref _sayWhiteFlag, value ); } }
		public string SayGreenFlag { get => _sayGreenFlag; set { SetSayString( "SayGreenFlag", ref _sayGreenFlag, value ); } }
		public string SayYellowFlag { get => _sayYellowFlag; set { SetSayString( "SayYellowFlag", ref _sayYellowFlag, value ); } }
		public string SayRedFlag { get => _sayRedFlag; set { SetSayString( "SayRedFlag", ref _sayRedFlag, value ); } }
		public string SayBlueFlag { get => _sayBlueFlag; set { SetSayString( "SayBlueFlag", ref _sayBlueFlag, value ); } }
		public string SayDebrisFlag { get => _sayDebrisFlag; set { SetSayString( "SayDebrisFlag", ref _sayDebrisFlag, value ); } }
		public string SayCrossedFlag { get => _sayCrossedFlag; set { SetSayString( "SayCrossedFlag", ref _sayCrossedFlag, value ); } }
		public string SayYellowWavingFlag { get => _sayYellowWavingFlag; set { SetSayString( "SayYellowWavingFlag", ref _sayYellowWavingFlag, value ); } }
		public string SayOneLapToGreenFlag { get => _sayOneLapToGreenFlag; set { SetSayString( "SayOneLapToGreenFlag", ref _sayOneLapToGreenFlag, value ); } }
		public string SayGreenHeldFlag { get => _sayGreenHeldFlag; set { SetSayString( "SayGreenHeldFlag", ref _sayGreenHeldFlag, value ); } }
		public string SayTenToGoFlag { get => _sayTenToGoFlag; set { SetSayString( "SayTenToGoFlag", ref _sayTenToGoFlag, value ); } }
		public string SayFiveToGoFlag { get => _sayFiveToGoFlag; set { SetSayString( "SayFiveToGoFlag", ref _sayFiveToGoFlag, value ); } }
		public string SayRandomWavingFlag { get => _sayRandomWavingFlag; set { SetSayString( "SayRandomWavingFlag", ref _sayRandomWavingFlag, value ); } }
		public string SayCautionFlag { get => _sayCautionFlag; set { SetSayString( "SayCautionFlag", ref _sayCautionFlag, value ); } }
		public string SayCautionWavingFlag { get => _sayCautionWavingFlag; set { SetSayString( "SayCautionWavingFlag", ref _sayCautionWavingFlag, value ); } }
		public string SayBlackFlag { get => _sayBlackFlag; set { SetSayString( "SayBlackFlag", ref _sayBlackFlag, value ); } }
		public string SayDisqualifyFlag { get => _sayDisqualifyFlag; set { SetSayString( "SayDisqualifyFlag", ref _sayDisqualifyFlag, value ); } }
		public string SayServicibleFlag { get => _sayServicibleFlag; set { SetSayString( "SayServicibleFlag", ref _sayServicibleFlag, value ); } }
		public string SayFurledFlag { get => _sayFurledFlag; set { SetSayString( "SayFurledFlag", ref _sayFurledFlag, value ); } }
		public string SayRepairFlag { get => _sayRepairFlag; set { SetSayString( "SayRepairFlag", ref _sayRepairFlag, value ); } }
		public string SayStartHiddenFlag { get => _sayStartHiddenFlag; set { SetSayString( "SayStartHiddenFlag", ref _sayStartHiddenFlag, value ); } }
		public string SayStartReadyFlag { get => _sayStartReadyFlag; set { SetSayString( "SayStartReadyFlag", ref _sayStartReadyFlag, value ); } }
		public string SayStartSetFlag { get => _sayStartSetFlag; set { SetSayString( "SayStartSetFlag", ref _sayStartSetFlag, value ); } }
		public string SayStartGoFlag { get => _sayStartGoFlag; set { SetSayString( "SayStartGoFlag", ref _sayStartGoFlag, value ); } }

		private void SetSayString( string stringName, ref string sayString, string value, [CallerMemberName] string? name = null )
		{
			if ( sayString != value )
			{
				if ( value == null )
				{
					value = string.Empty;
				}

				var app = (App) Application.Current;

				app.WriteLine( $"{stringName} changed - before \"{sayString}\" now \"{value}\"" );

				sayString = value;

				OnPropertyChanged( true, name );
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

		private SerializableDictionary<string, string> _pedalHapticsDeviceList = [];

		public SerializableDictionary<string, string> PedalHapticsDeviceList { get => _pedalHapticsDeviceList; }

		public void UpdatePedalHapticsDeviceList( SerializableDictionary<string, string> pedalHapticsDeviceList )
		{
			_pedalHapticsDeviceList = pedalHapticsDeviceList;

			OnPropertyChanged( false, nameof( PedalHapticsDeviceList ) );
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

		public SerializableDictionary<int, string> PedalHapticsEffectList { get; } = new SerializableDictionary<int, string> { { 0, "None" }, { 1, "Gear Change" }, { 2, "ABS Engaged" }, { 3, "RPM (Wide)" }, { 4, "RPM (Narrow)" }, { 5, "Steering Effects" }, { 6, "Wheel Lock" }, { 7, "Wheel Spin" }, { 8, "Clutch Slip" } };

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

		#region Version functions

		public string Version
		{
			get
			{
				var mainWindow = MarvinsAIRA.MainWindow.Instance;

				if ( mainWindow != null )
				{
					return MainWindow.GetVersion();
				}
				else
				{
					return string.Empty;
				}
			}
		}

		#endregion
	}
}
