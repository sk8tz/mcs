//
// XmlArrayAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml.Schema;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlArrayAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
	| AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class XmlArrayAttribute : Attribute
	{
		private string elementName;
		private XmlSchemaForm form;
		private bool isNullable;
		private string ns;

		public XmlArrayAttribute()
		{
		}

		public XmlArrayAttribute(string elementName)
		{
			ElementName = elementName;
		}

		public string ElementName 
		{
			get
			{
				return elementName;
			} 
			set
			{
				elementName = value;
			}
		}
		public XmlSchemaForm Form 
		{
			get
			{
				return form;
			} 
			set
			{
				form = value;
			}
		}
		public bool IsNullable 
		{
			get
			{
				return isNullable;
			} 
			set
			{
				isNullable = value;
			}
		}
		public string Namespace 
		{
			get
			{
				return ns;
			} 
			set
			{
				ns = value;
			}
		}
	}
}
