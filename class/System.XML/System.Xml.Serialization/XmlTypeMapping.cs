//
// XmlTypeMapping.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Xml;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlTypeMapping.
	/// </summary>
	public class XmlTypeMapping : XmlMapping
	{
		private string elementName;
		private string ns;
		private string typeFullName;
		private string typeName;

		internal XmlTypeMapping(string elementName, string ns, string typeFullName, string typeName)
		{
			this.elementName = elementName;
			this.ns = ns;
			this.typeFullName = typeFullName;
			this.typeName = typeName;
		}

		public string ElementName  
		{
			get { return elementName; }
		}
		public string Namespace  
		{
			get { return ns; }
		}
		public string TypeFullName  
		{
			get { return typeFullName; }
		}
		public string TypeName  
		{
			get { return typeName; }
		}
	}
}
