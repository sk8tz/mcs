//
// System.Drawing.Drawing2D.SmoothingMode.cs
//
// Author:
//   Stefan Maierhofer <sm@cg.tuwien.ac.at>
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for SmoothingMode.
	/// </summary>
	[Serializable]
	public enum SmoothingMode {
		AntiAlias = 4,
		Default = 0,
		HighQuality = 2,
		HighSpeed = 1,
		Invalid = -1,
		None = 3
	}
}
