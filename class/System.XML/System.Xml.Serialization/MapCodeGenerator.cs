// 
// System.Xml.Serialization.MapCodeGenerator 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc., 2003
//

using System.CodeDom;
using System.Collections;
using System.Xml.Schema;

namespace System.Xml.Serialization {
	internal class MapCodeGenerator {

		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		CodeAttributeDeclarationCollection includeMetadata;
		bool encodedFormat;

		Hashtable exportedMaps = new Hashtable ();

		public MapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
		}

		#region Code generation methods

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			CodeTypeDeclaration dummyClass = new CodeTypeDeclaration ();
			ExportMembersMapCode (dummyClass, (ClassMap)xmlMembersMapping.ObjectMap, xmlMembersMapping.Namespace, null);
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			ExportMapCode (xmlTypeMapping);
		}

		void ExportMapCode (XmlTypeMapping map)
		{
			switch (map.TypeData.SchemaType)
			{
				case SchemaTypes.Enum:
					ExportEnumCode (map);
					break;

				case SchemaTypes.Array:
					ExportArrayCode (map);
					break;

				case SchemaTypes.Class:
					ExportClassCode (map);
					break;

				case SchemaTypes.XmlSerializable:
				case SchemaTypes.XmlNode:
				case SchemaTypes.Primitive:
					// Ignore
					break;
			}
		}

		void ExportClassCode (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			CodeTypeDeclaration codeClass = new CodeTypeDeclaration (map.TypeData.TypeName);
			AddCodeType (codeClass, map.Documentation);
			codeClass.Attributes = MemberAttributes.Public;

			GenerateClass (map, codeClass);

			ExportMembersMapCode (codeClass, (ClassMap)map.ObjectMap, map.XmlTypeNamespace, map.BaseMap);

			if (map.BaseMap != null)
			{
				CodeTypeReference ctr = new CodeTypeReference (map.BaseMap.TypeData.FullTypeName);
				codeClass.BaseTypes.Add (ctr);
				ExportMapCode (map.BaseMap);
			}

			ExportDerivedTypes (map, codeClass);
		}
		
		void ExportDerivedTypes (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
			foreach (XmlTypeMapping tm in map.DerivedTypes)
			{
				GenerateClassInclude (codeClass, tm);
				ExportMapCode (tm);
				ExportDerivedTypes (tm, codeClass);
			}
		}

		void ExportMembersMapCode (CodeTypeDeclaration codeClass, ClassMap map, string defaultNamespace, XmlTypeMapping baseMap)
		{
			ICollection members = map.ElementMembers;
				
			if (members != null)
			{
				foreach (XmlTypeMapMemberElement member in members)
				{
					if (baseMap != null && DefinedInBaseMap (baseMap, member)) continue;

					Type memType = member.GetType();
					if (memType == typeof(XmlTypeMapMemberList))
					{
						AddArrayElementFieldMember (codeClass, (XmlTypeMapMemberList) member, defaultNamespace);
					}
					else if (memType == typeof(XmlTypeMapMemberFlatList))
					{
						AddElementFieldMember (codeClass, member, defaultNamespace);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						AddAnyElementFieldMember (codeClass, member, defaultNamespace);
					}
					else if (memType == typeof(XmlTypeMapMemberElement))
					{
						AddElementFieldMember (codeClass, member, defaultNamespace);
					}
					else
					{
						throw new InvalidOperationException ("Member type " + memType + " not supported");
					}
				}
			}

			// Write attributes

			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes) {
					if (baseMap != null && DefinedInBaseMap (baseMap, attr)) continue;
					AddAttributeFieldMember (codeClass, attr, defaultNamespace);
				}
			}

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null)
			{
				CodeMemberField codeField = new CodeMemberField (anyAttrMember.TypeData.FullTypeName, anyAttrMember.Name);
				AddComments (codeField, anyAttrMember.Documentation);
				codeField.Attributes = MemberAttributes.Public;
				GenerateAnyAttribute (codeField);
				codeClass.Members.Add (codeField);
			}
		}

		CodeMemberField CreateFieldMember (TypeData type, string name, object defaultValue, string comments)
		{
			CodeMemberField codeField = new CodeMemberField (type.FullTypeName, name);
			codeField.Attributes = MemberAttributes.Public;
			AddComments (codeField, comments);

			if (defaultValue != System.DBNull.Value)
				GenerateDefaultAttribute (codeField, type, defaultValue);

			return codeField;
		}

		void AddAttributeFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberAttribute attinfo, string defaultNamespace)
		{
			CodeMemberField codeField = CreateFieldMember (attinfo.TypeData, attinfo.Name, attinfo.DefaultValue, attinfo.Documentation);
			codeClass.Members.Add (codeField);

			CodeAttributeDeclarationCollection attributes = codeField.CustomAttributes;
			if (attributes == null) attributes = new CodeAttributeDeclarationCollection ();
			
			GenerateAttributeMember (attributes, attinfo, defaultNamespace, false);
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;

			if (attinfo.MappedType != null)
				ExportMapCode (attinfo.MappedType);

			if (attinfo.TypeData.IsValueType && attinfo.IsOptionalValueType)
			{
				codeField = new CodeMemberField (typeof(bool), attinfo.Name + "Specified");
				codeField.Attributes = MemberAttributes.Public;
				codeClass.Members.Add (codeField);
				GenerateSpecifierMember (codeField);
			}
		}
		
		public void AddAttributeMemberAttributes (XmlTypeMapMemberAttribute attinfo, string defaultNamespace, CodeAttributeDeclarationCollection attributes, bool forceUseMemberName)
		{
			GenerateAttributeMember (attributes, attinfo, defaultNamespace, forceUseMemberName);
		}

		void AddElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			CodeMemberField codeField = CreateFieldMember (member.TypeData, member.Name, member.DefaultValue, member.Documentation);
			codeClass.Members.Add (codeField);
			
			CodeAttributeDeclarationCollection attributes = codeField.CustomAttributes;
			if (attributes == null) attributes = new CodeAttributeDeclarationCollection ();
			
			AddElementMemberAttributes (member, defaultNamespace, attributes, false);
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;
			
			if (member.TypeData.IsValueType && member.IsOptionalValueType)
			{
				codeField = new CodeMemberField (typeof(bool), member.Name + "Specified");
				codeField.Attributes = MemberAttributes.Public;
				codeClass.Members.Add (codeField);
				GenerateSpecifierMember (codeField);
			}
		}

		public void AddElementMemberAttributes (XmlTypeMapMemberElement member, string defaultNamespace, CodeAttributeDeclarationCollection attributes, bool forceUseMemberName)
		{
			TypeData defaultType = member.TypeData;
			bool addAlwaysAttr = false;
			
			if (member is XmlTypeMapMemberFlatList)
			{
				defaultType = defaultType.ListItemTypeData;
				addAlwaysAttr = true;
			}
			
			foreach (XmlTypeMapElementInfo einfo in member.ElementInfo)
			{
				if (ExportExtraElementAttributes (attributes, einfo, defaultNamespace, defaultType))
					continue;

				GenerateElementInfoMember (attributes, member, einfo, defaultType, defaultNamespace, addAlwaysAttr, forceUseMemberName);
				if (einfo.MappedType != null) ExportMapCode (einfo.MappedType);
			}

			GenerateElementMember (attributes, member);
		}

		void AddAnyElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			CodeMemberField codeField = CreateFieldMember (member.TypeData, member.Name, member.DefaultValue, member.Documentation);
			codeClass.Members.Add (codeField);

			CodeAttributeDeclarationCollection attributes = new CodeAttributeDeclarationCollection ();
			foreach (XmlTypeMapElementInfo einfo in member.ElementInfo)
				ExportExtraElementAttributes (attributes, einfo, defaultNamespace, einfo.TypeData);
				
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;
		}

		bool DefinedInBaseMap (XmlTypeMapping map, XmlTypeMapMember member)
		{
			if (((ClassMap)map.ObjectMap).FindMember (member.Name) != null)
				return true;
			else if (map.BaseMap != null)
				return DefinedInBaseMap (map.BaseMap, member);
			else
				return false;
		}

		void AddArrayElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberList member, string defaultNamespace)
		{
			CodeMemberField codeField = new CodeMemberField (member.TypeData.FullTypeName, member.Name);
			AddComments (codeField, member.Documentation);
			codeField.Attributes = MemberAttributes.Public;
			codeClass.Members.Add (codeField);
			
			CodeAttributeDeclarationCollection attributes = new CodeAttributeDeclarationCollection ();
			AddArrayAttributes (attributes, member, defaultNamespace, false);

			ListMap listMap = (ListMap) member.ListTypeMapping.ObjectMap;
			AddArrayItemAttributes (attributes, listMap, member.TypeData.ListItemTypeData, defaultNamespace, 0);
			
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;
		}

		public void AddArrayAttributes (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, string defaultNamespace, bool forceUseMemberName)
		{
			GenerateArrayElement (attributes, member, defaultNamespace, forceUseMemberName);
		}

		public void AddArrayItemAttributes (CodeAttributeDeclarationCollection attributes, ListMap listMap, TypeData type, string defaultNamespace, int nestingLevel)
		{
			foreach (XmlTypeMapElementInfo ainfo in listMap.ItemInfo)
			{
				string defaultName;
				if (ainfo.MappedType != null) defaultName = ainfo.MappedType.ElementName;
				else defaultName = ainfo.TypeData.XmlType;

				GenerateArrayItemAttributes (attributes, listMap, type, ainfo, defaultName, defaultNamespace, nestingLevel);
				if (ainfo.MappedType != null) ExportMapCode (ainfo.MappedType);
			}

			if (listMap.IsMultiArray)
			{
				XmlTypeMapping nmap = listMap.NestedArrayMapping;
				AddArrayItemAttributes (attributes, (ListMap) nmap.ObjectMap, nmap.TypeData.ListItemTypeData, defaultNamespace, nestingLevel + 1);
			}
		}
		
		void ExportArrayCode (XmlTypeMapping map)
		{
			ListMap listMap = (ListMap) map.ObjectMap;
			foreach (XmlTypeMapElementInfo ainfo in listMap.ItemInfo)
			{
				if (ainfo.MappedType != null)
					ExportMapCode (ainfo.MappedType);
			}
		}

		bool ExportExtraElementAttributes (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, string defaultNamespace, TypeData defaultType)
		{
			if (einfo.IsTextElement) {
				GenerateTextElementAttribute (attributes, einfo, defaultType);
				return true;
			}
			else if (einfo.IsUnnamedAnyElement) {
				GenerateUnnamedAnyElementAttribute (attributes, einfo, defaultNamespace);
				return true;
			}
			return false;
		}

		void ExportEnumCode (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			CodeTypeDeclaration codeEnum = new CodeTypeDeclaration (map.TypeData.TypeName);
			codeEnum.Attributes = MemberAttributes.Public;
			codeEnum.IsEnum = true;
			AddCodeType (codeEnum, map.Documentation);

			GenerateEnum (map, codeEnum);
			EnumMap emap = (EnumMap) map.ObjectMap;
			
			if (emap.IsFlags)
				codeEnum.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Flags"));

			int flag = 1;
			foreach (EnumMap.EnumMapMember emem in emap.Members)
			{
				CodeMemberField codeField = new CodeMemberField ("", emem.EnumName);
				if (emap.IsFlags) {
					codeField.InitExpression = new CodePrimitiveExpression (flag);
					flag *= 2;
				}
				
				AddComments (codeField, emem.Documentation);

				GenerateEnumItem (codeField, emem);
				codeEnum.Members.Add (codeField);
			}
		}

		#endregion
		
		#region Helper methods
		
		bool IsMapExported (XmlTypeMapping map)
		{
			if (exportedMaps.Contains (map)) return true;
			if (map.TypeData.Type == typeof(object)) return true;
			if (!map.IncludeInSchema) return true;
			return false;
		}

		void SetMapExported (XmlTypeMapping map)
		{
			exportedMaps.Add (map,map);
		}

		public static void AddCustomAttribute (CodeTypeMember ctm, CodeAttributeDeclaration att, bool addIfNoParams)
		{
			if (att.Arguments.Count == 0 && !addIfNoParams) return;
			
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (att);
		}

		public static void AddCustomAttribute (CodeTypeMember ctm, string name, params CodeAttributeArgument[] args)
		{
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (new CodeAttributeDeclaration (name, args));
		}

		public static CodeAttributeArgument GetArg (string name, object value)
		{
			return new CodeAttributeArgument (name, new CodePrimitiveExpression(value));
		}

		public static CodeAttributeArgument GetArg (object value)
		{
			return new CodeAttributeArgument (new CodePrimitiveExpression(value));
		}

		public static CodeAttributeArgument GetTypeArg (string name, string typeName)
		{
			return new CodeAttributeArgument (name, new CodeTypeOfExpression(typeName));
		}

		public static CodeAttributeArgument GetEnumArg (string name, string enumType, string enumValue)
		{
			return new CodeAttributeArgument (name, new CodeFieldReferenceExpression (new CodeTypeReferenceExpression(enumType), enumValue));
		}
		
		public static void AddComments (CodeTypeMember member, string comments)
		{
			if (comments == null || comments == "") member.Comments.Add (new CodeCommentStatement ("<remarks/>", true));
			else member.Comments.Add (new CodeCommentStatement ("<remarks>\n" + comments + "\n</remarks>", true));
		}

		void AddCodeType (CodeTypeDeclaration type, string comments)
		{
			AddComments (type, comments);
			codeNamespace.Types.Add (type);
		}
		
		#endregion
		
		#region Overridable methods
		
		protected virtual void GenerateClass (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
		}
		
		protected virtual void GenerateClassInclude (CodeTypeDeclaration codeClass, XmlTypeMapping map)
		{
		}
		
		protected virtual void GenerateAnyAttribute (CodeMemberField codeField)
		{
		}
		
		protected virtual void GenerateDefaultAttribute (CodeMemberField codeField, TypeData typeData, object defaultValue)
		{
			if (typeData.Type == null)
			{
				// It must be an enumeration defined in the schema.
				if (typeData.SchemaType != SchemaTypes.Enum) 
					throw new InvalidOperationException ("Type " + typeData.TypeName + " not supported");
					
				CodeFieldReferenceExpression fref = new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typeData.FullTypeName), defaultValue.ToString());
				CodeAttributeArgument arg = new CodeAttributeArgument (fref);
				AddCustomAttribute (codeField, "System.ComponentModel.DefaultValue", arg);
				codeField.InitExpression = fref;
			}
			else
			{
				AddCustomAttribute (codeField, "System.ComponentModel.DefaultValue", GetArg (defaultValue));
				codeField.InitExpression = new CodePrimitiveExpression (defaultValue);
			}
		}
		
		protected virtual void GenerateAttributeMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberAttribute attinfo, string defaultNamespace, bool forceUseMemberName)
		{
		}
		
		protected virtual void GenerateElementInfoMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, XmlTypeMapElementInfo einfo, TypeData defaultType, string defaultNamespace, bool addAlwaysAttr, bool forceUseMemberName)
		{
		}
		
		protected virtual void GenerateElementMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member)
		{
		}
		
		protected virtual void GenerateArrayElement (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, string defaultNamespace, bool forceUseMemberName)
		{
		}
		
		protected virtual void GenerateArrayItemAttributes (CodeAttributeDeclarationCollection attributes, ListMap listMap, TypeData type, XmlTypeMapElementInfo ainfo, string defaultName, string defaultNamespace, int nestingLevel)
		{
		}

		protected virtual void GenerateTextElementAttribute (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, TypeData defaultType)
		{
		}
		
		protected virtual void GenerateUnnamedAnyElementAttribute (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, string defaultNamespace)
		{
		}
		
		protected virtual void GenerateEnum (XmlTypeMapping map, CodeTypeDeclaration codeEnum)
		{
		}		
		
		protected virtual void GenerateEnumItem (CodeMemberField codeField, EnumMap.EnumMapMember emem)
		{
		}

		protected virtual void GenerateSpecifierMember (CodeMemberField codeField)
		{
		}

		#endregion
	}
}
