// System.ComponentModel.Design.Serialization.ICodeDomDesignerReload.cs
//
// Author:
// 	Alejandro S�nchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro S�nchez Acosta
//

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	public interface ICodeDomDesignerReload
	{
		bool ShouldReloadDesigner (CodeCompileUnit newTree);
	}
}
