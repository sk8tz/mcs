// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaGroupRef.
	/// </summary>
	public class XmlSchemaGroupRef : XmlSchemaParticle
	{
		private XmlSchemaGroupBase particle;
		private XmlQualifiedName refName;
		private static string xmlname = "group";

		public XmlSchemaGroupRef()
		{
			refName = XmlQualifiedName.Empty;
		}
		[XmlIgnore]
		public XmlSchemaGroupBase Particle 
		{
			get{ return particle; }
		}
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; } 
			set{ refName = value; }
		}
		/// <remarks>
		/// 1. RefName must be present
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(refName == null || refName.IsEmpty)
			{
				error(h,"ref must be present");
			}
			if(this.Id != null && !XmlSchemaUtil.CheckID(Id))
				error(h, "id must be a valid ID");
			
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		//	<group 
		//		 id = ID 
		//		 ref = QName
		//		 minOccurs = ? : 1
		//		 maxOccurs = ? : 1>
		//		 Content: (annotation?)
		//	</group>
		internal static XmlSchemaGroupRef Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaGroupRef groupref = new XmlSchemaGroupRef();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaGroup.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			groupref.LineNumber = reader.LineNumber;
			groupref.LinePosition = reader.LinePosition;
			groupref.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					groupref.Id = reader.Value;
				}
				else if(reader.Name == "ref")
				{
					Exception innerex;
					groupref.refName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for ref attribute",innerex);
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						groupref.MaxOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for maxOccurs",e);
					}
				}
				else if(reader.Name == "minOccurs")
				{
					try
					{
						groupref.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs", e);
					}
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for group",null);
				}
				else
				{
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return groupref;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaGroupRef.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						groupref.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return groupref;
		}
	}
}
