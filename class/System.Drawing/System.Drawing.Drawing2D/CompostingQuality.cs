//
// System.Drawing.Drawing2D.CompostingQuality.cs
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
	/// Summary description for CompostingQuality.
	/// </summary>
	public enum CompositingQuality {
		AssumeLinear,
		Default,
		GammaCorrected,
		HighQuality,
		HighSpeed,
		Invalid
	}
}
