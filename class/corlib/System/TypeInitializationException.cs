//
// System.TypeInitializationException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
using System.Globalization;
using System.Runtime.Serialization;
namespace System {

	[Serializable]
	public sealed class TypeInitializationException : SystemException {
		string type_name;

		// Constructors
		public TypeInitializationException (string type_name, Exception inner)
			: base (Locale.GetText ("An exception was thrown by the type initializer for ") + type_name, inner)
		{
			this.type_name = type_name;
		}

		// Properties
		public string TypeName {
			get {
				return type_name;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("TypeName", type_name);
		}
	}

}
