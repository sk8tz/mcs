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
		private string typeFullName;
	
		public string TypeFullName {
			get { 
				return typeFullName; 
			}
		}
	}
}
