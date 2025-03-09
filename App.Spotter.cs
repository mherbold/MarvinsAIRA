
using System.Windows;

using IRSDKSharper;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private readonly IRacingSdkEnum.Flags[] _spotter_flagList = (IRacingSdkEnum.Flags[]) Enum.GetValues( typeof( IRacingSdkEnum.Flags ) );

		private IRacingSdkEnum.CarLeftRight _spotter_lastCarLeftRight = IRacingSdkEnum.CarLeftRight.Clear;
		private IRacingSdkEnum.Flags _spotter_lastSessionFlags = 0;
		private float _spotter_calloutTimer = 0f;

		public void UpdateSpotter( float deltaTime )
		{
			if ( !Settings.SpotterEnabled || !_irsdk_isOnTrack )
			{
				_spotter_lastCarLeftRight = IRacingSdkEnum.CarLeftRight.Clear;
				_spotter_calloutTimer = 0f;

				return;
			}

			if ( ProcessCarLeftRight( deltaTime ) )
			{
				ProcessSessionFlags();
			}
		}

		public bool ProcessCarLeftRight( float deltaTime )
		{
			_spotter_calloutTimer = Math.Max( 0f, _spotter_calloutTimer - deltaTime );

			var carLeftRight = _irsdk_carLeftRight;

			if ( carLeftRight == IRacingSdkEnum.CarLeftRight.Off )
			{
				carLeftRight = IRacingSdkEnum.CarLeftRight.Clear;
			}

			bool sayCarLeftRightNow = false;

			if ( carLeftRight != _spotter_lastCarLeftRight )
			{
				sayCarLeftRightNow = true;

				if ( ( _spotter_lastCarLeftRight == IRacingSdkEnum.CarLeftRight.CarLeft ) && ( carLeftRight == IRacingSdkEnum.CarLeftRight.TwoCarsLeft ) )
				{
					sayCarLeftRightNow = false;
				}
				else if ( ( _spotter_lastCarLeftRight == IRacingSdkEnum.CarLeftRight.CarRight ) && ( carLeftRight == IRacingSdkEnum.CarLeftRight.TwoCarsRight ) )
				{
					sayCarLeftRightNow = false;
				}
				else if ( ( _spotter_lastCarLeftRight == IRacingSdkEnum.CarLeftRight.TwoCarsLeft ) && ( carLeftRight == IRacingSdkEnum.CarLeftRight.CarLeft ) )
				{
					sayCarLeftRightNow = false;
				}
				else if ( ( _spotter_lastCarLeftRight == IRacingSdkEnum.CarLeftRight.TwoCarsRight ) && ( carLeftRight == IRacingSdkEnum.CarLeftRight.CarRight ) )
				{
					sayCarLeftRightNow = false;
				}

				_spotter_lastCarLeftRight = carLeftRight;
			}
			else if ( _spotter_calloutTimer == 0f )
			{
				if ( carLeftRight != IRacingSdkEnum.CarLeftRight.Clear )
				{
					sayCarLeftRightNow = true;
				}
			}

			if ( sayCarLeftRightNow )
			{
				switch ( carLeftRight )
				{
					case IRacingSdkEnum.CarLeftRight.Clear: Say( Settings.SayClear, null, true, false ); break;
					case IRacingSdkEnum.CarLeftRight.CarLeft: Say( Settings.SayCarLeft, null, true, false ); break;
					case IRacingSdkEnum.CarLeftRight.CarRight: Say( Settings.SayCarRight, null, true, false ); break;
					case IRacingSdkEnum.CarLeftRight.TwoCarsLeft: Say( Settings.SayTwoCarsLeft, null, true, false ); break;
					case IRacingSdkEnum.CarLeftRight.TwoCarsRight: Say( Settings.SayTwoCarsRight, null, true, false ); break;
					case IRacingSdkEnum.CarLeftRight.CarLeftRight: Say( Settings.SayThreeWide, null, true, false ); break;
				}

				_spotter_calloutTimer = 60.0f / Settings.SpotterCalloutFrequency;
			}

			return carLeftRight == IRacingSdkEnum.CarLeftRight.Clear;
		}

		public void ProcessSessionFlags()
		{
			foreach ( var flag in _spotter_flagList )
			{
				if ( ( ( _spotter_lastSessionFlags & flag ) == 0 ) && ( ( _irsdk_sessionFlags & flag ) != 0 ) )
				{
					switch ( flag )
					{
						case IRacingSdkEnum.Flags.Checkered: Say( Settings.SayCheckeredFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.White: Say( Settings.SayWhiteFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Green: Say( Settings.SayGreenFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Yellow: Say( Settings.SayYellowFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Red: Say( Settings.SayRedFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Blue: Say( Settings.SayBlueFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Debris: Say( Settings.SayDebrisFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Crossed: Say( Settings.SayCrossedFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.YellowWaving: Say( Settings.SayYellowWavingFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.OneLapToGreen: Say( Settings.SayOneLapToGreenFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.GreenHeld: Say( Settings.SayGreenHeldFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.TenToGo: Say( Settings.SayTenToGoFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.FiveToGo: Say( Settings.SayFiveToGoFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.RandomWaving: Say( Settings.SayRandomWavingFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Caution: Say( Settings.SayCautionFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.CautionWaving: Say( Settings.SayCautionWavingFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Black: Say( Settings.SayBlackFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Disqualify: Say( Settings.SayDisqualifyFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Servicible: Say( Settings.SayServicibleFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Furled: Say( Settings.SayFurledFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.Repair: Say( Settings.SayRepairFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.StartHidden: Say( Settings.SayStartHiddenFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.StartReady: Say( Settings.SayStartReadyFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.StartSet: Say( Settings.SayStartSetFlag, null, false, false ); break;
						case IRacingSdkEnum.Flags.StartGo: Say( Settings.SayStartGoFlag, null, false, false ); break;
					}
				}
			}

			_spotter_lastSessionFlags = _irsdk_sessionFlags;
		}
	}
}
