//
// System.Windows.Forms.FormStartPosition
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
  /// Specifies the initial position of a form.
	/// </summary>
	[Serializable]
//	[ComVisible(true)]
	public enum FormStartPosition
	{
		Manual = 0,
		CenterScreen = 1,
		WindowsDefaultLocation = 2,
		WindowsDefaultBounds = 3,
		CenterParent = 4,		
	}
}
