// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaNotation.
	/// </summary>
	public class XmlSchemaNotation : XmlSchemaAnnotated
	{
		private string name;
		private string pub;
		private string system;

		public XmlSchemaNotation()
		{
		}
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		[System.Xml.Serialization.XmlAttribute("public")]
		public string Public 
		{
			get{ return  pub; } 
			set{ pub = value; }
		}
		[System.Xml.Serialization.XmlAttribute("system")]
		public string System 
		{
			get{ return  system; } 
			set{ system = value; }
		}
	}
}
