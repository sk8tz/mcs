// System.ComponentModel.Design.DesignerTransactionCloseEventHandler.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[Serializable]
	[ComVisible(true)]
	public delegate void DesignerTransactionCloseEventHandler (object sender, DesignerTransactionCloseEventArgs e);
}
