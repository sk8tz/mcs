// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaType.
	/// </summary>
	public class XmlSchemaType : XmlSchemaAnnotated
	{
		private object baseSchemaType;
		private XmlSchemaDatatype datatype;
		private XmlSchemaDerivationMethod derivedBy;
		private XmlSchemaDerivationMethod final;
		private XmlSchemaDerivationMethod finalResolved;
		private bool isMixed;
		private string name;
		private XmlQualifiedName qName;

		public XmlSchemaType()
		{
			final = XmlSchemaDerivationMethod.None;
		}
		[XmlIgnore]
		public object BaseSchemaType 
		{
			get{ return  baseSchemaType; }
		}
		[XmlIgnore]
		public XmlSchemaDatatype Datatype 
		{
			get{ return datatype; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod DerivedBy 
		{
			get{ return derivedBy; }
		}
		[DefaultValue(XmlSchemaDerivationMethod.None)]
		[XmlAttribute]
		public XmlSchemaDerivationMethod Final 
		{
			get{ return  final; }
			set{ final = value; }
		}
		[XmlIgnore]
		public XmlSchemaDerivationMethod FinalResolved 
		{
			get{ return finalResolved; }
		}
		[XmlIgnore]
		public virtual bool IsMixed 
		{  
			get{ return  isMixed; }
			set{ isMixed = value; } 
		}
		[XmlAttribute]
		public string Name 
		{
			get{ return name; }
			set{ name = value; }
		}
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qName; }
		}
	}
}
