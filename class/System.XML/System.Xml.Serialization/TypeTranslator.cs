//
// System.Xml.Serialization.TypeTranslator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Erik LeBel (eriklebel@yahoo.ca)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Erik Lebel
//

using System;
using System.Collections;

namespace System.Xml.Serialization
{
	internal class TypeTranslator
	{
		static Hashtable primitives;

		static TypeTranslator ()
		{
			primitives = new Hashtable ();
			primitives.Add (typeof (bool), "boolean");
			primitives.Add (typeof (short), "short");
			primitives.Add (typeof (ushort), "unsignedShort");
			primitives.Add (typeof (int), "int");
			primitives.Add (typeof (uint), "unsignedInt");
			primitives.Add (typeof (long), "long");
			primitives.Add (typeof (ulong), "unsignedLong");
			primitives.Add (typeof (float), "float");
			primitives.Add (typeof (double), "double");
			primitives.Add (typeof (DateTime), "dateTime"); // TODO: timeInstant, Xml date, xml time
			primitives.Add (typeof (Guid), "guid");
			primitives.Add (typeof (Decimal), "decimal");
			primitives.Add (typeof (XmlQualifiedName), "QName");
			primitives.Add (typeof (string), "string");
			primitives.Add (typeof (byte), "unsignedByte");
			primitives.Add (typeof (sbyte), "byte");
			primitives.Add (typeof (char), "char");
			primitives.Add (typeof (object), "anyType");
			primitives.Add (typeof (byte[]), "base64Binary");
		}

		public TypeTranslator ()
		{
		}

		public TypeData GetTypeData (Type type)
		{
			string name = primitives [type] as string;
			if (name == null && type.IsArray) {
				name = primitives [type.GetElementType ()] as string;
				if (name != null)
					name = "ArrayOf" + Char.ToUpper (name [0]) + name.Substring (1);
			}
			
			return new TypeData (type, (name == null) ? type.Name : name, name != null);
		}
	}
}

