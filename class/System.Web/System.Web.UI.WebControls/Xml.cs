/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Xml
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  75%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[PersistChildren(false)]
	public class Xml : Control
	{
		private XmlDocument      document;
		private string           documentContent;
		private string           documentSource;
		private XslTransform     transform;
		private XsltArgumentList transformArgumentList;
		private string           transformSource;

		private XPathDocument xpathDoc;

		private static XslTransform defaultTransform;

		static Xml()
		{
			XmlTextReader reader = new XmlTextReader(new StringReader("<xsl:stylesheet version='1.0' " +
			                                        "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
			                                        "<xsl:template match=\"\">" +
			                                        "<xsl:copy-of select=\".\"/>" +
			                                        "</xsl:template>" +
			                                        "</xsl:stylesheet>"));
			defaultTransform = new XslTransform();
			defaultTransform.Load(reader);
		}

		public Xml(): base()
		{
		}

		[MonoTODO("Initialize_Document")]
		private void LoadXmlDoc()
		{
			throw new NotImplementedException();
		}

		public XmlDocument Document
		{
			get
			{
				if(document == null)
					LoadXmlDoc();
				return document;
			}
			set
			{
				documentSource  = null;
				documentContent = null;
				xpathDoc        = null;
				document        = value;
			}
		}

		public string DocumentContent
		{
			get
			{
				return String.Empty;
			}
			set
			{
				document        = null;
				xpathDoc        = null;
				documentContent = value;
			}
		}

		public string DocumentSource
		{
			get
			{
				if(documentSource != null)
					return documentSource;
				return String.Empty;
			}
			set
			{
				document        = null;
				documentContent = null;
				xpathDoc        = null;
				documentSource  = value;
			}
		}

		public XslTransform Transform
		{
			get
			{
				return transform;
			}
			set
			{
				transformSource = null;
				transform       = value;
			}
		}

		public string TransformSource
		{
			get
			{
				if(transformSource != null)
					return transformSource;
				return String.Empty;
			}
			set
			{
				transform       = null;
				transformSource = value;
			}
		}

		public XsltArgumentList TransformArgumentList
		{
			get
			{
				return transformArgumentList;
			}
			set
			{
				transformArgumentList = value;
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(obj is LiteralControl)
			{
				DocumentContent = ((LiteralControl)obj).Text;
				return;
			}
			throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Have_Children_of_Type", "Xml", GetType().Name.ToString()));
		}

		[MonoTODO("Initialize_xpathDocument")]
		private void LoadXpathDoc()
		{
			if(documentContent != null && documentContent.Length > 0)
			{
				xpathDoc = new XPathDocument(new StringReader(documentContent));
				return;
			}
			if(documentSource == null || documentSource.Length == 0)
			{
				return;
			}
			throw new NotImplementedException();
		}

		[MonoTODO("Initialize_Transform")]
		private void LoadTransform()
		{
			throw new ArgumentException();
		}

		[MonoTODO]
		protected override void Render(HtmlTextWriter output)
		{
			if(document == null)
			{
				LoadXpathDoc();
			}

			LoadTransform();
			if(document == null || xpathDoc == null)
			{
				return;
			}
			if(transform == null)
			{
				transform = defaultTransform;
			}
			if(document != null)
			{
				Transform.Transform(document, transformArgumentList, output);
				return;
			}
			Transform.Transform(xpathDoc, transformArgumentList, output);
		}
	}
}
