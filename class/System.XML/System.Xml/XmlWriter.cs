//
// System.Xml.XmlWriter
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Kral Ferch
// (C) 2002-2003 Atsushi Enomoto
//

using System;

namespace System.Xml
{
#if NET_2_0
	public abstract class XmlWriter : IDisposable
#else
	public abstract class XmlWriter
#endif
	{
		#region Constructors

		protected XmlWriter () { }

		#endregion

		#region Properties

		public abstract WriteState WriteState { get; }
		
		public abstract string XmlLang { get; }

		public abstract XmlSpace XmlSpace { get; }

		#endregion

		#region Methods

		public abstract void Close ();

#if NET_2_0
		public virtual void Dispose ()
		{
			Close ();
		}
#endif

		public abstract void Flush ();

		public abstract string LookupPrefix (string ns);

		private void WriteAttribute (XmlReader reader, bool defattr)
		{
			if (!defattr && reader.IsDefault)
				return;

			WriteStartAttribute (reader.Prefix, reader.LocalName, reader.NamespaceURI);
			while (reader.ReadAttributeValue ()) {
				switch (reader.NodeType) {
				case XmlNodeType.Text:
					WriteString (reader.Value);
					break;
				case XmlNodeType.EntityReference:
					WriteEntityRef (reader.Name);
					break;
				}
			}
			WriteEndAttribute ();
		}

		public virtual void WriteAttributes (XmlReader reader, bool defattr)
		{
			if(reader == null)
				throw new ArgumentException("null XmlReader specified.", "reader");

			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
				WriteAttributeString ("version", reader ["version"]);
				if (reader ["encoding"] != null)
					WriteAttributeString ("encoding", reader ["encoding"]);
				if (reader ["standalone"] != null)
					WriteAttributeString ("standalone", reader ["standalone"]);
				break;
			case XmlNodeType.Element:
				if (reader.MoveToFirstAttribute ())
					goto case XmlNodeType.Attribute;
				break;
			case XmlNodeType.Attribute:
				do {
					WriteAttribute (reader, defattr);
				} while (reader.MoveToNextAttribute ());
				break;
			default:
				throw new XmlException("NodeType is not one of Element, Attribute, nor XmlDeclaration.");
			}
		}

		public void WriteAttributeString (string localName, string value)
		{
			WriteAttributeString ("", localName, null, value);
		}

		public void WriteAttributeString (string localName, string ns, string value)
		{
			WriteAttributeString ("", localName, ns, value);
		}

		public void WriteAttributeString (string prefix, string localName, string ns, string value)
		{
			// In MS.NET (1.0), this check is done *here*, not at WriteStartAttribute.
			// (XmlTextWriter.WriteStartAttribute("xmlns", "anyname", null) throws an exception.

#if NET_1_0
			if ((prefix == "xmlns" || (prefix == "" && localName == "xmlns")) && ns == null)
				ns = "http://www.w3.org/2000/xmlns/";
#endif

			WriteStartAttribute (prefix, localName, ns);
			WriteString (value);
			WriteEndAttribute ();
		}

		public abstract void WriteBase64 (byte[] buffer, int index, int count);

		public abstract void WriteBinHex (byte[] buffer, int index, int count);

		public abstract void WriteCData (string text);

		public abstract void WriteCharEntity (char ch);

		public abstract void WriteChars (char[] buffer, int index, int count);

		public abstract void WriteComment (string text);

		public abstract void WriteDocType (string name, string pubid, string sysid, string subset);

		public void WriteElementString (string localName, string value)
		{
			WriteStartElement(localName);
			WriteString(value);
			WriteEndElement();
		}

		public void WriteElementString (string localName, string ns, string value)
		{
			WriteStartElement(localName, ns);
			WriteString(value);
			WriteEndElement();
		}

		public abstract void WriteEndAttribute ();

		public abstract void WriteEndDocument ();

		public abstract void WriteEndElement ();

		public abstract void WriteEntityRef (string name);

		public abstract void WriteFullEndElement ();

		public abstract void WriteName (string name);

		public abstract void WriteNmToken (string name);

		public virtual void WriteNode (XmlReader reader, bool defattr)
		{
			if (reader == null)
				throw new ArgumentException ();

			if (reader.ReadState == ReadState.Initial) {
				reader.Read ();
				do {
					WriteNode (reader, defattr);
				} while (!reader.EOF);
				return;
			}

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				WriteStartElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
#if false
				WriteAttributes (reader, defattr);
				reader.MoveToElement ();
#else
				// Well, I found that MS.NET took this way, since
				// there was a error-prone SgmlReader that fails
				// MoveToNextAttribute().
				if (reader.HasAttributes) {
					for (int i = 0; i < reader.AttributeCount; i++) {
						reader.MoveToAttribute (i);
						WriteAttribute (reader, defattr);
					}
					reader.MoveToElement ();
				}
#endif
				if (reader.IsEmptyElement)
					WriteEndElement ();
				else {
					int depth = reader.Depth;
					reader.Read ();
					if (reader.NodeType != XmlNodeType.EndElement) {
						do {
							WriteNode (reader, defattr);
						} while (depth < reader.Depth);
					}
					WriteFullEndElement ();
				}
				break;
			// In case of XmlAttribute, don't proceed reader.
			case XmlNodeType.Attribute:
				return;
			case XmlNodeType.Text:
				WriteString (reader.Value);
				break;
			case XmlNodeType.CDATA:
				WriteCData (reader.Value);
				break;
			case XmlNodeType.EntityReference:
				WriteEntityRef (reader.Name);
				break;
			case XmlNodeType.XmlDeclaration:
				// LAMESPEC: It means that XmlWriter implementation _must not_ check
				// whether PI name is "xml" (it is XML error) or not.
			case XmlNodeType.ProcessingInstruction:
				WriteProcessingInstruction (reader.Name, reader.Value);
				break;
			case XmlNodeType.Comment:
				WriteComment (reader.Value);
				break;
			case XmlNodeType.DocumentType:
				WriteDocType (reader.Name,
					reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
				break;
			case XmlNodeType.SignificantWhitespace:
				goto case XmlNodeType.Whitespace;
			case XmlNodeType.Whitespace:
				WriteWhitespace (reader.Value);
				break;
			case XmlNodeType.EndElement:
				WriteFullEndElement ();
				break;
			case XmlNodeType.EndEntity:
				break;
			case XmlNodeType.None:
				return;	// Do nothing, nor reporting errors.
			default:
				throw new XmlException ("Unexpected node " + reader.Name + " of type " + reader.NodeType);
			}
			reader.Read ();
		}

		public abstract void WriteProcessingInstruction (string name, string text);

		public abstract void WriteQualifiedName (string localName, string ns);

		public abstract void WriteRaw (string data);

		public abstract void WriteRaw (char[] buffer, int index, int count);

		public void WriteStartAttribute (string localName, string ns)
		{
			WriteStartAttribute (null, localName, ns);
		}

		public abstract void WriteStartAttribute (string prefix, string localName, string ns);

		public abstract void WriteStartDocument ();

		public abstract void WriteStartDocument (bool standalone);

		public void WriteStartElement (string localName)
		{
			WriteStartElement (null, localName, null);
		}

		public void WriteStartElement (string localName, string ns)
		{
			WriteStartElement (null, localName, ns);
		}

		public abstract void WriteStartElement (string prefix, string localName, string ns);

		public abstract void WriteString (string text);

		public abstract void WriteSurrogateCharEntity (char lowChar, char highChar);

		public abstract void WriteWhitespace (string ws);

		#endregion
	}
}
