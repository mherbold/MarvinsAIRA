
using System.Windows;

using IRSDKSharper;

namespace MarvinsAIRA
{
	public partial class App : Application
	{
		private IRacingSdkEnum.CarLeftRight _spotter_lastCarLeftRight = IRacingSdkEnum.CarLeftRight.Clear;
		private float _spotter_calloutTimer = 0f;

		public void UpdateSpotter( float deltaTime )
		{
			if ( !Settings.SpotterEnabled || !_irsdk_connected )
			{
				_spotter_lastCarLeftRight = IRacingSdkEnum.CarLeftRight.Clear;
				_spotter_calloutTimer = 0f;

				return;
			}

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
					case IRacingSdkEnum.CarLeftRight.Clear: Say( Settings.SayClear, null, true ); break;
					case IRacingSdkEnum.CarLeftRight.CarLeft: Say( Settings.SayCarLeft, null, true ); break;
					case IRacingSdkEnum.CarLeftRight.CarRight: Say( Settings.SayCarRight, null, true ); break;
					case IRacingSdkEnum.CarLeftRight.TwoCarsLeft: Say( Settings.SayTwoCarsLeft, null, true ); break;
					case IRacingSdkEnum.CarLeftRight.TwoCarsRight: Say( Settings.SayTwoCarsRight, null, true ); break;
					case IRacingSdkEnum.CarLeftRight.CarLeftRight: Say( Settings.SayThreeWide, null, true ); break;
				}

				_spotter_calloutTimer = 60.0f / Settings.SpotterCalloutFrequency;
			}
		}
	}
}
