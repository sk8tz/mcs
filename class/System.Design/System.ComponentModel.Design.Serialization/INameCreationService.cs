// System.ComponentModel.Design.Serialization.INameCreationService.cs
//
// Author:
// 	Alejandro S�nchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro S�nchez Acosta
//

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public interface INameCreationService
	{
		string CreateName (IContainer container, Type dataType);

		bool IsValidName (string name);

		void ValidateName (string name);
	}
}
