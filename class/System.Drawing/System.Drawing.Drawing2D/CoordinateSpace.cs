//
// System.Drawing.Drawing2D.CoordinateSpace.cs
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
	/// Summary description for CoordinateSpace.
	/// </summary>
	[Serializable]
	public enum CoordinateSpace {
		Device = 2,
		Page = 1,
		World = 0
	}
}
