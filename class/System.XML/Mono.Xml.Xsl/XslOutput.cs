//
// XslOutput.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
// (C) 2003 Oleg Tkachenko
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;

namespace Mono.Xml.Xsl
{
	using QName = System.Xml.XmlQualifiedName;

	public enum OutputMethod {
		XML,
		HTML,
		Text,
		Custom,
		Unknown
	}
	
	public enum StandaloneType {
		NONE,
		YES,
		NO
        }
	
	public class XslOutput	// also usable for xsl:result-document
	{
		string uri;
		QName customMethod;
		OutputMethod method = OutputMethod.Unknown; 
		string version;
		Encoding encoding = System.Text.Encoding.UTF8;
		bool omitXmlDeclaration;
		StandaloneType standalone = StandaloneType.NONE;
		string doctypePublic;
		string doctypeSystem;
		QName [] cdataSectionElements;
		string indent;
		string mediaType;
		bool escapeUriAttributes;
		bool includeContentType;
		bool normalizeUnicode;
		bool undeclareNamespaces;
		QName [] useCharacterMaps;

		// for compilation only.
		ArrayList cdSectsList = new ArrayList ();

		public XslOutput (string uri)
		{
			this.uri = uri;
		}

		public OutputMethod Method { get { return method; }}
		public QName CustomMethod { get { return customMethod; }}

		public string Version {
			get { return version; }
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		public string Uri {
			get { return uri; }
		}

		public bool OmitXmlDeclaration {
			get { return omitXmlDeclaration; }
		}

		public StandaloneType Standalone {
			get { return standalone; }
		}

		public string DoctypePublic {
			get { return doctypePublic; }
		}

		public string DoctypeSystem {
			get { return doctypeSystem; }
		}

		public QName [] CDataSectionElements {
			get {
				if (cdataSectionElements == null)
					cdataSectionElements = cdSectsList.ToArray (typeof (QName)) as QName [];
				return cdataSectionElements;
			}
		}

		public string Indent {
			get { return indent; }
		}

		public string MediaType {
			get { return mediaType; }
		}

		// Below are introduced in XSLT 2.0 (WD-20030502)
		public bool EscapeUriAttributes {
			get { return escapeUriAttributes; }
		}

		public bool IncludeContentType {
			get { return includeContentType; }
		}

		public bool NormalizeUnicode {
			get { return normalizeUnicode; }
		}

		public bool UndeclareNamespaces {
			get { return undeclareNamespaces; }
		}

		public QName [] UseCharacterMaps {
			get { return useCharacterMaps; }
		}

		public void Fill (XPathNavigator nav)
		{
			string att;
			
			att = nav.GetAttribute ("cdata-section-elements", "");
			if (att != String.Empty)
				cdSectsList.AddRange (XslNameUtil.FromListString (att, nav));

			att = nav.GetAttribute ("method", "");

			if (att != String.Empty) {
				switch (att) {
				case "xml":
					method = OutputMethod.XML;
					break;
				case "html":
					method = OutputMethod.HTML;
					break;
				case "text":
					method = OutputMethod.Text;
					break;
				default:
					method = OutputMethod.Custom;
					customMethod = XslNameUtil.FromString (att, nav);
					if (customMethod.Namespace == String.Empty) {
						IXmlLineInfo li = nav as IXmlLineInfo;
						throw new XsltCompileException (new ArgumentException ("Invalid output method value: '" + att + 
							"'. It must be either 'xml' or 'html' or 'text' or QName."),
							nav.BaseURI,
							li != null ? li.LineNumber : 0,
							li != null ? li.LinePosition : 0);
					}
					break;
				}
			}

			att = nav.GetAttribute ("version", "");
			if (att != String.Empty)
				this.version = att;

			att = nav.GetAttribute ("encoding", "");
			if (att != String.Empty)
				this.encoding = System.Text.Encoding.GetEncoding (att);

			att = nav.GetAttribute ("standalone", "");
			if (att != String.Empty)
				//TODO: Should we validate values?                
				this.standalone = att == "yes" ? StandaloneType.YES : StandaloneType.NO;


			att = nav.GetAttribute ("doctype-public", "");
			if (att != String.Empty)
				this.doctypePublic = att;

			att = nav.GetAttribute ("doctype-system", "");
			if (att != String.Empty)
				this.doctypeSystem = att;

			att = nav.GetAttribute ("media-type", "");
			if (att != String.Empty)
				this.mediaType = att;

			att = nav.GetAttribute ("omit-xml-declaration", "");
			if (att != String.Empty)
				this.omitXmlDeclaration = att == "yes";

			att = nav.GetAttribute ("indent", "");
			if (att != String.Empty)
				this.indent = att;
		}
	}

}
