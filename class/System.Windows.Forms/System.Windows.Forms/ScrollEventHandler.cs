//
// System.Windows.Forms.ScrollEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (DENNISH@Raytek.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Represents the method that handles the Scroll 
	/// event of a ScrollBar, TrackBar, or DataGrid.
	/// </summary>
	[Serializable]
	public delegate void ScrollEventHandler(object sender, ScrollEventArgs e);
}