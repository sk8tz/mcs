// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaFractionDigitsFacet.
	/// </summary>
	public class XmlSchemaFractionDigitsFacet : XmlSchemaNumericFacet
	{
		private static string xmlname = "fractionDigits";

		public XmlSchemaFractionDigitsFacet()
		{
		}

		//<fractionDigits
		//  fixed = boolean : false
		//  id = ID
		//  value = nonNegativeInteger
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</fractionDigits>
		internal static XmlSchemaFractionDigitsFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaFractionDigitsFacet fraction = new XmlSchemaFractionDigitsFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaFractionDigitsFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			fraction.LineNumber = reader.LineNumber;
			fraction.LinePosition = reader.LinePosition;
			fraction.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					fraction.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					fraction.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					fraction.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						fraction.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						fraction.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return fraction;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaFractionDigitsFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						fraction.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return fraction;
		}	


	}
}
