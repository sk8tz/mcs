// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaWhiteSpaceFacet.
	/// </summary>
	public class XmlSchemaWhiteSpaceFacet : XmlSchemaFacet
	{
		private static string xmlname = "whiteSpace";

		public XmlSchemaWhiteSpaceFacet()
		{
		}
		//	<whiteSpace
		//	fixed = boolean : false
		//	id = ID
		//	value = (collapse | preserve | replace)
		//	{any attributes with non-schema namespace . . .}>
		//	Content: (annotation?)
		//	</whiteSpace>
		internal static XmlSchemaWhiteSpaceFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaWhiteSpaceFacet ws = new XmlSchemaWhiteSpaceFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaWhiteSpaceFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			ws.LineNumber = reader.LineNumber;
			ws.LinePosition = reader.LinePosition;
			ws.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					ws.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					ws.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					ws.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						ws.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						ws.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return ws;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaWhiteSpaceFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						ws.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return ws;
		}


	}
}
