//
// System.Windows.Forms.ErrorBlinkStyle
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms
{

	/// <summary>
  /// Specifies constants indicating when the error icon, supplied by an ErrorProvider, 
  /// should blink to alert the user that an error has occurred.
	/// </summary>
	[Serializable]
	public enum ErrorBlinkStyle
	{
		BlinkIfDifferentError = 0,
		AlwaysBlink = 1,
		NeverBlink = 2,
	}
}
