// System.ComponentModel.Design.Serialization.IDesignerLoaderHost.cs
//
// Author:
// 	Alejandro S�nchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro S�nchez Acosta
//

using System.Collections;
using System.Web.UI.Design;

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerLoaderHost : IDesignerHost, IServiceContainer, IServiceProvider
	{
		void EndLoad (string baseClassName, bool successful, ICollection errorCollection);

		void Reload();
	}
}
