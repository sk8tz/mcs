//
// System.Windows.Forms.BoundsSpecified.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	[Flags]
	public enum BoundsSpecified
	{
		All = 15,
		Height = 8,
		Location = 3,
		None = 0,
		Size = 12,
		Width = 4,
		X = 1,
		Y = 2
	}
}
