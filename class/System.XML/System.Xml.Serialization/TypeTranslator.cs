//
// System.Xml.Serialization.TypeTranslator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Erik LeBel (eriklebel@yahoo.ca)
//  Lluis Sanchez Gual (lluis@ximian.com)
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
		static Hashtable nameCache;
		static Hashtable primitiveTypes;

		static TypeTranslator ()
		{
			nameCache = new Hashtable ();
			nameCache.Add (typeof (bool), new TypeData (typeof (bool), "boolean", true));
			nameCache.Add (typeof (short), new TypeData (typeof (short), "short", true));
			nameCache.Add (typeof (ushort), new TypeData (typeof (ushort), "unsignedShort", true));
			nameCache.Add (typeof (int), new TypeData (typeof (int), "int", true));
			nameCache.Add (typeof (uint), new TypeData (typeof (uint), "unsignedInt", true));
			nameCache.Add (typeof (long), new TypeData (typeof (long), "long", true));
			nameCache.Add (typeof (ulong), new TypeData (typeof (ulong), "unsignedLong", true));
			nameCache.Add (typeof (float), new TypeData (typeof (float), "float", true));
			nameCache.Add (typeof (double), new TypeData (typeof (double), "double", true));
			nameCache.Add (typeof (DateTime), new TypeData (typeof (DateTime), "dateTime", true));	// TODO: timeInstant, Xml date, xml time
			nameCache.Add (typeof (Guid), new TypeData (typeof (Guid), "guid", true));
			nameCache.Add (typeof (decimal), new TypeData (typeof (decimal), "decimal", true));
			nameCache.Add (typeof (XmlQualifiedName), new TypeData (typeof (XmlQualifiedName), "QName", true));
			nameCache.Add (typeof (string), new TypeData (typeof (string), "string", true));
			nameCache.Add (typeof (byte), new TypeData (typeof (byte), "unsignedByte", true));
			nameCache.Add (typeof (sbyte), new TypeData (typeof (sbyte), "byte", true));
			nameCache.Add (typeof (char), new TypeData (typeof (char), "char", true));
			nameCache.Add (typeof (object), new TypeData (typeof (object), "anyType", false));
			nameCache.Add (typeof (byte[]), new TypeData (typeof (byte[]), "base64Binary", true));
			nameCache.Add (typeof (XmlNode), new TypeData (typeof (XmlNode), "XmlNode", false));
			nameCache.Add (typeof (XmlElement), new TypeData (typeof (XmlElement), "XmlElement", false));

			primitiveTypes = new Hashtable();
			ICollection types = nameCache.Values;
			foreach (TypeData td in types)
				primitiveTypes.Add (td.XmlType, td);

			primitiveTypes.Add ("date", new TypeData (typeof (DateTime), "date", true));	// TODO: timeInstant
			primitiveTypes.Add ("time", new TypeData (typeof (DateTime), "time", true));
			primitiveTypes.Add ("NMTOKEN", new TypeData (typeof (string), "NMTOKEN", true));
			primitiveTypes.Add ("NCName", new TypeData (typeof (string), "NCName", true));
			primitiveTypes.Add ("language", new TypeData (typeof (string), "language", true));
		}

		public static TypeData GetTypeData (Type type)
		{
			return GetTypeData (type, null);
		}

		public static TypeData GetTypeData (Type type, string xmlDataType)
		{
			if (xmlDataType != null) return GetPrimitiveTypeData (xmlDataType);

			TypeData typeData = nameCache[type] as TypeData;
			if (typeData != null) return typeData;
			
			string name;
			if (type.IsArray) {
				string sufix = GetTypeData (type.GetElementType ()).XmlType;
				name = GetArrayName (sufix);
			}
			else 
				name = type.Name;

			typeData = new TypeData (type, name, false);
			nameCache[type] = typeData;
			return typeData;
		}

		public static bool IsPrimitive (Type type)
		{
			return GetTypeData (type).SchemaType == SchemaTypes.Primitive;
		}

		public static TypeData GetPrimitiveTypeData (string typeName)
		{
			TypeData td = (TypeData) primitiveTypes[typeName];
			if (td == null) throw new NotSupportedException ("Data type '" + typeName + "' not supported");
			return td;
		}

		public static TypeData CreateCustomType (string typeName, string fullTypeName, string xmlType, SchemaTypes schemaType, TypeData listItemTypeData)
		{
			TypeData td = new TypeData (typeName, fullTypeName, xmlType, schemaType, listItemTypeData);
			return td;
		}

		public static string GetArrayName (string elemName)
		{
			return "ArrayOf" + Char.ToUpper (elemName [0]) + elemName.Substring (1);
		}
	}
}

