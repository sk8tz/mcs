// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.ComponentModel;
using System.Xml.Serialization;
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnyAttribute.
	/// </summary>
	public class XmlSchemaAnyAttribute : XmlSchemaAnnotated
	{
		private string nameSpace;
		private XmlSchemaContentProcessing processing;
		private static string xmlname = "anyAttribute";
		private XsdWildcard wildcard;

		public XmlSchemaAnyAttribute()
		{
			wildcard = new XsdWildcard (this);
		}

		[System.Xml.Serialization.XmlAttribute("namespace")]
		public string Namespace 
		{ 
			get{ return nameSpace; } 
			set{ nameSpace = value; } 
		}
		
		[DefaultValue(XmlSchemaContentProcessing.None)]
		[System.Xml.Serialization.XmlAttribute("processContents")]
		public XmlSchemaContentProcessing ProcessContents 
		{ 
			get{ return processing; } 
			set{ processing = value; }
		}

		// Internal
		internal bool HasValueAny {
			get { return wildcard.HasValueAny; }
		}

		internal bool HasValueLocal {
			get { return wildcard.HasValueLocal; }
		}

		internal bool HasValueOther {
			get { return wildcard.HasValueOther; }
		}

		internal bool HasValueTargetNamespace {
			get { return wildcard.HasValueTargetNamespace; }
		}

		internal StringCollection ResolvedNamespaces {
			get { return wildcard.ResolvedNamespaces; }
		}

		internal XmlSchemaContentProcessing ResolvedProcessContents 
		{ 
			get{ return wildcard.ResolvedProcessing; } 
		}

		internal string TargetNamespace
		{
			get { return wildcard.TargetNamespace; }
		}

		/// <remarks>
		/// 1. id must be of type ID
		/// 2. namespace can have one of the following values:
		///		a) ##any or ##other
		///		b) list of anyURI and ##targetNamespace and ##local
		/// </remarks>
		[MonoTODO]
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			wildcard.TargetNamespace = schema.TargetNamespace;
			if (wildcard.TargetNamespace == null)
				wildcard.TargetNamespace = "";

			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);

			wildcard.Compile (Namespace, h, schema);

			if (processing == XmlSchemaContentProcessing.None)
				wildcard.ResolvedProcessing = XmlSchemaContentProcessing.Strict;
			else
				wildcard.ResolvedProcessing = processing;

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			return errorCount;
		}

		// 3.10.6 Wildcard Subset
		internal void ValidateWildcardSubset (XmlSchemaAnyAttribute other,
			ValidationEventHandler h, XmlSchema schema)
		{
			wildcard.ValidateWildcardSubset (other, h, schema);

			/*
			// 1.
			if (this.hasValueAny)
				return;
			if (this.hasValueOther) {
				if (other.hasValueOther) {
					// 2.1 and 2.2
					if (this.targetNamespace == other.targetNamespace ||
						other.targetNamespace == null || other.targetNamespace == "")
						return;
				}
				// 3.2.2
				else if (this.targetNamespace == null || targetNamespace == String.Empty)
					return;
				else {
					foreach (string ns in other.resolvedNamespaces)
						if (ns == this.targetNamespace) {
							error (h, "Invalid wildcard subset was found.");
							return;
						}
				}
			} else {
				// 3.1
				if (!this.hasValueLocal && other.hasValueLocal) {
					error (h, "Invalid wildcard subset was found.");
				} else if (other.resolvedNamespaces.Count == 0)
					return;
				else {
					ArrayList al = new ArrayList (this.resolvedNamespaces);
					foreach (string ns in other.resolvedNamespaces)
						if (!al.Contains (ns)) {
							error (h, "Invalid wildcard subset was found.");
							return;
						}
				}
			}
			*/
		}

		//<anyAttribute
		//  id = ID
		//  namespace = ((##any | ##other) | List of (anyURI | (##targetNamespace | ##local)) )  : ##any
		//  processContents = (lax | skip | strict) : strict
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</anyAttribute>
		internal static XmlSchemaAnyAttribute Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAnyAttribute any = new XmlSchemaAnyAttribute();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAnyAttribute.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			any.LineNumber = reader.LineNumber;
			any.LinePosition = reader.LinePosition;
			any.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					any.Id = reader.Value;
				}
				else if(reader.Name == "namespace")
				{
					any.nameSpace = reader.Value;
				}
				else if(reader.Name == "processContents")
				{
					Exception innerex;
					any.processing = XmlSchemaUtil.ReadProcessingAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for processContents",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for anyAttribute",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,any);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return any;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAnyAttribute.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						any.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}	
			return any;
		}
	}
}
