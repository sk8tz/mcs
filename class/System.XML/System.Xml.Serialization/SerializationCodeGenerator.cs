//
// System.Xml.Serialization.SerializationCodeGenerator.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
//

using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections;

namespace System.Xml.Serialization
{
	internal class SerializationCodeGenerator
	{
		XmlMapping _typeMap;
		SerializationFormat _format;
		TextWriter _writer;
		int _tempVarId = 0;
		int _indent = 0;
		Hashtable _uniqueNames = new Hashtable();
		int _methodId = 0;
		SerializerInfo _config;
		ArrayList _mapsToGenerate = new ArrayList ();

		public SerializationCodeGenerator (XmlMapping typeMap): this (typeMap, null)
		{
		}
		
		public static void Generate (string configFileName, string outputPath)
		{
			SerializationCodeGeneratorConfiguration cnf = null;
			StreamReader sr = new StreamReader (configFileName);
			try
			{
				XmlSerializer ser = new XmlSerializer (typeof (SerializationCodeGeneratorConfiguration));
				cnf = (SerializationCodeGeneratorConfiguration) ser.Deserialize (sr);
			}
			finally
			{
				sr.Close ();
			}
			
			if (outputPath == null) outputPath = "";
			
			CodeIdentifiers ids = new CodeIdentifiers ();
			if (cnf.Serializers != null)
			{
				foreach (SerializerInfo info in cnf.Serializers)
				{					
					Type type;
					if (info.Assembly != null)
					{
						Assembly asm;
						try {
							asm = Assembly.Load (info.Assembly);
						} catch {
							asm = Assembly.LoadFrom (info.Assembly);
						}
						type = asm.GetType (info.ClassName, true);
					}
					else
						type = Type.GetType (info.ClassName);
					
					if (type == null) throw new InvalidOperationException ("Type " + info.ClassName + " not found");
					
					string file = info.OutFileName;
					if (file == null || file == "") {
						int i = info.ClassName.LastIndexOf (".");
						if (i != -1) file = info.ClassName.Substring (i+1);
						else file = info.ClassName;
						file = ids.AddUnique (file, type) + "Serializer.cs";
					}
					StreamWriter writer = new StreamWriter (Path.Combine (outputPath, file));
					
					try
					{
						XmlTypeMapping map;
						
						if (info.SerializationFormat == SerializationFormat.Literal) {
							XmlReflectionImporter ri = new XmlReflectionImporter ();
							map = ri.ImportTypeMapping (type);
						}
						else {
							SoapReflectionImporter ri = new SoapReflectionImporter ();
							map = ri.ImportTypeMapping (type);
						}
						
						SerializationCodeGenerator gen = new SerializationCodeGenerator (map, info);
						gen.GenerateSerializers (writer);
					}
					finally
					{
						writer.Close ();
					}
				}
			}
		}
		
		public SerializationCodeGenerator (XmlMapping typeMap, SerializerInfo config)
		{
			_typeMap = typeMap;
			_format = typeMap.Format;
			
			string typeName;
			if (_typeMap is XmlTypeMapping) typeName = ((XmlTypeMapping)_typeMap).TypeName;
			else typeName = ((XmlMembersMapping)_typeMap).ElementName;
				
			if (config == null)
				config = new SerializerInfo();
			
			if (config.ReaderClassName == null || config.ReaderClassName == "")
				config.ReaderClassName = typeName + "_Reader";

			if (config.WriterClassName == null || config.WriterClassName == "")
				config.WriterClassName = typeName + "_Writer";

			if (config.Namespace == null || config.Namespace == "")
				config.Namespace = "CustomSerializers";

			_config = config;
		}
		
		public void GenerateSerializers (TextWriter writer)
		{
			_writer = writer;

			WriteLine ("using System;");
			WriteLine ("using System.Xml;");
			WriteLine ("using System.Xml.Serialization;");
			WriteLine ("using System.Text;");
			WriteLine ("using System.Collections;");
			WriteLine ("using System.Globalization;");
			WriteLine ("");
			WriteLine ("namespace " + _config.Namespace);
			WriteLineInd ("{");
			
			GenerateReader ();
			WriteLine ("");
			GenerateWriter ();
			
			WriteLineUni ("}");
		}

		#region Writer Generation

		//*******************************************************
		// Writer generation
		//

		public void GenerateWriter ()
		{
			WriteLine ("public class " + _config.WriterClassName + " : XmlSerializationWriter");
			WriteLineInd ("{");

			_mapsToGenerate = new ArrayList ();
			
			InitHooks ();
			GenerateWriteTree ();
			
			if (_typeMap is XmlMembersMapping)
				GenerateWriteMessage ((XmlMembersMapping)_typeMap);
			
			for (int n=0; n<_mapsToGenerate.Count; n++)
			{
				XmlTypeMapping map = (XmlTypeMapping) _mapsToGenerate[n];
				GenerateWriteObject (map);
				if (map.TypeData.SchemaType == SchemaTypes.Enum)
					GenerateGetXmlEnumValue (map);
			}
			
			GenerateWriteInitCallbacks ();

			WriteLineUni ("}");
		}
		
		void GenerateWriteTree ()
		{
			WriteLine ("public void WriteTree (" + GetRootTypeName () + " ob)");
			WriteLineInd ("{");
			WriteLine ("WriteStartDocument ();");
			
			if (_typeMap is XmlTypeMapping)
			{
				XmlTypeMapping mp = (XmlTypeMapping) _typeMap;
				if (mp.TypeData.SchemaType == SchemaTypes.Class || mp.TypeData.SchemaType == SchemaTypes.Array) 
					WriteLine ("TopLevelElement ();");

				if (_format == SerializationFormat.Literal) {
					WriteLine (GetWriteObjectName (mp) + " (ob, " + GetLiteral(mp.ElementName) + ", " + GetLiteral(mp.Namespace) + ", true, false, true);");
				}
				else
					WriteLine ("WritePotentiallyReferencingElement (" + GetLiteral(mp.ElementName) + ", " + GetLiteral(mp.Namespace) + ", ob, " + GetTypeOf(mp.TypeData) + ", true, false);");
			}
			else if (_typeMap is XmlMembersMapping) {
				WriteLine ("WriteMessage (ob);");
			}
			else
				throw new InvalidOperationException ("Unknown type map");

			if (_format == SerializationFormat.Encoded)
				WriteLine ("WriteReferencedElements ();");

			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateWriteMessage (XmlMembersMapping map)
		{
		}
		
		void GenerateGetXmlEnumValue (XmlTypeMapping map)
		{
			EnumMap emap = (EnumMap) map.ObjectMap;

			WriteLine ("string " + GetGetEnumValueName (map) + " (" + map.TypeFullName + " val)");
			WriteLineInd ("{");

			WriteLine ("switch (val)");
			WriteLineInd ("{");
			foreach (EnumMap.EnumMapMember mem in emap.Members)
				WriteLine ("case " + map.TypeFullName + "." + mem.EnumName + ": return " + GetLiteral (mem.XmlName) + ";");

			if (emap.IsFlags)
			{
				WriteLineInd ("default:");
				WriteLine ("System.Text.StringBuilder sb = new System.Text.StringBuilder ();");
				WriteLine ("string[] enumNames = val.ToString().Split (',');");
				WriteLine ("foreach (string name in enumNames)");
				WriteLineInd ("{");
				WriteLine ("switch (name.Trim())");
				WriteLineInd ("{");
				
				foreach (EnumMap.EnumMapMember mem in emap.Members)
					WriteLine ("case " + GetLiteral(mem.EnumName) + ": sb.Append (" + GetLiteral(mem.XmlName) + ").Append (' '); break; ");

				WriteLine ("default: sb.Append (name.Trim()).Append (' '); break; ");
				WriteLineUni ("}");
				WriteLineUni ("}");
				WriteLine ("return sb.ToString ().Trim();");
				Unindent ();
			}
			else
				WriteLine ("default: return val.ToString();");
			
			WriteLineUni ("}");
			
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateWriteObject (XmlTypeMapping typeMap)
		{
			WriteLine ("void " + GetWriteObjectName (typeMap) + " (" + typeMap.TypeFullName + " ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)");
			WriteLineInd ("{");
			
			PushHookContext ();
			
			SetHookVar ("$TYPE", typeMap.TypeName);
			SetHookVar ("$FULLTYPE", typeMap.TypeFullName);
			SetHookVar ("$OBJECT", "ob");
			SetHookVar ("$ELEMENT", "element");
			SetHookVar ("$NAMESPACE", "namesp");
			SetHookVar ("$NULLABLE", "isNullable");

			if (GenerateWriteHook (HookType.type, typeMap.TypeData.Type))
			{
				WriteLineUni ("}");
				WriteLine ("");
				PopHookContext ();
				return;
			}
			
			if (typeMap.TypeData.SchemaType != SchemaTypes.Enum)
			{
				WriteLine ("if (ob == null)");
				WriteLineInd ("{");
				WriteLineInd ("if (isNullable)");
				
				if (_format == SerializationFormat.Literal) 
					WriteLine ("WriteNullTagLiteral(element, namesp);");
				else 
					WriteLine ("WriteNullTagEncoded (element, namesp);");
				
				WriteLineUni ("return;");
				WriteLineUni ("}");
				WriteLine ("");
			}

			if (typeMap.TypeData.SchemaType == SchemaTypes.XmlNode)
			{
				if (_format == SerializationFormat.Literal)
					WriteLine ("WriteElementLiteral (ob, \"\", \"\", true, false);");
				else 
					WriteLine ("WriteElementEncoded (ob, \"\", \"\", true, false);");
					
				GenerateEndHook ();
				WriteLineUni ("}");
				WriteLine ("");
				PopHookContext ();
				return;
			}

			if (typeMap.TypeData.SchemaType == SchemaTypes.XmlSerializable)
			{
				WriteLine ("WriteSerializable (ob, element, namesp, isNullable);");
				
				GenerateEndHook ();
				WriteLineUni ("}");
				WriteLine ("");
				PopHookContext ();
				return;
			}

			ArrayList types = typeMap.DerivedTypes;
			bool first = true;
			for (int n=0; n<types.Count; n++)
			{
				XmlTypeMapping map = (XmlTypeMapping)types[n];
				
				WriteLineInd ((first?"else ":"") + "if (ob is " + map.TypeFullName + ") { ");
				WriteLine (GetWriteObjectName (map) + "((" + map.TypeFullName + ")ob, element, namesp, isNullable, true, false);");
				WriteLine ("return;");
				WriteLineUni ("}");
				first = false;
			}
			if (typeMap.TypeData.Type == typeof (object)) {
				WriteLineInd ("else {");
				WriteLine ("WriteTypedPrimitive (element, namesp, ob, true);");
				WriteLine ("return;");
				WriteLineUni ("}");
			}

			if (types.Count > 0)
				WriteLine ("");
			
			WriteLineInd ("if (writeWrappingElem) {");
			if (_format == SerializationFormat.Encoded) WriteLine ("needType = true;");
			WriteLine ("WriteStartElement (element, namesp, ob);");
			WriteLineUni ("}");
			WriteLine ("");

			WriteLine ("if (needType) WriteXsiType(" + GetLiteral(typeMap.XmlType) + ", " + GetLiteral(typeMap.Namespace) + ");");
			WriteLine ("");

			switch (typeMap.TypeData.SchemaType)
			{
				case SchemaTypes.Class: GenerateWriteObjectElement (typeMap, "ob", false); break;
				case SchemaTypes.Array: GenerateWriteListElement (typeMap, "ob"); break;
				case SchemaTypes.Primitive: GenerateWritePrimitiveElement (typeMap, "ob"); break;
				case SchemaTypes.Enum: GenerateWriteEnumElement (typeMap, "ob"); break;
			}

			WriteLine ("if (writeWrappingElem) WriteEndElement (ob);");

			GenerateEndHook ();
			WriteLineUni ("}");
			WriteLine ("");
			PopHookContext ();
		}

		void GenerateWriteObjectElement (XmlTypeMapping typeMap, string ob, bool isValueList)
		{
			ClassMap map = (ClassMap)typeMap.ObjectMap;
			if (!GenerateWriteHook (HookType.attributes, typeMap.TypeData.Type))
			{
				if (map.NamespaceDeclarations != null) {
					WriteLine ("WriteNamespaceDeclarations ((XmlSerializerNamespaces) " + ob + "." + map.NamespaceDeclarations.Name + ");");
					WriteLine ("");
				}
				
				ICollection attributes = map.AttributeMembers;
				if (attributes != null)
				{
					foreach (XmlTypeMapMemberAttribute attr in attributes) 
					{
						if (GenerateWriteMemberHook (typeMap.TypeData.Type, attr)) continue;
					
						string val = GenerateGetMemberValue (attr, ob, isValueList);
						string cond = GenerateMemberHasValueCondition (attr, ob, isValueList);
						
						if (cond != null) WriteLineInd ("if (" + cond + ") {");
						
						string strVal = GenerateGetStringValue (attr.MappedType, attr.TypeData, val);
						WriteLine ("WriteAttribute (" + GetLiteral(attr.AttributeName) + ", " + GetLiteral(attr.Namespace) + ", " + strVal + ");");
	
						if (cond != null) WriteLineUni ("}");
						GenerateEndHook ();
					}
					WriteLine ("");
				}
	
				XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
				if (anyAttrMember != null) 
				{
					if (!GenerateWriteMemberHook (typeMap.TypeData.Type, anyAttrMember))
					{
						string cond = GenerateMemberHasValueCondition (anyAttrMember, ob, isValueList);
						if (cond != null) WriteLineInd ("if (" + cond + ") {");
		
						string tmpVar = GetObTempVar ();
						WriteLine ("ICollection " + tmpVar + " = " + GenerateGetMemberValue (anyAttrMember, ob, isValueList) + ";");
						WriteLineInd ("if (" + tmpVar + " != null) {");
		
						string tmpVar2 = GetObTempVar ();
						WriteLineInd ("foreach (XmlAttribute " + tmpVar2 + " in " + tmpVar + ")");
						WriteLine ("WriteXmlAttribute (" + tmpVar2 + ", " + ob + ");");
						Unindent ();
						WriteLineUni ("}");
		
						if (cond != null) WriteLineUni ("}");
						WriteLine ("");
						GenerateEndHook ();
					}
				}
				GenerateEndHook ();
			}
			
			if (!GenerateWriteHook (HookType.elements, typeMap.TypeData.Type))
			{
				ICollection members = map.ElementMembers;
				if (members != null)
				{
					foreach (XmlTypeMapMemberElement member in members)
					{
						if (GenerateWriteMemberHook (typeMap.TypeData.Type, member)) continue;
						
						string cond = GenerateMemberHasValueCondition (member, ob, isValueList);
						if (cond != null) WriteLineInd ("if (" + cond + ") {");
						
						string memberValue = GenerateGetMemberValue (member, ob, isValueList);
						Type memType = member.GetType();
	
						if (memType == typeof(XmlTypeMapMemberList))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteMemberElement ((XmlTypeMapElementInfo) member.ElementInfo[0], memberValue);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberFlatList))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteListContent (member.TypeData, ((XmlTypeMapMemberFlatList)member).ListMap, memberValue, false);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberAnyElement))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberAnyElement))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberAnyAttribute))
						{
							// Ignore
						}
						else if (memType == typeof(XmlTypeMapMemberElement))
						{
							if (member.ElementInfo.Count == 1) {
								GenerateWriteMemberElement ((XmlTypeMapElementInfo)member.ElementInfo[0], memberValue);
							}
							else if (member.ChoiceMember != null)
							{
								string choiceValue = ob + "." + member.ChoiceMember;
								foreach (XmlTypeMapElementInfo elem in member.ElementInfo) {
									WriteLineInd ("if (" + choiceValue + " == " + GetLiteral(elem.ChoiceValue) + ") {");
									GenerateWriteMemberElement (elem, GetCast(elem.TypeData, member.TypeData, memberValue));
									WriteLineUni ("}");
								}
							}
							else
							{
	//							WriteLineInd ("if (" + memberValue + " == null) {");
	//							GenerateWriteMemberElement ((XmlTypeMapElementInfo)member.ElementInfo[0], memberValue);
	//							WriteLineUni ("}");
									
								bool first = true;
								Type type = memberValue.GetType();
								foreach (XmlTypeMapElementInfo elem in member.ElementInfo)
								{
									WriteLineInd ((first?"":"else ") + "if (" + memberValue + " is " + elem.TypeData.FullTypeName + ") {");
									GenerateWriteMemberElement (elem, GetCast(elem.TypeData, member.TypeData, memberValue));
									WriteLineUni ("}");
									first = false;
								}
							}
						}
						else
							throw new InvalidOperationException ("Unknown member type");
							
						if (cond != null)
							WriteLineUni ("}");
							
						GenerateEndHook ();
					}
				}
				GenerateEndHook ();
			}
		}
		
		void GenerateWriteMemberElement (XmlTypeMapElementInfo elem, string memberValue)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					string elemName = elem.WrappedElement ? elem.ElementName : "";
					if (_format == SerializationFormat.Literal) 
						WriteMetCall ("WriteElementLiteral", memberValue, GetLiteral(elemName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable), "false");
					else 
						WriteMetCall ("WriteElementEncoded", memberValue, GetLiteral(elemName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable), "false");
					break;

				case SchemaTypes.Enum:
				case SchemaTypes.Primitive:
					if (_format == SerializationFormat.Literal) 
						GenerateWritePrimitiveValueLiteral (memberValue, elem.ElementName, elem.Namespace, elem.MappedType, elem.TypeData, elem.WrappedElement, elem.IsNullable);
					else
						GenerateWritePrimitiveValueEncoded (memberValue, elem.ElementName, elem.Namespace, new XmlQualifiedName (elem.TypeData.XmlType, elem.DataTypeNamespace), elem.MappedType, elem.TypeData, elem.WrappedElement, elem.IsNullable);
					break;

				case SchemaTypes.Array:
					if (memberValue == null) {
						if (_format == SerializationFormat.Literal) 
							WriteMetCall ("WriteNullTagLiteral", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace));
						else
							WriteMetCall ("WriteNullTagEncoded", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace));
					}
					else if (elem.MappedType.MultiReferenceType) 
						WriteMetCall ("WriteReferencingElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue, GetLiteral(elem.IsNullable));
					else {
						WriteMetCall ("WriteStartElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue);
						GenerateWriteListContent (elem.TypeData, (ListMap) elem.MappedType.ObjectMap, memberValue, false);
						WriteMetCall ("WriteEndElement", memberValue);
					}
					break;

				case SchemaTypes.Class:
					if (elem.MappedType.MultiReferenceType)	{
						if (elem.MappedType.TypeData.Type == typeof(object))
							WriteMetCall ("WritePotentiallyReferencingElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue, "null", "false", GetLiteral(elem.IsNullable));
						else
							WriteMetCall ("WriteReferencingElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue, GetLiteral(elem.IsNullable));
					}
					else 
						WriteMetCall (GetWriteObjectName(elem.MappedType), memberValue, GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable), "false", "true");
					break;

				case SchemaTypes.XmlSerializable:
					WriteMetCall ("WriteSerializable", memberValue, GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable));
					break;

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}		

		void GenerateWriteListElement (XmlTypeMapping typeMap, string ob)
		{
			if (_format == SerializationFormat.Encoded)
			{
				string n, ns;
				string itemCount = GenerateGetListCount (typeMap.TypeData, ob);
				GenerateGetArrayType ((ListMap) typeMap.ObjectMap, itemCount, out n, out ns);
				
				string arrayType;
				if (ns != string.Empty)
					arrayType = "FromXmlQualifiedName (new XmlQualifiedName(" + n + "," + ns + "))";
				else
					arrayType = GetLiteral (n);
				
				WriteMetCall ("WriteAttribute", GetLiteral("arrayType"), GetLiteral(XmlSerializer.WsdlNamespace), arrayType);
			}
			GenerateWriteListContent (typeMap.TypeData, (ListMap) typeMap.ObjectMap, ob, false);
		}
		
		void GenerateWriteAnyElementContent (XmlTypeMapMemberAnyElement member, string memberValue)
		{
			bool singleElement = (member.TypeData.Type == typeof (XmlElement));
			string var;
			
			if (singleElement)
				var = memberValue;
			else {
				var = GetObTempVar ();
				WriteLineInd ("foreach (XmlNode " + var + " in " + memberValue + ") {");
			}
			
			string elem = GetObTempVar ();
			WriteLine ("XmlElement " + elem + " = " + var + " as XmlElement;");
			WriteLine ("if (" + elem + " == null) throw CreateUnknownTypeException (" + elem + ");");
			
			if (!member.IsDefaultAny) {
				for (int n=0; n<member.ElementInfo.Count; n++) {
					XmlTypeMapElementInfo info = (XmlTypeMapElementInfo)member.ElementInfo[n];
					string txt = "(" + elem + ".Name == " + GetLiteral(info.ElementName) + " && " + elem + ".NamespaceURI == " + GetLiteral(info.Namespace) + ")";
					if (n == member.ElementInfo.Count-1) txt += ") {";
					if (n == 0) WriteLineInd ("if (" + txt);
					else WriteLine ("|| " + txt);
				}				
			}
			
			if (_format == SerializationFormat.Literal) 
				WriteLine ("WriteElementLiteral (" + elem + ", \"\", \"\", false, true);");
			else 
				WriteLine ("WriteElementEncoded (" + elem + ", \"\", \"\", false, true);");

			if (!member.IsDefaultAny) {
				WriteLineUni ("}");
				WriteLineInd ("else");
				WriteLine ("throw CreateUnknownAnyElementException (" + elem + ".Name, " + elem + ".NamespaceURI);");
				Unindent ();
			}
			
			if (!singleElement)
				WriteLineUni ("}");
		}

		void GenerateWritePrimitiveElement (XmlTypeMapping typeMap, string ob)
		{
			string strVal = GenerateGetStringValue (typeMap, typeMap.TypeData, ob);
			WriteLine ("Writer.WriteString (" + strVal + ");");
		}

		void GenerateWriteEnumElement (XmlTypeMapping typeMap, string ob)
		{
			string strVal = GenerateGetEnumXmlValue (typeMap, ob);
			WriteLine ("Writer.WriteString (" + strVal + ");");
		}

		string GenerateGetStringValue (XmlTypeMapping typeMap, TypeData type, string value)
		{
			if (type.SchemaType == SchemaTypes.Array) {
				string str = GetStrTempVar ();
				WriteLine ("string " + str + " = null;");
				WriteLineInd ("if (" + value + " != null) {");
				string res = GenerateWriteListContent (typeMap.TypeData, (ListMap)typeMap.ObjectMap, value, true);
				WriteLine (str + " = " + res + ".ToString ().Trim ();");
				WriteLineUni ("}");
				return str;
			}
			else if (type.SchemaType == SchemaTypes.Enum) {
				return GenerateGetEnumXmlValue (typeMap, value);
			}
			else if (type.Type == typeof (XmlQualifiedName))
				return "FromXmlQualifiedName (" + value + ")";
			else if (value == null)
				return null;
			else
				return XmlCustomFormatter.GenerateToXmlString (type, value);
		}

		string GenerateGetEnumXmlValue (XmlTypeMapping typeMap, string ob)
		{
			return GetGetEnumValueName (typeMap) + " (" + ob + ")";
		}

		string GenerateGetListCount (TypeData listType, string ob)
		{
			if (listType.Type.IsArray)
				return "ob.Length";
			else
				return "ob.Count";
		}

		void GenerateGetArrayType (ListMap map, string itemCount, out string localName, out string ns)
		{
			string arrayDim;
			if (itemCount != "") arrayDim = "";
			else arrayDim = "[]";

			XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) map.ItemInfo[0];
			if (info.TypeData.SchemaType == SchemaTypes.Array)
			{
				string nm;
				GenerateGetArrayType ((ListMap)info.MappedType.ObjectMap, "", out nm, out ns);
				localName = nm + arrayDim;
			}
			else 
			{
				if (info.MappedType != null)
				{
					localName = info.MappedType.XmlType + arrayDim;
					ns = info.MappedType.Namespace;
				}
				else 
				{
					localName = info.TypeData.XmlType + arrayDim;
					ns = info.DataTypeNamespace;
				}
			}
			if (itemCount != "") {
				localName = "\"" + localName + "[\" + " + itemCount + " + \"]\"";
				ns = GetLiteral (ns);
			}
		}

		string GenerateWriteListContent (TypeData listType, ListMap map, string ob, bool writeToString)
		{
			string targetString = null;
			
			if (writeToString)
			{
				targetString = GetStrTempVar ();
				WriteLine ("System.Text.StringBuilder " + targetString + " = new System.Text.StringBuilder();");
			}
			
			if (listType.Type.IsArray)
			{
				string itemVar = GetNumTempVar ();
				WriteLineInd ("for (int "+itemVar+" = 0; "+itemVar+" < " + ob + ".Length; "+itemVar+"++) {");
				GenerateListLoop (map, ob + "["+itemVar+"]", listType.ListItemTypeData, targetString);
				WriteLineUni ("}");
			}
			else if (typeof(ICollection).IsAssignableFrom (listType.Type))
			{
				string itemVar = GetNumTempVar ();
				WriteLineInd ("for (int "+itemVar+" = 0; "+itemVar+" < " + ob + ".Count; "+itemVar+"++) {");
				GenerateListLoop (map, ob + "["+itemVar+"]", listType.ListItemTypeData, targetString);
				WriteLineUni ("}");
			}
			else if (typeof(IEnumerable).IsAssignableFrom (listType.Type))
			{
				string itemVar = GetObTempVar ();
				WriteLineInd ("foreach (" + listType.ListItemTypeData.FullTypeName + " " + itemVar + " in " + ob + ") {");
				GenerateListLoop (map, itemVar, listType.ListItemTypeData, targetString);
				WriteLineUni ("}");
			}
			else
				throw new Exception ("Unsupported collection type");

			return targetString;
		}
		
		void GenerateListLoop (ListMap map, string item, TypeData itemTypeData, string targetString)
		{
			bool first = true;
			foreach (XmlTypeMapElementInfo info in map.ItemInfo)
			{
				if (map.ItemInfo.Count > 1)
					WriteLineInd ((first?"":"else ") + "if (" + item + " is " + info.TypeData.FullTypeName + ") {");

				if (targetString == null) 
					GenerateWriteMemberElement (info, GetCast (info.TypeData, itemTypeData, item));
				else
				{
					string strVal = GenerateGetStringValue (info.MappedType, info.TypeData, GetCast (info.TypeData, itemTypeData, item));
					WriteLine (targetString + ".Append (" + strVal + ").Append (\" \");");
				}

				if (map.ItemInfo.Count > 1)
					WriteLineUni ("}");
				
				first = false;
			}
			
			if (map.ItemInfo.Count > 1)
				WriteLine ("else if (" + item + " != null) throw CreateUnknownTypeException (" + item + ");");
		}

		void GenerateWritePrimitiveValueLiteral (string memberValue, string name, string ns, XmlTypeMapping mappedType, TypeData typeData, bool wrapped, bool isNullable)
		{
			if (!wrapped) {
				string strVal = GenerateGetStringValue (mappedType, typeData, memberValue);
				WriteMetCall ("WriteValue", strVal);
			}
			else if (isNullable) {
				if (typeData.Type == typeof(XmlQualifiedName)) 
					WriteMetCall ("WriteNullableQualifiedNameLiteral", GetLiteral(name), GetLiteral(ns), memberValue);
				else  {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue);
					WriteMetCall ("WriteNullableStringLiteral", GetLiteral(name), GetLiteral(ns), strVal);
				}
			}
			else {
				if (typeData.Type == typeof(XmlQualifiedName))
					WriteMetCall ("WriteElementQualifiedName", GetLiteral(name), GetLiteral(ns), memberValue);
				else {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue);
					WriteMetCall ("WriteElementString", GetLiteral(name),GetLiteral(ns), strVal);
				}
			}
		}
		
		void GenerateWritePrimitiveValueEncoded (string memberValue, string name, string ns, XmlQualifiedName xsiType, XmlTypeMapping mappedType, TypeData typeData, bool wrapped, bool isNullable)
		{
			if (!wrapped) {
				string strVal = GenerateGetStringValue (mappedType, typeData, memberValue);
				WriteMetCall ("WriteValue", strVal);
			}
			else if (isNullable) {
				if (typeData.Type == typeof(XmlQualifiedName)) 
					WriteMetCall ("WriteNullableQualifiedNameEncoded", GetLiteral(name), GetLiteral(ns), memberValue, GetLiteral(xsiType));
				else  {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue);
					WriteMetCall ("WriteNullableStringEncoded", GetLiteral(name), GetLiteral(ns), strVal, GetLiteral(xsiType));
				}
			}
			else {
				if (typeData.Type == typeof(XmlQualifiedName))
					WriteMetCall ("WriteElementQualifiedName", GetLiteral(name), GetLiteral(ns), memberValue, GetLiteral(xsiType));
				else {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue);
					WriteMetCall ("WriteElementString", GetLiteral(name),GetLiteral(ns), strVal, GetLiteral(xsiType));
				}
			}
		}

		string GenerateGetMemberValue (XmlTypeMapMember member, string ob, bool isValueList)
		{
			if (isValueList) return ob + "[" + member.Index + "]";
			else return ob + "." + member.Name;
		}
		
		string GenerateMemberHasValueCondition (XmlTypeMapMember member, string ob, bool isValueList)
		{
			if (isValueList) {
				return member.Index + " < " + ob + ".Length";
			}
			else if (member.DefaultValue != System.DBNull.Value) {
				string mem = ob + "." + member.Name;
				if (member.DefaultValue == null) 
					return mem + " != null";
				else if (member.TypeData.SchemaType == SchemaTypes.Enum)
					return mem + " != " + GetCast (member.TypeData, GetLiteral (member.DefaultValue));
				else 
					return mem + " != " + GetLiteral (member.DefaultValue);
			}
			return null;
		}

		void GenerateWriteInitCallbacks ()
		{
			WriteLine ("protected override void InitCallbacks ()");
			WriteLineInd ("{");
			
			if (_format == SerializationFormat.Encoded)
			{
				foreach (XmlTypeMapping map in _mapsToGenerate)  {
					WriteMetCall ("AddWriteCallback", GetTypeOf(map.TypeData), GetLiteral(map.XmlType), GetLiteral(map.Namespace), "new XmlSerializationWriteCallback (" + GetWriteObjectCallbackName (map) + ")");
				}
			}	
			
			WriteLineUni ("}");
			WriteLine ("");
				
			if (_format == SerializationFormat.Encoded)
			{
				foreach (XmlTypeMapping map in _mapsToGenerate)  {
					if (map.TypeData.SchemaType == SchemaTypes.Enum)
						WriteWriteEnumCallback (map);
					else
						WriteWriteObjectCallback (map);
				}
			}
		}
		
		void WriteWriteEnumCallback (XmlTypeMapping map)
		{
			WriteLine ("void " + GetWriteObjectCallbackName (map) + " (object ob)");
			WriteLineInd ("{");
			WriteMetCall (GetWriteObjectName(map), GetCast (map.TypeData, "ob"), GetLiteral(map.ElementName), GetLiteral(map.Namespace), "false", "false", "false");
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void WriteWriteObjectCallback (XmlTypeMapping map)
		{
			WriteLine ("void " + GetWriteObjectCallbackName (map) + " (object ob)");
			WriteLineInd ("{");
			WriteMetCall (GetWriteObjectName(map), GetCast (map.TypeData, "ob"), GetLiteral(map.ElementName), GetLiteral(map.Namespace), "false", "true", "false");
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		#endregion
		
		#region Reader Generation

		//*******************************************************
		// Reader generation
		//
		
		public void GenerateReader ()
		{
			WriteLine ("public class " + _config.ReaderClassName + " : XmlSerializationReader");
			WriteLineInd ("{");

			_mapsToGenerate = new ArrayList ();
			InitHooks ();
			GenerateReadTree ();
			
			if (_typeMap is XmlMembersMapping)
				GenerateReadMessage ((XmlMembersMapping)_typeMap);

			for (int n=0; n<_mapsToGenerate.Count; n++)
			{
				XmlTypeMapping map = (XmlTypeMapping) _mapsToGenerate [n];
				GenerateReadObject (map);
				if (map.TypeData.SchemaType == SchemaTypes.Enum)
					GenerateGetEnumValue (map);
			}
			
			GenerateReadInitCallbacks ();
			
			if (_format == SerializationFormat.Encoded)
			{
				GenerateFixupCallbacks ();
				GenerateFillerCallbacks ();
			}
			
			WriteLineUni ("}");
		}
		
		void GenerateReadTree ()
		{
			WriteLine ("public " + GetRootTypeName () + " ReadTree ()");
			WriteLineInd ("{");
			WriteLine ("Reader.MoveToContent();");
			
			if (_typeMap is XmlTypeMapping)
			{
				XmlTypeMapping typeMap = (XmlTypeMapping) _typeMap;

				if (_format == SerializationFormat.Literal)
					WriteMetCall ("return " + GetReadObjectName (typeMap), "true", "true");
				else
				{
					WriteLine (typeMap.TypeFullName + " ob = null;");
					WriteLine ("Reader.MoveToContent();");
					WriteLine ("if (Reader.NodeType == System.Xml.XmlNodeType.Element) ");
					WriteLineInd ("{");
					WriteLineInd ("if (Reader.LocalName == " + GetLiteral(typeMap.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (typeMap.Namespace) + ")");
					WriteLine ("ob = ReadReferencedElement();");
					Unindent ();
					WriteLineInd ("else ");
					WriteLine ("throw CreateUnknownNodeException();");
					Unindent ();
					WriteLineUni ("}");
					WriteLineInd ("else ");
					WriteLine ("UnknownNode(null);");
					Unindent ();
					WriteLine ("");
					WriteLine ("ReadReferencedElements();");
					WriteLine ("return ob;");
				}
			}
			else {
				WriteLine ("return ReadMessage ();");
			}

			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateReadMessage (XmlMembersMapping map)
		{
		}
		
		void GenerateReadObject (XmlTypeMapping typeMap)
		{
			if (_format == SerializationFormat.Literal)
				WriteLine ("public " + typeMap.TypeFullName + " " + GetReadObjectName (typeMap) + " (bool isNullable, bool checkType)");
			else
				WriteLine ("public " + typeMap.TypeFullName + " " + GetReadObjectName (typeMap) + " ()");
			
			WriteLineInd ("{");

			PushHookContext ();
			
			SetHookVar ("$TYPE", typeMap.TypeName);
			SetHookVar ("$FULLTYPE", typeMap.TypeFullName);
			SetHookVar ("$NULLABLE", "isNullable");
			
			switch (typeMap.TypeData.SchemaType)
			{
				case SchemaTypes.Class: GenerateReadClassInstance (typeMap, "isNullable", "checkType"); break;
				case SchemaTypes.Array: 
					WriteLine ("return " + GenerateReadListElement (typeMap, null, "isNullable", true)); 
					break;
				case SchemaTypes.XmlNode: GenerateReadXmlNodeElement (typeMap, "isNullable"); break;
				case SchemaTypes.Primitive: GenerateReadPrimitiveElement (typeMap, "isNullable"); break;
				case SchemaTypes.Enum: GenerateReadEnumElement (typeMap, "isNullable"); break;
				case SchemaTypes.XmlSerializable: GenerateReadXmlSerializableElement (typeMap, "isNullable"); break;
				default: throw new Exception ("Unsupported map type");
			}
			
			WriteLineUni ("}");
			WriteLine ("");
			PopHookContext ();
		}
				
		void GenerateReadClassInstance (XmlTypeMapping typeMap, string isNullable, string checkType)
		{
			WriteLine (typeMap.TypeFullName + " ob = null;");
			SetHookVar ("$OBJECT", "ob");
		
			if (GenerateReadHook (HookType.type, typeMap.TypeData.Type)) {
				WriteLine ("return ob;");
				return;
			}
			
			if (_format == SerializationFormat.Literal) {
				WriteLine ("if (" + isNullable + " && ReadNull()) return null;");
				WriteLine ("");
				WriteLine ("if (checkType) ");
				WriteLineInd ("{");
			}
			else {
				WriteLine ("if (ReadNull()) return null;");
				WriteLine ("");
			}
			
			WriteLine ("System.Xml.XmlQualifiedName t = GetXsiType();");
			WriteLine ("if (t != null) ");
			WriteLineInd ("{");
			
			bool first = true;
			foreach (XmlTypeMapping realMap in typeMap.DerivedTypes)
			{
				WriteLineInd ((first?"":"else ") + "if (t.Name == " + GetLiteral (realMap.XmlType) + " && t.Namespace == " + GetLiteral (realMap.Namespace) + ")");
				WriteLine ("return " + GetReadObjectName(realMap) + " (" + isNullable + ", " + checkType + ");");
				Unindent ();
				first = false;
			}

			WriteLine ((first?"":"else ") + "if (t.Name != " + GetLiteral (typeMap.XmlType) + " || t.Namespace != " + GetLiteral (typeMap.Namespace) + ")");
			if (typeMap.TypeData.Type == typeof(object))
				WriteLine ("\treturn ReadTypedPrimitive (t);");
			else
				WriteLine ("\tthrow CreateUnknownTypeException(t);");

			WriteLineUni ("}");
			
			if (_format == SerializationFormat.Literal)
				WriteLineUni ("}");

			if (typeMap.TypeData.Type.IsAbstract) {
				GenerateEndHook ();
				WriteLine ("return ob;");
				return;
			}

			WriteLine ("");
			WriteLine ("ob = new " + typeMap.TypeFullName + " ();");
			WriteLine ("");
			
			WriteLine ("Reader.MoveToElement();");
			WriteLine ("");
			
			GenerateReadMembers (typeMap, (ClassMap)typeMap.ObjectMap, "ob", false);
			
			WriteLine ("");
			
			GenerateEndHook ();
			WriteLine ("return ob;");
		}

		void GenerateReadMembers (XmlTypeMapping typeMap, ClassMap map, string ob, bool isValueList)
		{
			// A value list cannot have attributes

			bool first;
			if (!isValueList && !GenerateReadHook (HookType.attributes, typeMap.TypeData.Type))
			{
				// Reads attributes
				
				XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
				
				if (anyAttrMember != null)
				{
					WriteLine ("int anyAttributeIndex = 0;");
					WriteLine (anyAttrMember.TypeData.FullTypeName + " anyAttributeArray = null;");
				}
				
				WriteLine ("while (Reader.MoveToNextAttribute())");
				WriteLineInd ("{");
				first = true;
				if (map.AttributeMembers != null) {
					foreach (XmlTypeMapMemberAttribute at in map.AttributeMembers)
					{
						WriteLineInd ((first?"":"else ") + "if (Reader.LocalName == " + GetLiteral (at.AttributeName) + " && Reader.NamespaceURI == " + GetLiteral (at.Namespace) + ") {");
						if (!GenerateReadMemberHook (typeMap.TypeData.Type, at)) {
							GenerateSetMemberValue (at, ob, GenerateGetValueFromXmlString ("Reader.Value", at.TypeData, at.MappedType), isValueList);
							GenerateEndHook ();
						}
						WriteLineUni ("}");
						first = false;
					}
				}
				WriteLineInd ((first?"":"else ") + "if (IsXmlnsAttribute (Reader.Name)) {");

				// If the map has NamespaceDeclarations,
				// then store this xmlns to the given member.
				// If the instance doesn't exist, then create.
				
				if (map.NamespaceDeclarations != null) {
					if (!GenerateReadMemberHook (typeMap.TypeData.Type,map.NamespaceDeclarations)) {
						string nss = ob + "." + map.NamespaceDeclarations.Name;
						WriteLine ("if (" + nss + " == null) " + nss + " = new XmlSerializerNamespaces ();");
						WriteLineInd ("if (Reader.Prefix == \"xmlns\")");
						WriteLine (nss + ".Add (Reader.LocalName, Reader.Value);");
						Unindent ();
						WriteLineInd ("else");
						WriteLine (nss + ".Add (\"\", Reader.Value);");
						Unindent ();
						GenerateEndHook ();
					}
				}
				
				WriteLineUni ("}");
				WriteLineInd ("else {");

				if (anyAttrMember != null) 
				{
					if (!GenerateReadArrayMemberHook (typeMap.TypeData.Type, anyAttrMember, "anyAttributeIndex")) {
						WriteLine ("System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);");
						if (typeof(System.Xml.Schema.XmlSchemaAnnotated).IsAssignableFrom (typeMap.TypeData.Type)) 
							WriteLine ("ParseWsdlArrayType (attr);");
						GenerateAddListValue (anyAttrMember.TypeData, "anyAttributeArray", "anyAttributeIndex", GetCast (anyAttrMember.TypeData.ListItemTypeData, "attr"), true);
						GenerateEndHook ();
					}
					WriteLine ("anyAttributeIndex++;");
				}
				else {
					if (!GenerateReadHook (HookType.unknownAttribute, typeMap.TypeData.Type)) {
						WriteLine ("UnknownNode (" + ob + ");");
						GenerateEndHook ();
					}
				}

				WriteLineUni ("}");
				WriteLineUni ("}");

				if (anyAttrMember != null && !MemberHasReadReplaceHook (typeMap.TypeData.Type, anyAttrMember))
				{
					WriteLine ("");
					WriteLine("anyAttributeArray = (" + anyAttrMember.TypeData.FullTypeName + ") ShrinkArray (anyAttributeArray, anyAttributeIndex, " + GetTypeOf(anyAttrMember.TypeData.Type.GetElementType()) + ", true);");
					GenerateSetMemberValue (anyAttrMember, ob, "anyAttributeArray", isValueList);
				}
				WriteLine ("");
	
				GenerateEndHook ();
			}

			WriteLine ("Reader.MoveToElement();");
			WriteLineInd ("if (Reader.IsEmptyElement) {"); 
			WriteLine ("Reader.Skip ();");
			WriteLine ("return " + ob + ";");
			WriteLineUni ("}");
			WriteLine ("");

			WriteLine ("Reader.ReadStartElement();");

			// Reads elements

			WriteLine("Reader.MoveToContent();");
			WriteLine ("");

			if (!GenerateReadHook (HookType.elements, typeMap.TypeData.Type))
			{
				string[] readFlag = null;
				if (map.ElementMembers != null)
				{
					string readFlagsVars = "bool ";
					readFlag = new string[map.ElementMembers.Count];
					for (int n=0; n<map.ElementMembers.Count; n++) {
						readFlag[n] = GetBoolTempVar ();
						if (n > 0) readFlagsVars += ", ";
						readFlagsVars += readFlag[n] + "=false";
					}
					if (map.ElementMembers.Count > 0) WriteLine (readFlagsVars + ";");
					WriteLine ("");
				}
	
				string[] indexes = null;
				string[] flatLists = null;
	
				if (map.FlatLists != null) 
				{
					indexes = new string[map.FlatLists.Count];
					flatLists = new string[map.FlatLists.Count];
					
					string code = "int ";
					for (int n=0; n<map.FlatLists.Count; n++) 
					{
						XmlTypeMapMember mem = (XmlTypeMapMember)map.FlatLists[n];
						indexes[n] = GetNumTempVar ();
						if (n > 0) code += ", ";
						code += indexes[n] + "=0";
						if (!MemberHasReadReplaceHook (typeMap.TypeData.Type, mem)) {
							flatLists[n] = GetObTempVar ();
							string rval = "null";
							if (IsReadOnly (typeMap, mem, isValueList)) rval = ob + "." + mem.Name;
							WriteLine (mem.TypeData.FullTypeName + " " + flatLists[n] + " = " + rval + ";");
						}
					}
					WriteLine (code + ";");
					WriteLine ("");
				}
				
				if (_format == SerializationFormat.Encoded && map.ElementMembers != null)
				{
					WriteLine ("Fixup fixup = new Fixup(" + ob + ", new XmlSerializationFixupCallback(" + GetFixupCallbackName (typeMap) + "), " + map.ElementMembers.Count + ");");
					WriteLine ("AddFixup (fixup);");
					WriteLine ("");
				}
	
				WriteLine ("while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) ");
				WriteLineInd ("{");
				WriteLine ("if (Reader.NodeType == System.Xml.XmlNodeType.Element) ");
				WriteLineInd ("{");
				
				first = true;
				foreach (XmlTypeMapElementInfo info in map.AllElementInfos)
				{
					if (info.IsTextElement || info.IsUnnamedAnyElement) continue;
					WriteLineInd ((first?"":"else ") + "if (Reader.LocalName == " + GetLiteral (info.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (info.Namespace) + " && !" + readFlag[info.Member.Index] + ") {");
	
					if (info.Member.GetType() == typeof (XmlTypeMapMemberList))
					{
						if (_format == SerializationFormat.Encoded && info.MultiReferenceType)
						{
							string list = GetObTempVar ();
							WriteLine ("object " + list + " = ReadReferencingElement (out fixup.Ids[" + info.Member.Index + "]);");
							WriteLineInd ("if (fixup.Ids[" + info.Member.Index + "] == null) {");	// Already read
							if (IsReadOnly (typeMap, info.Member, isValueList)) 
								WriteLine ("throw CreateReadOnlyCollectionException (" + GetLiteral(info.TypeData.FullTypeName) + ");");
							else 
								GenerateSetMemberValue (info.Member, ob, GetCast (info.Member.TypeData,list), isValueList);
							WriteLineUni ("}");
	
							if (!info.MappedType.TypeData.Type.IsArray)
							{
								WriteLineInd ("else {");
								if (IsReadOnly (typeMap, info.Member, isValueList)) 
									WriteLine (list + " = " + GenerateGetMemberValue (info.Member, ob, isValueList) + ";");
								else { 
									WriteLine (list + " = " + GenerateCreateList (info.MappedType.TypeData.Type) + ";");
									GenerateSetMemberValue (info.Member, ob, list, isValueList);
								}
								WriteLine ("AddFixup (new CollectionFixup (" + list + ", new XmlSerializationCollectionFixupCallback (" + GetFillListName(info.Member.TypeData) + "), fixup.Ids[" + info.Member.Index + "]));");
								WriteLine ("fixup.Ids[" + info.Member.Index + "] = null;");		// The member already has the value, no further fix needed.
								WriteLineUni ("}");
							}
						}
						else
						{
							if (!GenerateReadMemberHook (typeMap.TypeData.Type, info.Member)) {
								if (IsReadOnly (typeMap, info.Member, isValueList)) GenerateReadListElement (info.MappedType, GenerateGetMemberValue (info.Member, ob, isValueList), GetLiteral(info.IsNullable), false);
								else GenerateSetMemberValue (info.Member, ob, GenerateReadListElement (info.MappedType, null, GetLiteral(info.IsNullable), true), isValueList);
								GenerateEndHook ();
							}
						}
						WriteLine (readFlag[info.Member.Index] + " = true;");
					}
					else if (info.Member.GetType() == typeof (XmlTypeMapMemberFlatList))
					{
						XmlTypeMapMemberFlatList mem = (XmlTypeMapMemberFlatList)info.Member;
						if (!GenerateReadArrayMemberHook (typeMap.TypeData.Type, info.Member, indexes[mem.FlatArrayIndex])) {
							GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], GenerateReadObjectElement (info), !IsReadOnly (typeMap, info.Member, isValueList));
							GenerateEndHook ();
						}
						WriteLine (indexes[mem.FlatArrayIndex] + "++;");
					}
					else if (info.Member.GetType() == typeof (XmlTypeMapMemberAnyElement))
					{
						XmlTypeMapMemberAnyElement mem = (XmlTypeMapMemberAnyElement)info.Member;
						if (mem.TypeData.IsListType) { 
							if (!GenerateReadArrayMemberHook (typeMap.TypeData.Type, info.Member, indexes[mem.FlatArrayIndex])) {
								GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], "ReadXmlNode (false)", true);
								GenerateEndHook ();
							}
							WriteLine (indexes[mem.FlatArrayIndex] + "++;");
						}
						else {
							if (!GenerateReadMemberHook (typeMap.TypeData.Type, info.Member)) {
								GenerateSetMemberValue (mem, ob, "ReadXmlNode (false)", isValueList);
								GenerateEndHook ();
							}
						}
					}
					else if (info.Member.GetType() == typeof(XmlTypeMapMemberElement))
					{
						WriteLine (readFlag[info.Member.Index] + " = true;");
						if (_format == SerializationFormat.Encoded && info.MultiReferenceType) 
						{
							string val = GetObTempVar ();
							WriteLine ("object " + val + " = ReadReferencingElement (out fixup.Ids[" + info.Member.Index + "]);");
							WriteLineInd ("if (fixup.Ids[" + info.Member.Index + "] == null) {");	// already read
							GenerateSetMemberValue (info.Member, ob, val, isValueList);
							WriteLineUni ("}");
						}
						else if (!GenerateReadMemberHook (typeMap.TypeData.Type, info.Member)) {
							GenerateSetMemberValue (info.Member, ob, GenerateReadObjectElement (info), isValueList);
							GenerateEndHook ();
						}
					}
					else
						throw new InvalidOperationException ("Unknown member type");
	
					WriteLineUni ("}");
					first = false;
				}
				
				if (!first) WriteLineInd ("else {");
				
				if (map.DefaultAnyElementMember != null)
				{
					XmlTypeMapMemberAnyElement mem = map.DefaultAnyElementMember;
					if (mem.TypeData.IsListType) {
						if (!GenerateReadArrayMemberHook (typeMap.TypeData.Type, mem, indexes[mem.FlatArrayIndex])) {
							GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], "ReadXmlNode (false)", true);
							GenerateEndHook ();
						}
						WriteLine (indexes[mem.FlatArrayIndex] + "++;");
					}
					else if (! GenerateReadMemberHook (typeMap.TypeData.Type, mem)) {
						GenerateSetMemberValue (mem, ob, "ReadXmlNode (false)", isValueList);
						GenerateEndHook ();
					}
				}
				else {
					if (!GenerateReadHook (HookType.unknownElement, typeMap.TypeData.Type)) {
						WriteLine ("UnknownNode (" + ob + ");");
						GenerateEndHook ();
					}
				}
				
				if (!first) WriteLineUni ("}");
	
				WriteLineUni ("}");
				
				if (map.XmlTextCollector != null)
				{
					WriteLine ("else if (Reader.NodeType == System.Xml.XmlNodeType.Text)");
					WriteLineInd ("{");
	
					if (map.XmlTextCollector is XmlTypeMapMemberExpandable)
					{
						XmlTypeMapMemberExpandable mem = (XmlTypeMapMemberExpandable)map.XmlTextCollector;
						if (!GenerateReadArrayMemberHook (typeMap.TypeData.Type, map.XmlTextCollector, indexes[mem.FlatArrayIndex])) {
							string val = (mem.TypeData.ListItemType == typeof (string)) ? "Reader.ReadString()" : "ReadXmlNode (false)";
							GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], val, true);
							GenerateEndHook ();
						}
						WriteLine (indexes[mem.FlatArrayIndex] + "++;");
					}
					else if (!GenerateReadMemberHook (typeMap.TypeData.Type, map.XmlTextCollector))
					{
						XmlTypeMapMemberElement mem = (XmlTypeMapMemberElement) map.XmlTextCollector;
						XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) mem.ElementInfo [0];
						if (info.TypeData.Type == typeof (string))
							GenerateSetMemberValue (mem, ob, "ReadString (" + GenerateGetMemberValue (mem, ob, isValueList) + ")", isValueList);
						else
							GenerateSetMemberValue (mem, ob, GenerateGetValueFromXmlString ("Reader.ReadString()", info.TypeData, info.MappedType), isValueList);
						GenerateEndHook ();
					}
					WriteLineUni ("}");
				}
					
				WriteLine ("else");
				WriteLine ("\tUnknownNode(ob);");
				WriteLine ("");
	
				WriteLine ("Reader.MoveToContent();");
				WriteLineUni ("}");
	
				if (flatLists != null)
				{
					WriteLine ("");
					foreach (XmlTypeMapMemberExpandable mem in map.FlatLists)
					{
						if (MemberHasReadReplaceHook (typeMap.TypeData.Type, mem)) continue;
						
						string list = flatLists[mem.FlatArrayIndex];
						if (mem.TypeData.Type.IsArray)
							WriteLine (list + " = (" + mem.TypeData.FullTypeName + ") ShrinkArray (" + list + ", " + indexes[mem.FlatArrayIndex] + ", " + GetTypeOf(mem.TypeData.Type.GetElementType()) + ", true);");
						if (!IsReadOnly (typeMap, mem, isValueList))
							GenerateSetMemberValue (mem, ob, list, isValueList);
					}
				}
				GenerateEndHook ();
			}			

			WriteLine ("");
			WriteLine ("ReadEndElement();");
		}
		
		bool IsReadOnly (XmlTypeMapping map, XmlTypeMapMember member, bool isValueList)
		{
			if (isValueList) return false;
			else return member.IsReadOnly (map.TypeData.Type);
		}

		void GenerateSetMemberValue (XmlTypeMapMember member, string ob, string value, bool isValueList)
		{
			if (isValueList) WriteLine (ob + "[" + member.Index + "] = " + value + ";");
			else WriteLine (ob + "." + member.Name + " = " + value + ";");
		}

		object GenerateGetMemberValue (XmlTypeMapMember member, object ob, bool isValueList)
		{
			if (isValueList) return ob + "[" + member.Index + "]";
			else return ob + "." + member.Name;
		}

		string GenerateReadObjectElement (XmlTypeMapElementInfo elem)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					return "ReadXmlNode (true)";

				case SchemaTypes.Primitive:
				case SchemaTypes.Enum:
					return GenerateReadPrimitiveValue (elem);

				case SchemaTypes.Array:
					return GenerateReadListElement (elem.MappedType, null, GetLiteral(elem.IsNullable), true);

				case SchemaTypes.Class:
					return GetReadObjectName(elem.MappedType) + " (" + GetLiteral(elem.IsNullable) + ", true)";

				case SchemaTypes.XmlSerializable:
					return "ReadSerializable (new " + elem.TypeData.FullTypeName + " ())";

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}

		string GenerateReadPrimitiveValue (XmlTypeMapElementInfo elem)
		{
			if (elem.TypeData.Type == typeof (XmlQualifiedName)) {
				if (elem.IsNullable) return "ReadNullableQualifiedName ()";
				else return "ReadElementQualifiedName ()";
			}
			else if (elem.IsNullable)
				return GenerateGetValueFromXmlString ("ReadNullableString ()", elem.TypeData, elem.MappedType);
			else
				return GenerateGetValueFromXmlString ("Reader.ReadElementString ()", elem.TypeData, elem.MappedType);
		}
		
		string GenerateGetValueFromXmlString (string value, TypeData typeData, XmlTypeMapping typeMap)
		{
			if (typeData.SchemaType == SchemaTypes.Array)
				return GenerateReadListString (typeMap, value);
			else if (typeData.SchemaType == SchemaTypes.Enum)
				return GenerateGetEnumValue (typeMap, value);
			else if (typeData.Type == typeof (XmlQualifiedName))
				return "ToXmlQualifiedName (" + value + ")";
			else 
				return XmlCustomFormatter.GenerateFromXmlString (typeData, value);
		}
		
		string GenerateReadListElement (XmlTypeMapping typeMap, string list, string isNullable, bool canCreateInstance)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;

			if (list == null) {
				if (canCreateInstance) {
					list = GetObTempVar ();
					WriteLine (typeMap.TypeFullName + " " + list + " = null;");
					WriteLineInd ("if (!ReadNull()) {");
					WriteLine (list + " = " + GenerateCreateList (listType) + ";");
				}
				else throw new InvalidOperationException ("Cannot assign array to read only property: " + typeMap.TypeFullName);
			}
			else {
				WriteLineInd ("if (!ReadNull()) {");
			}

			WriteLineInd ("if (Reader.IsEmptyElement) {");
			WriteLine ("Reader.Skip();");
			if (listType.IsArray)
				WriteLine (list + " = (" + typeMap.TypeFullName + ") ShrinkArray (" + list + ", 0, " + GetTypeOf(listType.GetElementType()) + ", " + isNullable + ");");

			Unindent ();
			WriteLineInd ("} else {");

			string index = GetNumTempVar ();
			WriteLine ("int " + index + " = 0;");
			WriteLine ("Reader.ReadStartElement();");
			WriteLine ("Reader.MoveToContent();");
			WriteLine ("");

			WriteLine ("while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) ");
			WriteLineInd ("{");
			WriteLine ("if (Reader.NodeType == System.Xml.XmlNodeType.Element) ");
			WriteLineInd ("{");

			bool first = true;
			foreach (XmlTypeMapElementInfo elemInfo in listMap.ItemInfo)
			{
				WriteLineInd ((first?"":"else ") + "if (Reader.LocalName == " + GetLiteral (elemInfo.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (elemInfo.Namespace) + ") {");
				GenerateAddListValue (typeMap.TypeData, list, index, GenerateReadObjectElement (elemInfo), false);
				WriteLine (index + "++;");
				WriteLineUni ("}");
				first = false;
			}
			if (!first) WriteLine ("else UnknownNode (null);");
			else WriteLine ("UnknownNode (null);");
			
			WriteLineUni ("}");
			WriteLine ("else UnknownNode (null);");
			WriteLine ("");
			WriteLine ("Reader.MoveToContent();");
			WriteLineUni ("}");
			
			WriteLine ("ReadEndElement();");

			if (listType.IsArray)
				WriteLine (list + " = (" + typeMap.TypeFullName + ") ShrinkArray (" + list + ", " + index + ", " + GetTypeOf(listType.GetElementType()) + ", " + isNullable + ");");

			WriteLineUni ("}");
			WriteLineUni ("}");

			return list;
		}

		string GenerateReadListString (XmlTypeMapping typeMap, string values)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;
			string itemType = listType.GetElementType().FullName;
			
			string list = GetObTempVar ();
			WriteLine (itemType + "[] " + list + ";");
			
			string var = GetStrTempVar ();
			WriteLine ("string " + var + " = " + values + ".Trim();");
			WriteLineInd ("if (" + var + " != string.Empty) {");
			
			string valueArray = GetObTempVar ();
			WriteLine ("string[] " + valueArray + " = " + var + ".Split (' ');");
			
			WriteLine (list + " = new " + itemType + " [" + valueArray + ".Length];");

			XmlTypeMapElementInfo info = (XmlTypeMapElementInfo)listMap.ItemInfo[0];

			string index = GetNumTempVar ();
			WriteLineInd ("for (int " + index + " = 0; " + index + " < " + valueArray + ".Length; " + index + "++)");
			WriteLine (list + "[" + index + "] = " + GenerateGetValueFromXmlString (valueArray + "[" + index + "]", info.TypeData, info.MappedType) + ";");
			Unindent ();
			WriteLineUni ("}");
			WriteLine ("else");
			WriteLine ("\t" + list + " = new " + itemType + " [0];");
			
			return list;
		}

		void GenerateAddListValue (TypeData listType, string list, string index, string value, bool canCreateInstance)
		{
			Type type = listType.Type;
			if (type.IsArray)
			{
				WriteLine (list + " = (" + type.FullName + ") EnsureArrayIndex (" + list + ", " + index + ", " + GetTypeOf(type.GetElementType()) + ");");
				WriteLine (list + "[" + index + "] = " + value + ";");
			}
			else	// Must be IEnumerable
			{
				WriteLine ("if (" + list + " == null)");
				if (canCreateInstance) 
					WriteLine ("\t" + list + " = new " + listType.FullTypeName + "();");
				else 
					WriteLine ("\tthrow CreateReadOnlyCollectionException (" + GetLiteral (listType.FullTypeName) + ");");
				
				WriteLine (list + ".Add (" + value + ");");
			}
		}

		string GenerateCreateList (Type listType)
		{
			if (listType.IsArray)
				return "(" + listType.FullName + ") EnsureArrayIndex (null, 0, " + GetTypeOf(listType.GetElementType()) + ")";
			else
				return "new " + listType.FullName + "()";
		}
		
		void GenerateFillerCallbacks ()
		{
			foreach (TypeData td in _listsToFill)
			{
				string metName = GetFillListName (td);
				WriteLine ("void " + metName + " (object list, object source)");
				WriteLineInd ("{");
				WriteLine ("if (list == null) throw CreateReadOnlyCollectionException (" + GetLiteral (td.FullTypeName) + ");");
				WriteLine ("");

				WriteLine (td.FullTypeName + " dest = (" + td.FullTypeName + ") list;");
				WriteLine ("foreach (object ob in (IEnumerable)source)");
				WriteLine ("\t dest.Add (ob)");
				WriteLineUni ("}");
				WriteLine ("");
			}
		}

		void GenerateReadXmlNodeElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("return ReadXmlNode (false);");
		}

		void GenerateReadPrimitiveElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("XmlQualifiedName t = GetXsiType();");
			WriteLine ("if (t == null) t = new XmlQualifiedName (" + GetLiteral(typeMap.XmlType) + ", " + GetLiteral(typeMap.Namespace) + ");");
			WriteLine ("return ReadTypedPrimitive (t);");
		}

		void GenerateReadEnumElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("Reader.ReadStartElement ();");
			WriteLine (typeMap.TypeFullName + " res = " + GenerateGetEnumValue (typeMap, "Reader.ReadString()") + ";");
			WriteLine ("Reader.ReadEndElement ();");
			WriteLine ("return res;");
		}

		string GenerateGetEnumValue (XmlTypeMapping typeMap, string val)
		{
			return GetGetEnumValueName (typeMap) + " (" + val + ")";
		}
		
		void GenerateGetEnumValue (XmlTypeMapping typeMap)
		{
			string metName = GetGetEnumValueName (typeMap);
			EnumMap map = (EnumMap) typeMap.ObjectMap;

			if (map.IsFlags)
			{
				string switchMethod =  metName + "_Switch";
				WriteLine (typeMap.TypeFullName + " " + metName + " (string xmlName)");
				WriteLineInd ("{");
				WriteLine ("if (xmlName.Trim().IndexOf (' ') != -1)");
				WriteLineInd ("{");
				WriteLine (typeMap.TypeFullName + " sb = (" + typeMap.TypeFullName + ")0;");
				WriteLine ("string[] enumNames = xmlName.ToString().Split (' ');");
				WriteLine ("foreach (string name in enumNames)");
				WriteLineInd ("{");
				WriteLine ("if (name == string.Empty) continue;");
				WriteLine ("sb |= " + switchMethod + " (name); ");
				WriteLineUni ("}");
				WriteLine ("return sb;");
				WriteLineUni ("}");
				WriteLine ("else");
				WriteLine ("\treturn " + switchMethod + " (xmlName);");
				WriteLineUni ("}");
				metName = switchMethod;
			}

			WriteLine (typeMap.TypeFullName + " " + metName + " (string xmlName)");
			WriteLineInd ("{");
			GenerateGetSingleEnumValue (typeMap, "xmlName");
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateGetSingleEnumValue (XmlTypeMapping typeMap, string val)
		{
			EnumMap map = (EnumMap) typeMap.ObjectMap;
			WriteLine ("switch (" + val + ")");
			WriteLineInd ("{");
			foreach (EnumMap.EnumMapMember mem in map.Members)
			{
				WriteLine ("case " + GetLiteral (mem.XmlName) + ": return " + typeMap.TypeFullName + "." + mem.EnumName + ";");
			}
			WriteLineInd ("default:");
			WriteLineInd ("try {");
			WriteLine ("return (" + typeMap.TypeFullName + ") Int64.Parse (" + val + ");");
			WriteLineUni ("}");
			WriteLineInd ("catch {");
			WriteLine ("throw new InvalidOperationException (\"Invalid enumeration value: \" + " + val + ");");
			WriteLineUni ("}");
			Unindent ();
			WriteLineUni ("}");
		}
		
		void GenerateReadXmlSerializableElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("Reader.MoveToContent ();");
			WriteLine ("if (Reader.NodeType == XmlNodeType.Element)");
			WriteLineInd ("{");
			WriteLine ("if (Reader.LocalName == " + GetLiteral (typeMap.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (typeMap.Namespace) + ")");
			WriteLine ("\treturn ReadSerializable (new " + typeMap.TypeData.FullTypeName + "());");
			WriteLine ("else");
			WriteLine ("\tthrow CreateUnknownNodeException ();");
			WriteLineUni ("}");
			WriteLine ("else UnknownNode (null);");
			WriteLine ("");
			WriteLine ("return null;");
		}

		void GenerateReadInitCallbacks ()
		{
			WriteLine ("protected override void InitCallbacks ()");
			WriteLineInd ("{");

			if (_format == SerializationFormat.Encoded)
			{
				foreach (XmlTypeMapping map in _mapsToGenerate)  
				{
					if (map.TypeData.SchemaType == SchemaTypes.Class || map.TypeData.SchemaType == SchemaTypes.Enum)
					{
						WriteMetCall ("AddReadCallback", GetLiteral (map.XmlType), GetLiteral(map.Namespace), GetTypeOf(map.TypeData.Type), "new XmlSerializationReadCallback (" + GetReadObjectName (map) + ")");
					}
				}
			}
			
			WriteLineUni ("}");
			WriteLine ("");

			WriteLine ("protected override void InitIDs ()");
			WriteLine ("{");
			WriteLine ("}");
			WriteLine ("");
		}

		void GenerateFixupCallbacks ()
		{
			foreach (XmlTypeMapping map in _mapsToGenerate)  
			{
				if (map.TypeData.SchemaType == SchemaTypes.Class)
				{
					WriteLine ("void " + GetFixupCallbackName (map) + " (object obfixup)");
					WriteLineInd ("{");					
					WriteLine ("Fixup fixup = (Fixup)obfixup;");
					WriteLine (map.TypeFullName + " source = (" + map.TypeFullName + ") fixup.Source;");
					WriteLine ("string[] ids = fixup.Ids;");
					WriteLine ("");

					ClassMap cmap = (ClassMap)map.ObjectMap;
					ICollection members = cmap.ElementMembers;
					if (members != null) {
						foreach (XmlTypeMapMember member in members)
						{
							WriteLineInd ("if (ids[" + member.Index + "] != null)");
							GenerateSetMemberValue (member, "source", GetCast (member.TypeData, "GetTarget(ids[" + member.Index + "])"), false);
							Unindent ();
						}
					}
					WriteLineUni ("}");
					WriteLine ("");
				}
			}
		}
	
		#endregion
		
		#region Helper methods

		//*******************************************************
		// Helper methods
		//
		
		ArrayList _listsToFill = new ArrayList ();
		Hashtable _hooks;
		Hashtable _hookVariables;
		Stack _hookContexts;
		Stack _hookOpenHooks;
		
		class HookInfo {
			public HookType HookType;
			public Type Type;
			public string Member;
			public HookDir Direction;
		}

		void InitHooks ()
		{
			_hookContexts = new Stack ();
			_hookOpenHooks = new Stack ();
			_hookVariables = new Hashtable ();
			_hooks = new Hashtable ();
		}
		
		void PushHookContext ()
		{
			_hookContexts.Push (_hookVariables);
			_hookVariables = (Hashtable) _hookVariables.Clone ();
		}
		
		void PopHookContext ()
		{
			_hookVariables = (Hashtable) _hookContexts.Pop ();
		}
		
		void SetHookVar (string var, string value)
		{
			_hookVariables [var] = value;
		}

		bool GenerateReadHook (HookType hookType, Type type)
		{
			return GenerateHook (hookType, HookDir.Read, type, null);
		}

		bool GenerateWriteHook (HookType hookType, Type type)
		{
			return GenerateHook (hookType, HookDir.Write, type, null);
		}
		
		bool GenerateWriteMemberHook (Type type, XmlTypeMapMember member)
		{
			SetHookVar ("$MEMBER", member.Name);
			return GenerateHook (HookType.member, HookDir.Write, type, member.Name);
		}
		
		bool GenerateReadMemberHook (Type type, XmlTypeMapMember member)
		{
			SetHookVar ("$MEMBER", member.Name);
			return GenerateHook (HookType.member, HookDir.Read, type, member.Name);
		}
		
		bool GenerateReadArrayMemberHook (Type type, XmlTypeMapMember member, string index)
		{
			SetHookVar ("$INDEX", index);
			return GenerateReadMemberHook (type, member);
		}
	
		bool MemberHasReadReplaceHook (Type type, XmlTypeMapMember member)
		{
			return _config.GetHooks (HookType.member, HookDir.Read, HookAction.Replace, type, member.Name).Count > 0;
		}
		
		bool GenerateHook (HookType hookType, HookDir dir, Type type, string member)
		{
			GenerateHooks (hookType, dir, type, null, HookAction.InsertBefore);
			if (GenerateHooks (hookType, dir, type, null, HookAction.Replace))
			{
				GenerateHooks (hookType, dir, type, null, HookAction.InsertAfter);
				return true;
			}
			else
			{
				HookInfo hi = new HookInfo ();
				hi.HookType = hookType;
				hi.Type = type;
				hi.Member = member;
				hi.Direction = dir;
				_hookOpenHooks.Push (hi);
				return false;
			}
		}
		
		void GenerateEndHook ()
		{
			HookInfo hi = (HookInfo) _hookOpenHooks.Pop();
			GenerateHooks (hi.HookType, hi.Direction, hi.Type, hi.Member, HookAction.InsertAfter);
		}
		
		bool GenerateHooks (HookType hookType, HookDir dir, Type type, string member, HookAction action)
		{
			ArrayList hooks = _config.GetHooks (hookType, dir, action, type, null);
			if (hooks.Count == 0) return false;			
			foreach (Hook hook in hooks)
			{
				string code = hook.GetCode (action);
				foreach (DictionaryEntry de in _hookVariables)
					code = code.Replace ((string)de.Key, (string)de.Value);
				WriteMultilineCode (code);
			}
			return true;
		}
		
		string GetRootTypeName ()
		{
			if (_typeMap is XmlTypeMapping) return ((XmlTypeMapping)_typeMap).TypeFullName;
			else return "object[]";
		}

		string GetNumTempVar ()
		{
			return "n" + (_tempVarId++);
		}
		
		string GetObTempVar ()
		{
			return "o" + (_tempVarId++);
		}
		
		string GetStrTempVar ()
		{
			return "s" + (_tempVarId++);
		}
		
		string GetBoolTempVar ()
		{
			return "b" + (_tempVarId++);
		}
		
		string GetUniqueName (string uniqueGroup, object ob, string name)
		{
			Hashtable names = (Hashtable) _uniqueNames [uniqueGroup];
			if (names == null) {
				names = new Hashtable ();
				_uniqueNames [uniqueGroup] = names; 
			}
			
			string res = (string) names [ob];
			if (res != null) return res;

			foreach (string n in names.Values)
				if (n == name) return GetUniqueName (uniqueGroup, ob, name + (_methodId++));
				
			names [ob] = name;
			return name;
		}
		
		string GetWriteObjectName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("rw", typeMap, "WriteObject_" + typeMap.XmlType);
		}
		
		string GetReadObjectName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("rr", typeMap, "ReadObject_" + typeMap.XmlType);
		}
		
		string GetGetEnumValueName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("ge", typeMap, "GetEnumValue_" + typeMap.XmlType);
		}

		string GetWriteObjectCallbackName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("wc", typeMap, "WriteCallback_" + typeMap.XmlType);
		}
		
		string GetFixupCallbackName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("fc", typeMap, "FixupCallback_" + typeMap.XmlType);
		}
		
		string GetFillListName (TypeData td)
		{
			if (!_listsToFill.Contains (td)) _listsToFill.Add (td);
			return GetUniqueName ("fl", td, "Fill_" + td.TypeName);
		}
		
		string GetCast (TypeData td, TypeData tdval, string val)
		{
			if (td.FullTypeName == tdval.FullTypeName) return val;
			else return GetCast (td, val);
		}

		string GetCast (TypeData td, string val)
		{
			return "((" + td.FullTypeName + ") " + val + ")";
		}

		string GetTypeOf (TypeData td)
		{
			return "typeof(" + td.FullTypeName + ")";
		}
		
		string GetTypeOf (Type td)
		{
			return "typeof(" + td.FullName + ")";
		}
		
		string GetString (string str)
		{
			return "\"" + str + "\"";
		}
		
		string GetLiteral (object ob)
		{
			if (ob == null) return "null";
			if (ob is string) return "\"" + ob.ToString().Replace("\"","\"\"") + "\"";
			if (ob is bool) return ((bool)ob) ? "true" : "false";
			if (ob is XmlQualifiedName) {
				XmlQualifiedName qn = (XmlQualifiedName)ob;
				return "new XmlQualifiedName (" + GetLiteral(qn.Name) + "," + GetLiteral(qn.Namespace) + ")";
			}
			else return ob.ToString ();
		}
		
		void WriteLineInd (string code)
		{
			WriteLine (code);
			_indent++;
		}
		
		void WriteLineUni (string code)
		{
			if (_indent > 0) _indent--;
			WriteLine (code);
		}
		
		void WriteLine (string code)
		{
			if (code != "")	_writer.Write (new String ('\t',_indent));
			_writer.WriteLine (code);
		}
		
		void WriteMultilineCode (string code)
		{
			string tabs = new string ('\t',_indent);
			code = code.Replace ("\r","");
			code = code.Replace ("\t","");
			while (code.StartsWith ("\n")) code = code.Substring (1);
			while (code.EndsWith ("\n")) code = code.Substring (0, code.Length - 1);
			code = code.Replace ("\n", "\n" + tabs);
			WriteLine (code);
		}
		
		string Params (params string[] pars)
		{
			string res = "";
			foreach (string p in pars)
			{
				if (res != "") res += ", ";
				res += p;
			}
			return res;
		}
		
		void WriteMetCall (string method, params string[] pars)
		{
			WriteLine (method + " (" + Params (pars) + ");");
		}
		
		void Indent ()
		{
			_indent++;
		}

		void Unindent ()
		{
			_indent--;
		}

		#endregion

	}
}
