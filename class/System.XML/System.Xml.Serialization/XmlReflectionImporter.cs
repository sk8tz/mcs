// 
// System.Xml.Serialization.XmlReflectionImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Erik LeBel (eriklebel@yahoo.ca)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Erik LeBel
//

using System.Reflection;
using System.Collections;

namespace System.Xml.Serialization {
	public class XmlReflectionImporter {

		string initialDefaultNamespace;
		XmlAttributeOverrides attributeOverrides;
		ArrayList includedTypes;
		ReflectionHelper helper = new ReflectionHelper();
		int arrayChoiceCount = 1;
		ArrayList relatedMaps = new ArrayList ();

		static readonly string errSimple = "Cannot serialize object of type '{0}'. Base " +
			"type '{1}' has simpleContent and can be only extended by adding XmlAttribute " +
			"elements. Please consider changing XmlTextMember of the base class to string array";

		#region Constructors

		public XmlReflectionImporter ()
			: this (null, null)
		{
		}

		public XmlReflectionImporter (string defaultNamespace)
			: this (null, defaultNamespace)
		{
		}

		public XmlReflectionImporter (XmlAttributeOverrides attributeOverrides)
			: this (attributeOverrides, null)
		{
		}

		public XmlReflectionImporter (XmlAttributeOverrides attributeOverrides, string defaultNamespace)
		{
			if (defaultNamespace == null)
				this.initialDefaultNamespace = String.Empty;
			else
				this.initialDefaultNamespace = defaultNamespace;

			if (attributeOverrides == null)
				this.attributeOverrides = new XmlAttributeOverrides();
			else
				this.attributeOverrides = attributeOverrides;
		}

		void Reset ()
		{
			helper = new ReflectionHelper();
			arrayChoiceCount = 1;
		}

		#endregion // Constructors

		#region Methods

		public XmlMembersMapping ImportMembersMapping (string elementName,
			string ns,
			XmlReflectionMember [] members,
			bool hasWrapperElement)
		{
			Reset ();
			XmlMemberMapping[] mapping = new XmlMemberMapping[members.Length];
			for (int n=0; n<members.Length; n++)
			{
				XmlTypeMapMember mapMem = CreateMapMember (members[n], ns);
				mapping[n] = new XmlMemberMapping (members[n].MemberName, ns, mapMem, false);
			}
			XmlMembersMapping mps = new XmlMembersMapping (elementName, ns, hasWrapperElement, false, mapping);
			mps.RelatedMaps = relatedMaps;
			mps.Format = SerializationFormat.Literal;
			return mps;
		}

		public XmlTypeMapping ImportTypeMapping (Type type)
		{
			return ImportTypeMapping (type, null, null);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, string defaultNamespace)
		{
			return ImportTypeMapping (type, null, defaultNamespace);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute group)
		{
			return ImportTypeMapping (type, group, null);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (void))
				throw new InvalidOperationException ("Type " + type.Name + " may not be serialized.");

			if (defaultNamespace == null) defaultNamespace = initialDefaultNamespace;
			if (defaultNamespace == null) defaultNamespace = string.Empty;

			XmlTypeMapping map;

			switch (TypeTranslator.GetTypeData(type).SchemaType)
			{
				case SchemaTypes.Class: map = ImportClassMapping (type, root, defaultNamespace); break;
				case SchemaTypes.Array: map = ImportListMapping (type, root, defaultNamespace, null, 0); break;
				case SchemaTypes.XmlNode: map = ImportXmlNodeMapping (type, root, defaultNamespace); break;
				case SchemaTypes.Primitive: map = ImportPrimitiveMapping (type, root, defaultNamespace); break;
				case SchemaTypes.Enum: map = ImportEnumMapping (type, root, defaultNamespace); break;
				case SchemaTypes.XmlSerializable: map = ImportXmlSerializableMapping (type, root, defaultNamespace); break;
				default: throw new NotSupportedException ("Type " + type.FullName + " not supported for XML stialization");
			}

			map.RelatedMaps = relatedMaps;
			map.Format = SerializationFormat.Literal;
			return map;
		}

		XmlTypeMapping CreateTypeMapping (TypeData typeData, XmlRootAttribute root, string defaultXmlType, string defaultNamespace)
		{
			string membersNamespace;
			string elementName;
			bool includeInSchema = true;
			XmlAttributes atts = null;
			if (defaultXmlType == null) defaultXmlType = typeData.XmlType;

			if (!typeData.IsListType)
			{
				if (attributeOverrides != null) 
					atts = attributeOverrides[typeData.Type];

				if (atts != null && typeData.SchemaType == SchemaTypes.Primitive)
					throw new InvalidOperationException ("XmlRoot and XmlType attributes may not be specified for the type " + typeData.FullTypeName);
			}

			if (atts == null) 
				atts = new XmlAttributes (typeData.Type);

			if (atts.XmlRoot != null && root == null)
				root = atts.XmlRoot;

			if (atts.XmlType != null)
			{
				if (atts.XmlType.Namespace != null && atts.XmlType.Namespace != string.Empty)
					defaultNamespace = atts.XmlType.Namespace;

				if (atts.XmlType.TypeName != null && atts.XmlType.TypeName != string.Empty)
					defaultXmlType = atts.XmlType.TypeName;
					
				includeInSchema = atts.XmlType.IncludeInSchema;
			}

			membersNamespace = defaultNamespace;
			elementName = defaultXmlType;

			if (root != null)
			{
				if (root.ElementName != null && root.ElementName != String.Empty)
					elementName = root.ElementName;
				if (root.Namespace != null && root.Namespace != String.Empty)
					membersNamespace = root.Namespace;
			}

			if (membersNamespace == null) membersNamespace = "";
			XmlTypeMapping map = new XmlTypeMapping (elementName, membersNamespace, typeData, defaultXmlType, defaultNamespace);
			map.IncludeInSchema = includeInSchema;
			relatedMaps.Add (map);
			
			return map;
		}

		XmlTypeMapping ImportClassMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;

			ReflectionHelper.CheckSerializableType (type);
			
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);
			helper.RegisterSchemaType (map, map.XmlType, map.Namespace);

			// Import members

			ClassMap classMap = new ClassMap ();
			map.ObjectMap = classMap;

//			try
//			{
				ICollection members = GetReflectionMembers (type);
				foreach (XmlReflectionMember rmember in members)
				{
					if (rmember.XmlAttributes.XmlIgnore) continue;
					classMap.AddMember (CreateMapMember (rmember, map.Namespace));
				}
//			}
//			catch (Exception ex) {
//				throw helper.CreateError (map, ex.Message);
//			}

			ImportIncludedTypes (type, defaultNamespace);
			
			// Import extra classes

			if (type == typeof (object) && includedTypes != null)
			{
				foreach (Type intype in includedTypes)
					map.DerivedTypes.Add (ImportTypeMapping (intype, defaultNamespace));
			}

			// Register inheritance relations

			if (type.BaseType != null)
			{
				XmlTypeMapping bmap = ImportClassMapping (type.BaseType, root, defaultNamespace);
				
				if (type != typeof (object))
					map.BaseMap = bmap;
				
				// At this point, derived classes of this map must be already registered
				
				bmap.DerivedTypes.Add (map);
				bmap.DerivedTypes.AddRange (map.DerivedTypes);
				
				if (((ClassMap)bmap.ObjectMap).HasSimpleContent && classMap.ElementMembers != null && classMap.ElementMembers.Count != 1)
					throw new InvalidOperationException (String.Format (errSimple, map.TypeData.TypeName, map.BaseMap.TypeData.TypeName));
			}
			
			return map;
		}

		string GetTypeNamespace (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			string mapNamespace = defaultNamespace;

			XmlAttributes atts = null;
			if (!typeData.IsListType)
			{
				if (attributeOverrides != null)
					atts = attributeOverrides[typeData.Type];
			}

			if (atts == null)
				atts = new XmlAttributes (typeData.Type);

   			if (atts.XmlRoot != null && root == null)
   				root = atts.XmlRoot;

   			if (atts.XmlType != null)
   			{
   				if (atts.XmlType.Namespace != null && atts.XmlType.Namespace != string.Empty)
   					mapNamespace = atts.XmlType.Namespace;
			}
			
			if (root != null)
			{
				if (root.Namespace != null && root.Namespace != String.Empty)
					mapNamespace = root.Namespace;
			}

			if (mapNamespace == null) return "";
			else return mapNamespace;
		}

		XmlTypeMapping ImportListMapping (Type type, XmlRootAttribute root, string defaultNamespace, XmlAttributes atts, int nestingLevel)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			ListMap obmap = new ListMap ();

			ReflectionHelper.CheckSerializableType (type);
			
			if (atts == null) atts = new XmlAttributes();
			Type itemType = typeData.ListItemType;

			// warning: byte[][] should not be considered multiarray
			bool isMultiArray = (type.IsArray && (TypeTranslator.GetTypeData(itemType).SchemaType == SchemaTypes.Array) && itemType.IsArray);

			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			foreach (XmlArrayItemAttribute att in atts.XmlArrayItems)
			{
				if (att.NestingLevel != nestingLevel) continue;
				Type elemType = (att.Type != null) ? att.Type : itemType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData(elemType, att.DataType));
				elem.Namespace = att.Namespace != null ? att.Namespace : defaultNamespace;
				if (elem.Namespace == null) elem.Namespace = "";
				elem.Form = att.Form;
				elem.IsNullable = att.IsNullable && CanBeNull (elem.TypeData);
				elem.NestingLevel = att.NestingLevel;

				if (isMultiArray)
					elem.MappedType = ImportListMapping (elemType, null, elem.Namespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);

				if (att.ElementName != null) elem.ElementName = att.ElementName;
				else if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
				else elem.ElementName = TypeTranslator.GetTypeData(elemType).XmlType;

				list.Add (elem);
			}

			if (list.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData (itemType));
				if (isMultiArray)
					elem.MappedType = ImportListMapping (itemType, null, defaultNamespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (itemType, null, defaultNamespace);

				if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
				else elem.ElementName = TypeTranslator.GetTypeData(itemType).XmlType ;

				elem.Namespace = (defaultNamespace != null) ? defaultNamespace : "";
				elem.IsNullable = CanBeNull (elem.TypeData);
				list.Add (elem);
			}

			obmap.ItemInfo = list;

			// If there can be different element names (types) in the array, then its name cannot
			// be "ArrayOfXXX" it must be something like ArrayOfChoiceNNN

			string baseName;
			if (list.Count > 1)
				baseName = "ArrayOfChoice" + (arrayChoiceCount++);
			else
			{
				XmlTypeMapElementInfo elem = ((XmlTypeMapElementInfo)list[0]);
				if (elem.MappedType != null) baseName = TypeTranslator.GetArrayName (elem.MappedType.ElementName);
				else baseName = TypeTranslator.GetArrayName (elem.ElementName);
			}

			// Avoid name colisions

			int nameCount = 1;
			string name = baseName;

			do {
				XmlTypeMapping foundMap = helper.GetRegisteredSchemaType (name, defaultNamespace);
				if (foundMap == null) nameCount = -1;
				else if (obmap.Equals (foundMap.ObjectMap) && typeData.Type == foundMap.TypeData.Type) return foundMap;
				else name = baseName + (nameCount++);
			}
			while (nameCount != -1);

			XmlTypeMapping map = CreateTypeMapping (typeData, root, name, defaultNamespace);
			map.ObjectMap = obmap;

			// Register this map as a derived class of object

			helper.RegisterSchemaType (map, name, defaultNamespace);
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);

			ImportIncludedTypes (type, defaultNamespace);
			
			return map;
		}

		XmlTypeMapping ImportXmlNodeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (TypeTranslator.GetTypeData (type), root, defaultNamespace));
			if (map != null) return map;

			// Registers the maps for XmlNode and XmlElement

			XmlTypeMapping nodeMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlNode)), root, null, defaultNamespace);
			helper.RegisterClrType (nodeMap, typeof(XmlNode), nodeMap.Namespace);

			XmlTypeMapping elemMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlElement)), root, null, defaultNamespace);
			helper.RegisterClrType (elemMap, typeof(XmlElement), elemMap.Namespace);

			XmlTypeMapping textMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlText)), root, null, defaultNamespace);
			helper.RegisterClrType (elemMap, typeof(XmlText), textMap.Namespace);

			XmlTypeMapping obmap = ImportTypeMapping (typeof(object));
			obmap.DerivedTypes.Add (nodeMap);
			obmap.DerivedTypes.Add (elemMap);
			obmap.DerivedTypes.Add (textMap);
			nodeMap.DerivedTypes.Add (elemMap);
			nodeMap.DerivedTypes.Add (textMap);

			return helper.GetRegisteredClrType (type, GetTypeNamespace (TypeTranslator.GetTypeData (type), root, defaultNamespace));
		}

		XmlTypeMapping ImportPrimitiveMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);
			return map;
		}

		XmlTypeMapping ImportEnumMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);

			string [] names = Enum.GetNames (type);
			ArrayList members = new ArrayList();
			foreach (string name in names)
			{
				MemberInfo[] mem = type.GetMember (name);
				string xmlName = null;
				object[] atts = mem[0].GetCustomAttributes (typeof(XmlIgnoreAttribute), false);
				if (atts.Length > 0) continue;
				atts = mem[0].GetCustomAttributes (typeof(XmlEnumAttribute), false);
				if (atts.Length > 0) xmlName = ((XmlEnumAttribute)atts[0]).Name;
				if (xmlName == null) xmlName = name;
				members.Add (new EnumMap.EnumMapMember (xmlName, name));
			}

			bool isFlags = type.GetCustomAttributes (typeof(FlagsAttribute),false).Length > 0;
			map.ObjectMap = new EnumMap ((EnumMap.EnumMapMember[])members.ToArray (typeof(EnumMap.EnumMapMember)), isFlags);
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);
			return map;
		}

		XmlTypeMapping ImportXmlSerializableMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);
			return map;
		}

		void ImportIncludedTypes (Type type, string defaultNamespace)
		{
			XmlIncludeAttribute[] includes = (XmlIncludeAttribute[])type.GetCustomAttributes (typeof (XmlIncludeAttribute), false);
			for (int n=0; n<includes.Length; n++)
			{
				Type includedType = includes[n].Type;
				ImportTypeMapping (includedType, null, defaultNamespace);
			}
		}

		public ICollection GetReflectionMembers (Type type)
		{
			ArrayList members = new ArrayList();
			PropertyInfo[] properties = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo prop in properties)
			{
				if (!prop.CanRead) continue;
				if (!prop.CanWrite && TypeTranslator.GetTypeData (prop.PropertyType).SchemaType != SchemaTypes.Array)
					continue;
				if (prop.GetIndexParameters().Length > 0) continue;
					
				XmlAttributes atts = attributeOverrides[type, prop.Name];
				if (atts == null) atts = new XmlAttributes (prop);
				if (atts.XmlIgnore) continue;
				XmlReflectionMember member = new XmlReflectionMember(prop.Name, prop.PropertyType, atts);
				members.Add (member);
			}

			FieldInfo[] fields = type.GetFields (BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				XmlAttributes atts = attributeOverrides[type, field.Name];
				if (atts == null) atts = new XmlAttributes (field);
				if (atts.XmlIgnore) continue;
				XmlReflectionMember member = new XmlReflectionMember(field.Name, field.FieldType, atts);
				members.Add (member);
			}
			return members;
		}
		
		private XmlTypeMapMember CreateMapMember (XmlReflectionMember rmember, string defaultNamespace)
		{
			XmlTypeMapMember mapMember;
			XmlAttributes atts = rmember.XmlAttributes;
			TypeData typeData = TypeTranslator.GetTypeData (rmember.MemberType);

			if (atts.XmlAnyAttribute != null)
			{
				if ( (rmember.MemberType.FullName == "System.Xml.XmlAttribute[]") ||
					 (rmember.MemberType.FullName == "System.Xml.XmlNode[]") )
				{
					mapMember = new XmlTypeMapMemberAnyAttribute();
				}
				else
					throw new InvalidOperationException ("XmlAnyAttributeAttribute can only be applied to members of type XmlAttribute[] or XmlNode[]");
			}
			else if (atts.XmlAnyElements != null && atts.XmlAnyElements.Count > 0)
			{
				if ( (rmember.MemberType.FullName == "System.Xml.XmlElement[]") ||
					 (rmember.MemberType.FullName == "System.Xml.XmlNode[]") ||
					 (rmember.MemberType.FullName == "System.Xml.XmlElement"))
				{
					XmlTypeMapMemberAnyElement member = new XmlTypeMapMemberAnyElement();
					member.ElementInfo = ImportAnyElementInfo (defaultNamespace, rmember.MemberType, member, atts);
					mapMember = member;
				}
				else
					throw new InvalidOperationException ("XmlAnyElementAttribute can only be applied to members of type XmlElement, XmlElement[] or XmlNode[]");
			}
			else if (atts.Xmlns)
			{
				XmlTypeMapMemberNamespaces mapNamespaces = new XmlTypeMapMemberNamespaces ();
				mapMember = mapNamespaces;
			}
			else if (atts.XmlAttribute != null)
			{
				// An attribute

				if (atts.XmlElements != null && atts.XmlElements.Count > 0)
					throw new Exception ("XmlAttributeAttribute and XmlElementAttribute cannot be applied to the same member");

				XmlTypeMapMemberAttribute mapAttribute = new XmlTypeMapMemberAttribute ();
				if (atts.XmlAttribute.AttributeName == null) 
					mapAttribute.AttributeName = rmember.MemberName;
				else 
					mapAttribute.AttributeName = atts.XmlAttribute.AttributeName;

				mapAttribute.Form = atts.XmlAttribute.Form;
				mapAttribute.Namespace = (atts.XmlAttribute.Namespace != null) ? atts.XmlAttribute.Namespace : "";
				if (typeData.IsComplexType)
					mapAttribute.MappedType = ImportTypeMapping (typeData.Type, null, mapAttribute.Namespace);

				typeData = TypeTranslator.GetTypeData(rmember.MemberType, atts.XmlAttribute.DataType);
				mapMember = mapAttribute;
			}
			else if (typeData.SchemaType == SchemaTypes.Array)
			{
				// If the member has a single XmlElementAttribute and the type is the type of the member,
				// then it is not a flat list
				
				if (atts.XmlElements.Count > 1 ||
				   (atts.XmlElements.Count == 1 && atts.XmlElements[0].Type != typeData.Type) ||
				   (atts.XmlText != null))
				{
					// A flat list

					// TODO: check that it does not have XmlArrayAttribute
					XmlTypeMapMemberFlatList member = new XmlTypeMapMemberFlatList ();
					member.ListMap = new ListMap ();
					member.ListMap.ItemInfo = ImportElementInfo (rmember.MemberName, defaultNamespace, typeData.ListItemType, member, atts);
					member.ElementInfo = member.ListMap.ItemInfo;
					mapMember = member;
				}
				else
				{
					// A list

					XmlTypeMapMemberList member = new XmlTypeMapMemberList ();

					// Creates an ElementInfo that identifies the array instance. 
					member.ElementInfo = new XmlTypeMapElementInfoList();
					XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, typeData);
					elem.ElementName = (atts.XmlArray != null && atts.XmlArray.ElementName != null) ? atts.XmlArray.ElementName : rmember.MemberName;
					elem.Namespace = (atts.XmlArray != null && atts.XmlArray.Namespace != null) ? atts.XmlArray.Namespace : defaultNamespace;
					elem.MappedType = ImportListMapping (rmember.MemberType, null, elem.Namespace, atts, 0);
					member.ElementInfo.Add (elem);
					mapMember = member;
				}
			}
			else
			{
				// An element

				XmlTypeMapMemberElement member = new XmlTypeMapMemberElement ();
				member.ElementInfo = ImportElementInfo (rmember.MemberName, defaultNamespace, rmember.MemberType, member, atts);
				mapMember = member;
			}

			mapMember.DefaultValue = atts.XmlDefaultValue;
			mapMember.TypeData = typeData;
			mapMember.Name = rmember.MemberName;
			return mapMember;
		}

		XmlTypeMapElementInfoList ImportElementInfo (string defaultName, string defaultNamespace, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			ImportTextElementInfo (list, defaultType, member, atts);
			
			if (atts.XmlElements.Count == 0 && list.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(defaultType));
				elem.ElementName = defaultName;
				elem.Namespace = defaultNamespace;
				if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (defaultType, null, defaultNamespace);
				list.Add (elem);
			}

			bool multiType = (atts.XmlElements.Count > 1);
			foreach (XmlElementAttribute att in atts.XmlElements)
			{
				Type elemType = (att.Type != null) ? att.Type : defaultType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(elemType, att.DataType));
				elem.ElementName = (att.ElementName != null) ? att.ElementName : defaultName;
				elem.Namespace = (att.Namespace != null) ? att.Namespace : defaultNamespace;
				elem.Form = att.Form;
				elem.IsNullable = att.IsNullable;
				if (elem.TypeData.IsComplexType)
				{
					if (att.DataType != null) throw new InvalidOperationException ("'" + att.DataType + "' is an invalid value for the XmlElementAttribute.DateTime property. The property may only be specified for primitive types.");
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);
				}

				if (att.ElementName != null) 
					elem.ElementName = att.ElementName;
				else if (multiType) {
					if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
					else elem.ElementName = TypeTranslator.GetTypeData(elemType).XmlType;
				}
				else
					elem.ElementName = defaultName;

				list.Add (elem);
			}
			return list;
		}

		XmlTypeMapElementInfoList ImportAnyElementInfo (string defaultNamespace, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			ImportTextElementInfo (list, defaultType, member, atts);

			foreach (XmlAnyElementAttribute att in atts.XmlAnyElements)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(typeof(XmlElement)));
				if (att.Name != null && att.Name != string.Empty) elem.ElementName = att.Name;
				else elem.IsUnnamedAnyElement = true;
				elem.Namespace = (att.Namespace != null) ? att.Namespace : "";
				list.Add (elem);
			}
			return list;
		}

		void ImportTextElementInfo (XmlTypeMapElementInfoList list, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			if (atts.XmlText != null)
			{
				member.IsXmlTextCollector = true;
				if (atts.XmlText.Type != null) defaultType = atts.XmlText.Type;
				if (defaultType == typeof(XmlNode)) defaultType = typeof(XmlText);	// Nodes must be text nodes

				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(defaultType, atts.XmlText.DataType));

				if (elem.TypeData.SchemaType != SchemaTypes.Primitive &&
					elem.TypeData.SchemaType != SchemaTypes.Enum &&
				    elem.TypeData.SchemaType != SchemaTypes.XmlNode &&
				    !(elem.TypeData.SchemaType == SchemaTypes.Array && elem.TypeData.ListItemTypeData.SchemaType == SchemaTypes.XmlNode)
				 )
					throw new InvalidOperationException ("XmlText cannot be used to encode complex types");

				elem.IsTextElement = true;
				elem.WrappedElement = false;
				list.Add (elem);
			}
		}
		
		bool CanBeNull (TypeData type)
		{
			return (type.SchemaType != SchemaTypes.Primitive || type.Type == typeof (string));
		}
		
		public void IncludeType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (includedTypes == null) includedTypes = new ArrayList ();
			includedTypes.Add (type);
		}

		#endregion // Methods
	}
}
