// System.ComponentModel.Design.ActiveDesignerEventHandler.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
// 

using System.Runtime.Serialization;

namespace System.ComponentModel.Design
{
	[Serializable]
	public delegate void ActiveDesignerEventHandler (object sender, ActiveDesignerEventArgs e);
}
