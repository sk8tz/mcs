//
// System.Windows.Forms.QueryAccessibilityHelpEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Represents the method that will handle the QueryAccessibilityHelp event of a control.
	/// </summary>
	[Serializable]
	public delegate void QueryAccessibilityHelpEventHandler(object sender, QueryAccessibilityHelpEventArgs e);
}