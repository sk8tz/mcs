//
// HtmlEmitter.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//	
// (C)2003 Atsushi Enomoto
//
// TODO:
//	indent, uri escape, allowed entity char such as &nbsp;,
//	encoding to meta tag, doctype-public/doctype-system.
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.Xml.Xsl
{
	public class HtmlEmitter : Emitter {
		TextWriter writer;
		Stack elementNameStack;
		bool openElement;
		bool openAttribute;
		int xmlDepth;
		bool indent;

		public HtmlEmitter (TextWriter writer, XslOutput output)
		{
			this.writer = writer;
			indent = !(output.Indent == "no");
			elementNameStack = new Stack ();
			xmlDepth = -1;
		}

		public override void WriteStartDocument (StandaloneType standalone)
		{
			// do nothing
		}
		
		public override void WriteEndDocument ()
		{
			// do nothing
		}

		public override void WriteDocType (string name, string publicId, string systemId)
		{
			writer.Write ("<!DOCTYPE ");
			writer.Write (name);
			writer.Write (' ');
			if (publicId != null && publicId != String.Empty) {
				writer.Write ("PUBLIC ");
				writer.Write (publicId);
				writer.Write (' ');
				writer.Write (systemId);
			} else if (systemId != null && systemId != String.Empty) {
				writer.Write ("SYSTEM ");
				writer.Write (systemId);
			}
			writer.Write ('>');
		}

		private void CloseAttribute ()
		{
			writer.Write ('\"');
			openAttribute = false;
		}

		private void CloseStartElement ()
		{
			if (openAttribute)
				CloseAttribute ();
			writer.Write ('>');
			openElement = false;
		}

		private void Indent (string elementName, bool alwaysOutputNewLine)
		{
			if (!indent)
				return;
			switch (elementName.ToUpper ()) {
			case "FORM":
				return;
			}
			if (alwaysOutputNewLine || elementNameStack.Count > 0)
				writer.Write ("\r\n");
			for (int i = 0; i < elementNameStack.Count; i++)
					writer.Write ("  ");
		}

		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			if (openElement)
				CloseStartElement ();
			Indent (elementNameStack.Count > 0 ? elementNameStack.Peek () as string : String.Empty, false);
			string formatName = localName;

			writer.Write ('<');
			if (nsURI != String.Empty) {
				// XML output
				if (prefix != String.Empty) {
					writer.Write (prefix);
					writer.Write (':');
					formatName = String.Concat (prefix, ":", localName);
				}
				// TODO: handle xmlns using namespaceManager
				
				if (xmlDepth < 0)
					xmlDepth = elementNameStack.Count + 1;
			}
			writer.Write (formatName);
			elementNameStack.Push (formatName);
			openElement = true;
		}

		public override void WriteEndElement ()
		{
			WriteFullEndElement ();
		}

		public override void WriteFullEndElement ()
		{
			string element = elementNameStack.Pop () as string;

			switch (element.ToUpper ()) {
			case "AREA":
			case "BASE":
			case "BASEFONT":
			case "BR":
			case "COL":
			case "FRAME":
			case "HR":
			case "IMAGE":
			case "INPUT":
			case "ISINDEX":
			case "LINK":
			case "META":
			case "PARAM":
				if (openAttribute)
					CloseAttribute ();
				writer.Write ('>');
				break;
			default:
				if (openElement)
					CloseStartElement ();
				Indent (element, true);
				writer.Write ("</");
				writer.Write (element);
				writer.Write (">");
				break;
			}
			openElement = false;

			if (xmlDepth > elementNameStack.Count)
				xmlDepth = -1;
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value)
		{
			if (xmlDepth >= 0) {
				writer.Write (' ');
				writer.Write (localName);
				writer.Write ("=\"");
				openAttribute = true;
				WriteFormattedString (value);
				openAttribute = false;
				writer.Write ('\"');
			}

			string attribute = localName.ToUpper ();
			writer.Write (' ');
			writer.Write (localName);

			switch (attribute) {
			case "OPTION":
			case "CHECKED":
				return;
			}

			writer.Write ("=\"");
			openAttribute = true;
			WriteFormattedString (value);
			openAttribute = false;
			writer.Write ('\"');
		}

		public override void WriteComment (string text) {
			writer.Write ("<!--");
			writer.Write (text);
			writer.Write ("-->");
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			writer.Write ("<?");
			writer.Write (name);
			if (text != null && text != String.Empty) {
				writer.Write (' ');
				writer.Write (text);
			}

			if (xmlDepth >= 0)
				writer.Write ("?>");
			else
				writer.Write (">"); // HTML PI ends with '>'
		}

		public override void WriteString (string text)
		{
			if (openElement)
				CloseStartElement ();
			WriteFormattedString (text);
		}

		private void WriteFormattedString (string text)
		{
			// style and script should not be escaped.
			if (!openAttribute) {
				string element = ((string) elementNameStack.Peek ()).ToUpper ();
				switch (element) {
				case "SCRIPT":
				case "STYLE":
					writer.Write (text);
					return;
				}
			}

			int start = 0;
			for (int i = 0; i < text.Length; i++) {
				switch (text [i]) {
				case '&':
					// '&' '{' should be "&{", not "&amp;{"
					if (xmlDepth < 0 && i + 1 < text.Length && text [i + 1] == '{')
						continue;
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&amp;");
					start = i;
					break;
				case '<':
					if (openAttribute)
						continue;
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&lt;");
					start = i;
					break;
				case '>':
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&gt;");
					start = i;
					break;
				case '\'':
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&apos;");
					start = i;
					break;
				case '\"':
					writer.Write (text.ToCharArray (), start, i - start);
					writer.Write ("&quot;");
					start = i;
					break;
				}
			}
			writer.Write (text.ToCharArray (), start, text.Length - start);
		}

		public override void WriteRaw (string data)
		{
			writer.Write (data);
		}

		public override void Done ()
		{
			writer.Flush ();
		}
	}
}
