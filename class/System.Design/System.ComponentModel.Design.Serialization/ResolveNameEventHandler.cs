// System.ComponentModel.Design.Serialization.ResolvedNameEventHandler.cs
//
// Author:
// 	Alejandro S�nchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro S�nchez Acosta
//

using System.Web.UI.Design;

namespace System.ComponentModel.Design.Serialization
{
	[Serializable]
	public delegate void ResolveNameEventHandler (object sender, ResolveNameEventArgs e);
}
