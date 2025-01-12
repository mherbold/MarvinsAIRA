
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;

namespace MarvinsAIRA
{
	public class Settings : INotifyPropertyChanged
	{
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

		/* Force feedback settings - device list */

		private SerializableDictionary<Guid, string> _deviceList = [];

		public SerializableDictionary<Guid, string> DeviceList { get => _deviceList; }

		/* Force feedback settings - selected device */

		private Guid _selectedDeviceId = Guid.Empty;

		public Guid SelectedDeviceId
		{
			get => _selectedDeviceId;

			set
			{
				if ( _selectedDeviceId != value )
				{
					_selectedDeviceId = value;

					OnPropertyChanged();
				}
			}
		}

		/* Force feedback settings - wheel max force */

		private float _wheelMaxForce = 10.9f;

		public float WheelMaxForce
		{
			get => _wheelMaxForce;

			set
			{
				if ( _wheelMaxForce != value )
				{
					_wheelMaxForce = value;

					WheelMaxForceString = $"{_wheelMaxForce:F1} N⋅m";

					OnPropertyChanged();
				}
			}
		}

		private string _wheelMaxForceString = "10.9 N⋅m";

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

		/* Force feedback settings - overall scale */

		private int _overallScale = 10;

		public int OverallScale
		{
			get => _overallScale;

			set
			{
				value = Math.Max( 0, Math.Min( 250, value ) );

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

		/* Force feedback settings - detail scale */

		private int _detailScale = 100;

		public int DetailScale
		{
			get => _detailScale;

			set
			{
				value = Math.Max( 0, Math.Min( 250, value ) );

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

		/* Wind settings - car speed */

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

		/* Wind settings - wind force */

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

		/* Wind settings - wind force string */

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

		/* Per-car/track/track config force feedback settings */

		public class ForceFeedbackSettings
		{
			public string CarScreenName = "";
			public string TrackDisplayName = "";
			public string TrackConfigName = "";

			public int OverallScale = 10;
			public int DetailScale = 100;
		}

		public List<ForceFeedbackSettings> ForceFeedbackSettingsList { get; private set; } = [];

		/* Button mappings */

		private Guid _decreaseOverallScaleDeviceInstanceGuid = Guid.Empty;

		public Guid DecreaseOverallScaleDeviceInstanceGuid
		{
			get => _decreaseOverallScaleDeviceInstanceGuid;

			set
			{
				if ( _decreaseOverallScaleDeviceInstanceGuid != value )
				{
					_decreaseOverallScaleDeviceInstanceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		private Guid _increaseOverallScaleDeviceInstanceGuid = Guid.Empty;

		public Guid IncreaseOverallScaleDeviceInstanceGuid
		{
			get => _increaseOverallScaleDeviceInstanceGuid;

			set
			{
				if ( _increaseOverallScaleDeviceInstanceGuid != value )
				{
					_increaseOverallScaleDeviceInstanceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		private Guid _decreaseDetailScaleDeviceInstanceGuid = Guid.Empty;

		public Guid DecreaseDetailScaleDeviceInstanceGuid
		{
			get => _decreaseDetailScaleDeviceInstanceGuid;

			set
			{
				if ( _decreaseDetailScaleDeviceInstanceGuid != value )
				{
					_decreaseDetailScaleDeviceInstanceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		private Guid _increaseDetailScaleDeviceInstanceGuid = Guid.Empty;

		public Guid IncreaseDetailScaleDeviceInstanceGuid
		{
			get => _increaseDetailScaleDeviceInstanceGuid;

			set
			{
				if ( _increaseDetailScaleDeviceInstanceGuid != value )
				{
					_increaseDetailScaleDeviceInstanceGuid = value;

					OnPropertyChanged();
				}
			}
		}

		private string _decreaseOverallScaleDeviceProductName = string.Empty;

		public string DecreaseOverallScaleDeviceProductName
		{
			get => _decreaseOverallScaleDeviceProductName;

			set
			{
				if ( _decreaseOverallScaleDeviceProductName != value )
				{
					_decreaseOverallScaleDeviceProductName = value;

					OnPropertyChanged();
				}
			}
		}

		private string _increaseOverallScaleDeviceProductName = string.Empty;

		public string IncreaseOverallScaleDeviceProductName
		{
			get => _increaseOverallScaleDeviceProductName;

			set
			{
				if ( _increaseOverallScaleDeviceProductName != value )
				{
					_increaseOverallScaleDeviceProductName = value;

					OnPropertyChanged();
				}
			}
		}

		private string _decreaseDetailScaleDeviceProductName = string.Empty;

		public string DecreaseDetailScaleDeviceProductName
		{
			get => _decreaseDetailScaleDeviceProductName;

			set
			{
				if ( _decreaseDetailScaleDeviceProductName != value )
				{
					_decreaseDetailScaleDeviceProductName = value;

					OnPropertyChanged();
				}
			}
		}

		private string _increaseDetailScaleDeviceProductName = string.Empty;

		public string IncreaseDetailScaleDeviceProductName
		{
			get => _increaseDetailScaleDeviceProductName;

			set
			{
				if ( _increaseDetailScaleDeviceProductName != value )
				{
					_increaseDetailScaleDeviceProductName = value;

					OnPropertyChanged();
				}
			}
		}

		private int _decreaseOverallScaleButtonNumber = 0;

		public int DecreaseOverallScaleButtonNumber
		{
			get => _decreaseOverallScaleButtonNumber;

			set
			{
				if ( _decreaseOverallScaleButtonNumber != value )
				{
					_decreaseOverallScaleButtonNumber = value;

					OnPropertyChanged();
				}
			}
		}

		private int _increaseOverallScaleButtonNumber = 0;

		public int IncreaseOverallScaleButtonNumber
		{
			get => _increaseOverallScaleButtonNumber;

			set
			{
				if ( _increaseOverallScaleButtonNumber != value )
				{
					_increaseOverallScaleButtonNumber = value;

					OnPropertyChanged();
				}
			}
		}

		private int _decreaseDetailScaleButtonNumber = 0;

		public int DecreaseDetailScaleButtonNumber
		{
			get => _decreaseDetailScaleButtonNumber;

			set
			{
				if ( _decreaseDetailScaleButtonNumber != value )
				{
					_decreaseDetailScaleButtonNumber = value;

					OnPropertyChanged();
				}
			}
		}

		private int _increaseDetailScaleButtonNumber = 0;

		public int IncreaseDetailScaleButtonNumber
		{
			get => _increaseDetailScaleButtonNumber;

			set
			{
				if ( _increaseDetailScaleButtonNumber != value )
				{
					_increaseDetailScaleButtonNumber = value;

					OnPropertyChanged();
				}
			}
		}

		/* Notification functions */

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged( [CallerMemberName] string? name = null )
		{
			var app = (App) Application.Current;

			app.QueueForSerialization();

			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public void UpdateDeviceList( SerializableDictionary<Guid, string> deviceList )
		{
			_deviceList = deviceList;

			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( DeviceList ) ) );
		}
	}
}
