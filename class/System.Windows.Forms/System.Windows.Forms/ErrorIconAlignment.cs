//
// System.Windows.Forms.ErrorIconAlignment
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
  /// Specifies constants indicating the locations that an error icon can appear
  /// in relation to the control with an error. 
	/// </summary>
	[Serializable]
	public enum ErrorIconAlignment
	{
		BottomLeft,
		BottomRight,
		MiddleLeft,
		MiddleRight,
		TopLeft,
		TopRight
	}
}