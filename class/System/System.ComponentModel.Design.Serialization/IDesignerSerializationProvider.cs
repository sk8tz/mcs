// System.ComponentModel.Design.Serialization.IDesignerSerializationProvider.cs
//
// Author:
// 	Alejandro S�nchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro S�nchez Acosta
//

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerSerializationProvider
	{
		object GetSerializer (IDesignerSerializationManager manager, 
				      object currentSerializer, Type objectType, 
				      Type serializerType);
	}
}
