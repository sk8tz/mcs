// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeRestriction.
	/// </summary>
	public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType baseType;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaObjectCollection facets;
		public XmlSchemaSimpleTypeRestriction()
		{
			facets = new XmlSchemaObjectCollection();
		}

		[XmlElement("simpleType",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleType BaseType 
		{
			get{ return  baseType; } 
			set{ baseType = value; }
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; } 
			set{ baseTypeName = value; }
		}

		[XmlElement("minExclusive",typeof(XmlSchemaMinExclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("minInclusive",typeof(XmlSchemaMinInclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")] 
		[XmlElement("maxExclusive",typeof(XmlSchemaMaxExclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("maxInclusive",typeof(XmlSchemaMaxInclusiveFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("totalDigits",typeof(XmlSchemaTotalDigitsFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("fractionDigits",typeof(XmlSchemaFractionDigitsFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("length",typeof(XmlSchemaLengthFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("minLength",typeof(XmlSchemaMinLengthFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("maxLength",typeof(XmlSchemaMaxLengthFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("enumeration",typeof(XmlSchemaEnumerationFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("whiteSpace",typeof(XmlSchemaWhiteSpaceFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("pattern",typeof(XmlSchemaPatternFacet),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Facets 
		{
			get{ return facets; }
		}
	}
}
