// 
// System.Web.Services.Description.ExtensionManager.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
//

using System.Reflection;
using System.Collections;
using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description 
{
	internal abstract class ExtensionManager 
	{
		static Hashtable extensionsByName;
		static Hashtable extensionsByType;

		static ExtensionManager ()
		{
			extensionsByName = new Hashtable ();
			extensionsByType = new Hashtable ();

			RegisterExtensionType (typeof (HttpAddressBinding));
			RegisterExtensionType (typeof (HttpBinding));
			RegisterExtensionType (typeof (HttpOperationBinding));
			RegisterExtensionType (typeof (HttpUrlEncodedBinding));
			RegisterExtensionType (typeof (HttpUrlReplacementBinding));
			RegisterExtensionType (typeof (MimeContentBinding));
			RegisterExtensionType (typeof (MimeMultipartRelatedBinding));
			RegisterExtensionType (typeof (MimeTextBinding));
			RegisterExtensionType (typeof (MimeXmlBinding));
			RegisterExtensionType (typeof (SoapAddressBinding));
			RegisterExtensionType (typeof (SoapBinding));
			RegisterExtensionType (typeof (SoapBodyBinding));
			RegisterExtensionType (typeof (SoapFaultBinding));
			RegisterExtensionType (typeof (SoapHeaderBinding));
			RegisterExtensionType (typeof (SoapHeaderFaultBinding));
			RegisterExtensionType (typeof (SoapOperationBinding));
		}
	
		public static void RegisterExtensionType (Type type)
		{
			ExtensionInfo ext = new ExtensionInfo();
			ext.Type = type;
			
			object[] ats = type.GetCustomAttributes (typeof(XmlFormatExtensionPrefixAttribute), true);
			if (ats.Length > 0)
			{
				XmlFormatExtensionPrefixAttribute at = (XmlFormatExtensionPrefixAttribute)ats[0];
				ext.Prefix = at.Prefix;
				ext.Namespace = at.Namespace;
			}
			
			ats = type.GetCustomAttributes (typeof(XmlFormatExtensionAttribute), true);
			if (ats.Length > 0)
			{
				XmlFormatExtensionAttribute at = (XmlFormatExtensionAttribute)ats[0];
				ext.ElementName = at.ElementName;
				if (at.Namespace != null) ext.Namespace = at.Namespace;
			}
			
			XmlRootAttribute root = new XmlRootAttribute ();
			root.ElementName = ext.ElementName;
			if (ext.Namespace != null) root.Namespace = ext.Namespace;

			XmlReflectionImporter ri = new XmlReflectionImporter ();
			XmlTypeMapping map = ri.ImportTypeMapping (type, root);
			
			// TODO: use array method to create the serializers
			ext.Serializer = new XmlSerializer (map);

			if (ext.ElementName == null) throw new InvalidOperationException ("XmlFormatExtensionAttribute must be applied to type " + type);
			extensionsByName.Add (ext.Namespace + " " + ext.ElementName, ext);
			extensionsByType.Add (type, ext);
		}
		
		public static ExtensionInfo GetFormatExtensionInfo (string elementName, string namesp)
		{
			return (ExtensionInfo) extensionsByName [namesp + " " + elementName];
		}
		
		public static ExtensionInfo GetFormatExtensionInfo (Type extType)
		{
			return (ExtensionInfo) extensionsByType [extType];
		}
		
		public static ICollection GetFormatExtensions ()
		{
			return extensionsByName.Values;
		}

		public static ServiceDescriptionFormatExtensionCollection GetExtensionPoint (object ob)
		{
			Type type = ob.GetType ();
			object[] ats = type.GetCustomAttributes (typeof(XmlFormatExtensionPointAttribute), true);
			if (ats.Length == 0) return null;

			XmlFormatExtensionPointAttribute at = (XmlFormatExtensionPointAttribute)ats[0];
			
			PropertyInfo prop = type.GetProperty (at.MemberName);
			if (prop != null)
				return prop.GetValue (ob, null) as ServiceDescriptionFormatExtensionCollection;
			else {
				FieldInfo field = type.GetField (at.MemberName);
				if (field != null)
					return field.GetValue (ob) as ServiceDescriptionFormatExtensionCollection;
				else
					throw new InvalidOperationException ("XmlFormatExtensionPointAttribute: Member " + at.MemberName + " not found");
			}
		}
	}
	
	internal class ExtensionInfo
	{
		public string _prefix;
		public string _namespace;
		public string _elementName;
		public Type _type;
		public XmlSerializer _serializer;

		public string Prefix
		{
			get { return _prefix; }
			set { _prefix = value; }
		}
		
		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}
		
		public string ElementName
		{
			get { return _elementName; }
			set { _elementName = value; }
		}
		
		public Type Type
		{
			get { return _type; }
			set { _type = value; }
		}
		
		public XmlSerializer Serializer
		{
			get { return _serializer; }
			set { _serializer = value; }
		}		
	}
}
