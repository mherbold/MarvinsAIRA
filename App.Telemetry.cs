
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Windows;

namespace MarvinsAIRA
{
	[StructLayout( LayoutKind.Sequential, Pack = 4 )]
	public struct TelemetryData
	{
		public int tickCount;

		public float wheelMax;
		public float overallScale;
		public bool overallScaleAutoReady;
		public float overallScaleAutoClipLimit;
		public float detailScale;
		public float parkedScale;
		public float frequency;

		public int understeerFxStyle;
		public float understeerFxStrength;
		public float understeerFxCurve;
		public float understeerYRFactorLStart;
		public float understeerYRFactorLEnd;
		public float understeerYRFactorRStart;
		public float understeerYRFactorREnd;

		public int oversteerFxStyle;
		public float oversteerFxStrength;
		public float oversteerFxCurve;
		public float oversteerYVelocityStart;
		public float oversteerYVelocityEnd;

		public float lfeScale;

		public float ffbInAmount;
		public float ffbOutAmount;
		public bool ffbClipping;
		public float yawRateFactor;
		public float gForce;
		public float understeerAmount;
		public float oversteerAmount;
		public bool crashProtectionEngaged;

		public bool forceFeedbackEnabled;
		public bool steeringEffectsEnabled;
		public bool lfeToFFBEnabled;
		public bool windSimulatorEnabled;
		public bool spotterEnabled;

		public float overallScaleAutoPeak;

		public bool curbProtectionEngaged;

		public float ffbSteadyState;
		public float shockVelocity;
		public float lfeInAmount;
		public float lfeOutAmount;

		public float ffbCurve;
		public float minForce;
	}

	public partial class App : Application
	{
		private const string TELEMETRY_MEMORYMAPPEDFILENAME = "Local\\MAIRATelemetry";

		private TelemetryData _telemetry_data = new();

		private MemoryMappedFile? _telemetry_memoryMappedFile = null;
		private MemoryMappedViewAccessor? _telemetry_memoryMappedFileViewAccessor = null;

		private void InitializeTelemetry()
		{
			WriteLine( "InitializeTelemetry called.", true );

			try
			{
				WriteLine( "...creating or opening the memory mapped file..." );

				var sizeOfTelemetryData = Marshal.SizeOf( typeof( TelemetryData ) );

				_telemetry_memoryMappedFile = MemoryMappedFile.CreateOrOpen( TELEMETRY_MEMORYMAPPEDFILENAME, sizeOfTelemetryData );

				WriteLine( "...creating the memory mapped file view accessor..." );

				_telemetry_memoryMappedFileViewAccessor = _telemetry_memoryMappedFile.CreateViewAccessor();

				WriteLine( "...telemetry is ready to go." );
			}
			catch
			{

			}
		}

		private void StopTelemetry()
		{
			WriteLine( "StopTelemetry called.", true );

			_telemetry_memoryMappedFileViewAccessor = null;
			_telemetry_memoryMappedFile = null;
		}

		public void UpdateTelemetry()
		{
			_telemetry_data.tickCount++;

			_telemetry_data.wheelMax = Settings.WheelMaxForce;
			_telemetry_data.overallScale = Settings.OverallScale / 100f;
			_telemetry_data.overallScaleAutoReady = FFB_AutoOverallScaleIsReady;
			_telemetry_data.overallScaleAutoPeak = _ffb_autoTorqueNM;
			_telemetry_data.overallScaleAutoClipLimit = Settings.AutoOverallScaleClipLimit / 100f;
			_telemetry_data.detailScale = Settings.DetailScale / 100f;
			_telemetry_data.parkedScale = Settings.ParkedScale / 100f;
			_telemetry_data.frequency = 1000f / ( 18 - Settings.Frequency );

			_telemetry_data.understeerFxStyle = Settings.USEffectStyle;
			_telemetry_data.understeerFxStrength = Settings.USEffectStrength / 100f;
			_telemetry_data.understeerFxCurve = Settings.USCurve;
			_telemetry_data.understeerYRFactorLStart = Settings.USStartYawRateFactorLeft;
			_telemetry_data.understeerYRFactorLEnd = Settings.USEndYawRateFactorLeft;
			_telemetry_data.understeerYRFactorRStart = Settings.USStartYawRateFactorRight;
			_telemetry_data.understeerYRFactorREnd = Settings.USEndYawRateFactorRight;
			_telemetry_data.understeerAmount = _ffb_understeerAmount;

			_telemetry_data.oversteerFxStyle = Settings.OSEffectStyle;
			_telemetry_data.oversteerFxStrength = Settings.OSEffectStrength / 100f;
			_telemetry_data.oversteerFxCurve = Settings.OSCurve;
			_telemetry_data.oversteerYVelocityStart = Settings.OSStartYVelocity;
			_telemetry_data.oversteerYVelocityEnd = Settings.OSEndYVelocity;
			_telemetry_data.oversteerAmount = _ffb_oversteerAmount;

			_telemetry_data.lfeScale = Settings.LFEScale / 100f;
			_telemetry_data.lfeInAmount = _ffb_lfeInMagnitude;
			_telemetry_data.lfeOutAmount = _ffb_lfeOutTorqueNM;

			_telemetry_data.ffbInAmount = _ffb_inTorqueNM;
			_telemetry_data.ffbOutAmount = _ffb_outTorqueNM;
			_telemetry_data.ffbSteadyState = _ffb_rawSteadyStateTorqueNM;
			_telemetry_data.ffbClipping = _ffb_clippedTimer > 0f;

			_telemetry_data.yawRateFactor = _ffb_yawRateFactorInstant;
			_telemetry_data.gForce = _irsdk_gForce;
			_telemetry_data.shockVelocity = _ffb_maxShockVel;

			_telemetry_data.crashProtectionEngaged = _ffb_crashProtectionTimer > 0f;
			_telemetry_data.curbProtectionEngaged = _ffb_curbProtectionTimer > 0f;

			_telemetry_data.forceFeedbackEnabled = Settings.ForceFeedbackEnabled;
			_telemetry_data.steeringEffectsEnabled = Settings.SteeringEffectsEnabled;
			_telemetry_data.lfeToFFBEnabled = Settings.LFEToFFBEnabled;
			_telemetry_data.windSimulatorEnabled = Settings.WindSimulatorEnabled;
			_telemetry_data.spotterEnabled = Settings.SpotterEnabled;


			_telemetry_data.ffbCurve = Settings.FFBCurve;
			_telemetry_data.minForce = Settings.MinForce;

        _telemetry_memoryMappedFileViewAccessor?.Write( 0, ref _telemetry_data );
		}
	}
}
