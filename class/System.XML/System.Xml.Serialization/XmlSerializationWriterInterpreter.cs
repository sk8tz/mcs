//
// XmlSerializationWriterInterpreter.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	internal class XmlSerializationWriterInterpreter: XmlSerializationWriter
	{
		XmlMapping _typeMap;
		SerializationFormat _format;

		public XmlSerializationWriterInterpreter(XmlMapping typeMap)
		{
			_typeMap = typeMap;
			_format = typeMap.Format;
		}

		protected override void InitCallbacks ()
		{
			ArrayList maps = _typeMap.RelatedMaps;
			if (maps != null)
			{
				foreach (XmlTypeMapping map in maps)  {
					CallbackInfo info = new CallbackInfo (this, map);
					if (map.TypeData.SchemaType == SchemaTypes.Enum) AddWriteCallback(map.TypeData.Type, map.XmlType, map.Namespace, new XmlSerializationWriteCallback (info.WriteObject));
					else AddWriteCallback(map.TypeData.Type, map.XmlType, map.Namespace, new XmlSerializationWriteCallback (info.WriteEnum));
				}
			}
		}

		internal override void WriteObject (object ob)
		{
			WriteStartDocument ();

			if (_typeMap is XmlTypeMapping)
			{
				XmlTypeMapping mp = (XmlTypeMapping) _typeMap;
				if (mp.TypeData.SchemaType == SchemaTypes.Class || mp.TypeData.SchemaType == SchemaTypes.Array) 
					TopLevelElement ();

				if (_format == SerializationFormat.Literal)
					WriteObject (mp, ob, mp.ElementName, mp.Namespace, true, false, true);
				else
					WritePotentiallyReferencingElement (mp.ElementName, mp.Namespace, ob, mp.TypeData.Type, true, false);
			}
			else if (ob is object[])
				WriteMessage ((XmlMembersMapping)_typeMap, (object[]) ob);
			else
				throw CreateUnknownTypeException (ob);

			WriteReferencedElements ();
		}

		internal void WriteObject (XmlTypeMapping typeMap, object ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable) 
				{
					if (_format == SerializationFormat.Literal) WriteNullTagLiteral(element, namesp);
					else WriteNullTagEncoded (element, namesp);
				}
				return;
			}

			if (ob is XmlNode)
			{
				if (_format == SerializationFormat.Literal) WriteElementLiteral((XmlNode)ob, "", "", true, false);
				else WriteElementEncoded((XmlNode)ob, "", "", true, false);
				return;
			}

			XmlTypeMapping map = typeMap.GetRealTypeMap (ob.GetType().FullName);

			if (map == null) 
			{
				WriteTypedPrimitive (element, namesp, ob, true);
				return;
			}

			if (writeWrappingElem)
			{
				if (map != typeMap || _format == SerializationFormat.Encoded) needType = true;
				WriteStartElement (element, namesp, ob);
			}

			if (needType) 
				WriteXsiType(map.XmlType, map.Namespace);

			switch (map.TypeData.SchemaType)
			{
				case SchemaTypes.Class: WriteObjectElement (map, ob, element, namesp); break;
				case SchemaTypes.Array: WriteListElement (map, ob, element, namesp); break;
				case SchemaTypes.Primitive: WritePrimitiveElement (map, ob, element, namesp); break;
				case SchemaTypes.Enum: WriteEnumElement (map, ob, element, namesp); break;
			}

			if (writeWrappingElem)
				WriteEndElement (ob);
		}

		void WriteMessage (XmlMembersMapping membersMap, object[] parameters)
		{
			if (membersMap.HasWrapperElement) {
				TopLevelElement ();
				WriteStartElement(membersMap.ElementName, membersMap.Namespace, (_format == SerializationFormat.Encoded));

				if (Writer.LookupPrefix (XmlSchema.Namespace) == null)
					WriteAttribute ("xmlns","xsd",XmlSchema.Namespace,XmlSchema.Namespace);
	
				if (Writer.LookupPrefix (XmlSchema.InstanceNamespace) == null)
					WriteAttribute ("xmlns","xsi",XmlSchema.InstanceNamespace,XmlSchema.InstanceNamespace);
			}
			
			WriteMembers ((ClassMap)membersMap.ObjectMap, parameters, true);

			if (membersMap.HasWrapperElement)
				WriteEndElement();
		}

		void WriteObjectElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			ClassMap map = (ClassMap)typeMap.ObjectMap;
			WriteMembers (map, ob, false);
		}

		void WriteMembers (ClassMap map, object ob, bool isValueList)
		{
			// Write attributes

			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes) {
					if (MemberHasValue (attr, ob, isValueList))
						WriteAttribute (attr.AttributeName, attr.Namespace, GetStringValue (attr.MappedType, GetMemberValue (attr, ob, isValueList)));
				}
			}

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null && MemberHasValue (anyAttrMember, ob, isValueList))
			{
				ICollection extraAtts = (ICollection) GetMemberValue (anyAttrMember, ob, isValueList);
				if (extraAtts != null) 
				{
					foreach (XmlAttribute attr in extraAtts)
						WriteAttribute (attr.LocalName, attr.NamespaceURI, attr.Value);
				}
			}

			// Write elements

			ICollection members = map.ElementMembers;
			if (members != null)
			{
				foreach (XmlTypeMapMemberElement member in members)
				{
					if (!MemberHasValue (member, ob, isValueList)) continue;
					object memberValue = GetMemberValue (member, ob, isValueList);
					Type memType = member.GetType();

					if (memType == typeof(XmlTypeMapMemberList))
					{
						if (memberValue != null) 
							WriteMemberElement ((XmlTypeMapElementInfo) member.ElementInfo[0], memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberFlatList))
					{
						if (memberValue != null)
							WriteListContent (member.TypeData, ((XmlTypeMapMemberFlatList)member).ListMap, memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						if (memberValue != null)
							WriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						if (memberValue != null)
							WriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyAttribute))
					{
						// Ignore
					}
					else if (memType == typeof(XmlTypeMapMemberElement))
					{
						XmlTypeMapElementInfo elem = member.FindElement (ob, memberValue);
						WriteMemberElement (elem, memberValue);
					}
					else
						throw new InvalidOperationException ("Unknown member type");
				}
			}
		}

		object GetMemberValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) return ((object[])ob)[member.Index];
			else return member.GetValue (ob);
		}

		bool MemberHasValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) {
				return member.Index < ((object[])ob).Length;
			}
			else if (member.DefaultValue != null) {
				object val = GetMemberValue (member, ob, isValueList);
				if (val != null && val.Equals (member.DefaultValue)) return false;
			}
			return true;
		}

		void WriteMemberElement (XmlTypeMapElementInfo elem, object memberValue)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					string elemName = elem.WrappedElement ? elem.ElementName : "";
					if (_format == SerializationFormat.Literal) WriteElementLiteral(((XmlNode)memberValue), elemName, elem.Namespace, elem.IsNullable, false);
					else WriteElementEncoded(((XmlNode)memberValue), elemName, elem.Namespace, elem.IsNullable, false);
					break;

				case SchemaTypes.Enum:
				case SchemaTypes.Primitive:
					if (!elem.WrappedElement) {
						WriteValue (GetStringValue (elem.MappedType, memberValue));
					}
					else if (_format == SerializationFormat.Literal) {
						if (elem.IsNullable) WriteNullableStringLiteral (elem.ElementName, elem.Namespace, GetStringValue (elem.MappedType, memberValue));
						else WriteElementString (elem.ElementName, elem.Namespace, GetStringValue (elem.MappedType, memberValue));
					}
					else {
						if (elem.IsNullable) WriteNullableStringEncoded (elem.ElementName, elem.Namespace, GetStringValue (elem.MappedType, memberValue), new XmlQualifiedName (elem.DataType, elem.DataTypeNamespace));
						else WriteElementString (elem.ElementName, elem.Namespace, GetStringValue (elem.MappedType, memberValue), new XmlQualifiedName (elem.DataType, elem.DataTypeNamespace));
					}
					break;

				case SchemaTypes.Array:
					if (elem.MappedType.MultiReferenceType) 
						WriteReferencingElement (elem.ElementName, elem.Namespace, memberValue, elem.IsNullable);
					else {
						WriteStartElement(elem.ElementName, elem.Namespace, memberValue);
						WriteListContent (elem.TypeData, (ListMap) elem.MappedType.ObjectMap, memberValue);
						WriteEndElement (memberValue);
					}
					break;

				case SchemaTypes.Class:
					if (elem.MappedType.MultiReferenceType)	{
						if (elem.MappedType.TypeData.Type == typeof(object))
							WritePotentiallyReferencingElement (elem.ElementName, elem.Namespace, memberValue, null, false, elem.IsNullable);
						else
							WriteReferencingElement (elem.ElementName, elem.Namespace, memberValue, elem.IsNullable);
					}
					else WriteObject (elem.MappedType, memberValue, elem.ElementName, elem.Namespace, elem.IsNullable, false, true);
					break;

				case SchemaTypes.DataSet:
					throw new NotSupportedException ("Invalid type");

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}

		void WriteListElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			if (_format == SerializationFormat.Encoded)
			{
				string n, ns;
				int itemCount = GetListCount (typeMap.TypeData, ob);
				((ListMap) typeMap.ObjectMap).GetArrayType (itemCount, out n, out ns);
				string arrayType = (ns != string.Empty) ? FromXmlQualifiedName (new XmlQualifiedName(n,ns)) : n;
				WriteAttribute ("arrayType", SoapReflectionImporter.EncodingNamespace, arrayType);
			}
			WriteListContent (typeMap.TypeData, (ListMap) typeMap.ObjectMap, ob);
		}

		void WriteListContent (TypeData listType, ListMap map, object ob)
		{
			if (listType.Type.IsArray)
			{
				Array array = (Array)ob;
				for (int n=0; n<array.Length; n++)
				{
					object item = array.GetValue (n);
					XmlTypeMapElementInfo info = map.FindElement (item);
					if (info != null) WriteMemberElement (info, item);
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else if (ob is ICollection)
			{
				int count = (int) listType.Type.GetProperty ("Count").GetValue(ob,null);
				PropertyInfo itemProp = listType.Type.GetProperty ("Item");
				object[] index = new object[1];
				for (int n=0; n<count; n++)
				{
					index[0] = n;
					object item = itemProp.GetValue (ob, index);
					XmlTypeMapElementInfo info = map.FindElement (item);
					if (info != null) WriteMemberElement (info, item);
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else if (ob is IEnumerable)
			{
				IEnumerable e = (IEnumerable)ob;
				foreach (object item in e)
				{
					XmlTypeMapElementInfo info = map.FindElement (item);
					if (info != null) WriteMemberElement (info, item);
					else if (item != null) throw CreateUnknownTypeException (item);
				}
			}
			else
				throw new Exception ("Unsupported collection type");
		}

		int GetListCount (TypeData listType, object ob)
		{
			if (listType.Type.IsArray)
				return ((Array)ob).Length;
			else
				return (int) listType.Type.GetProperty ("Count").GetValue(ob,null);
		}

		void WriteAnyElementContent (XmlTypeMapMemberAnyElement member, object memberValue)
		{
			if (member.TypeData.Type == typeof (XmlElement)) {
				memberValue = new object[] { memberValue };
			}

			Array elems = (Array) memberValue;
			foreach (XmlNode elem in elems)
			{
				if (elem is XmlElement) 
				{
					if (member.IsElementDefined (elem.Name, elem.NamespaceURI))
					{
						if (_format == SerializationFormat.Literal) WriteElementLiteral (elem, "", "", false, true);
						else WriteElementEncoded (elem, "", "", false, true);
					}
					else
						throw CreateUnknownAnyElementException (elem.Name, elem.NamespaceURI);
				}
				else
					CreateUnknownTypeException (elem);
			}
		}

		void WritePrimitiveElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			Writer.WriteString (GetStringValue (typeMap, ob));
		}

		void WriteEnumElement (XmlTypeMapping typeMap, object ob, string element, string namesp)
		{
			Writer.WriteString (GetEnumXmlValue (typeMap, ob));
		}

		string GetStringValue (XmlTypeMapping typeMap, object value)
		{
			if (typeMap != null && typeMap.TypeData.SchemaType == SchemaTypes.Enum)
				return GetEnumXmlValue (typeMap, value);
			else
				return XmlCustomFormatter.ToXmlString (value);
		}

		string GetEnumXmlValue (XmlTypeMapping typeMap, object ob)
		{
			EnumMap map = (EnumMap)typeMap.ObjectMap;
			return map.GetXmlName (ob);
		}

		class CallbackInfo
		{
			XmlSerializationWriterInterpreter _swi;
			XmlTypeMapping _typeMap;

			public CallbackInfo (XmlSerializationWriterInterpreter swi, XmlTypeMapping typeMap)
			{
				_swi = swi;
				_typeMap = typeMap;
			}

			internal void WriteObject (object ob)
			{
				_swi.WriteObject (_typeMap, ob, _typeMap.ElementName, _typeMap.Namespace, false, true, false);
			}

			internal void WriteEnum (object ob)
			{
				_swi.WriteObject (_typeMap, ob, _typeMap.ElementName, _typeMap.Namespace, false, false, false);
			}
		}

	}
}
