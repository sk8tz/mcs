//
// System.Runtime.InteropServices.Expando.IExpando.cs
//
// Author:
//    Alejandro S�nchez Acosta (raciel@es.gnu.org)
// 
// (C) Alejandro S�nchez Acosta
// 

using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.InteropServices.Expando
{
	[Guid("")]
	public interface IExpando : IReflect
	{
		FieldInfo AddField (string name);

		MethodInfo AddMethod (string name, Delegate method);

		PropertyInfo AddProperty(string name);

		void RemoveMember(MemberInfo m);
	}
}
