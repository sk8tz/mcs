// System.ComponentModel.Design.Serialization.IDesignerSerializationService.cs
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
	public interface IDesignerSerializationService
	{
		ICollection Deserialize (object serializationData);

		object Serialize (ICollection objects);
	}
}
