// System.ComponentModel.Design.Serialization.IDesignerLoaderService.cs
//
// Author:
// 	Alejandro S�nchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro S�nchez Acosta
//

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerLoaderService
	{
		void AddLoadDependency();

		void DependentLoadComplete (bool successful, ICollection errorCollection);

		bool Reload();
	}
}
