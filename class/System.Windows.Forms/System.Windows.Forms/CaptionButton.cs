//
// System.Windows.Forms.CaptionButton
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.CaptionButton
{

	/// <summary>
  /// Specifies the type of caption button to display.
	/// </summary>

	[Serializable]
	public enum CaptionButton
	{
		Close = 0,
		Minimize = 1,
		Minimize = 2,
		Restore = 3,
		Help = 4,
	}
}
