//
// System.Xml.XmlTextWriter
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlTextWriter : XmlWriter
	{
		#region Fields

		protected TextWriter w;
		protected bool nullEncoding = false;
		protected bool openWriter = true;
		protected bool openStartElement = false;
		protected bool openStartAttribute = false;
		protected bool documentStarted = false;
		protected bool namespaces = true;
		protected bool openAttribute = false;
		protected Stack openElements = new Stack ();
		protected Formatting formatting = Formatting.None;
		protected int indentation = 2;
		protected char indentChar = ' ';
		protected string indentChars = "  ";
		protected char quoteChar = '\"';
		protected int indentLevel = 0;
		protected string indentFormatting;
		protected Stream baseStream = null;
		protected string xmlLang = null;
		protected XmlSpace xmlSpace = XmlSpace.None;
		protected bool openXmlLang = false;
		protected bool openXmlSpace = false;

		#endregion

		#region Constructors

		public XmlTextWriter (TextWriter w) : base ()
		{
			this.w = w;
			
			try {
				baseStream = ((StreamWriter)w).BaseStream;
			}
			catch (Exception) { }
		}

		public XmlTextWriter (Stream w,	Encoding encoding) : base ()
		{
			if (encoding == null) {
				nullEncoding = true;
				encoding = new UTF8Encoding ();
			}

			this.w = new StreamWriter(w, encoding);
			baseStream = w;
		}

		public XmlTextWriter (string filename, Encoding encoding) : base ()
		{
			this.w = new StreamWriter(filename, false, encoding);
			baseStream = ((StreamWriter)w).BaseStream;
		}

		#endregion

		#region Properties

		public Stream BaseStream {
			get { return baseStream; }
		}


		public Formatting Formatting {
			get { return formatting; }
			set { formatting = value; }
		}

		public bool IndentingOverriden 
		{
			get {
				if (openElements.Count == 0)
					return false;
				else
					return (((XmlTextWriterOpenElement)openElements.Peek()).IndentingOverriden);
			}
			set {
				if (openElements.Count > 0)
					((XmlTextWriterOpenElement)openElements.Peek()).IndentingOverriden = value;
			}
		}

		public int Indentation {
			get { return indentation; }
			set {
				indentation = value;
				UpdateIndentChars ();
			}
		}

		public char IndentChar {
			get { return indentChar; }
			set {
				indentChar = value;
				UpdateIndentChars ();
			}
		}

		public bool Namespaces {
			get { return namespaces; }
			set {
				if (ws != WriteState.Start)
					throw new InvalidOperationException ("NotInWriteState.");
				
				namespaces = value;
			}
		}

		[MonoTODO]
		public char QuoteChar {
			get { return quoteChar; }
			set {
				if ((value != '\'') && (value != '\"'))
					throw new ArgumentException ("This is an invalid XML attribute quote character. Valid attribute quote characters are ' and \".");
				
				quoteChar = value;
			}
		}

		public override WriteState WriteState {
			get { return ws; }
		}
		
		public override string XmlLang {
			get {
				string xmlLang = null;
				int i;

				for (i = 0; i < openElements.Count; i++) 
				{
					xmlLang = ((XmlTextWriterOpenElement)openElements.ToArray().GetValue(i)).XmlLang;
					if (xmlLang != null)
						break;
				}

				return xmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				XmlSpace xmlSpace = XmlSpace.None;
				int i;

				for (i = 0; i < openElements.Count; i++) 
				{
					xmlSpace = ((XmlTextWriterOpenElement)openElements.ToArray().GetValue(i)).XmlSpace;
					if (xmlSpace != XmlSpace.None)
						break;
				}

				return xmlSpace;
			}
		}

		#endregion

		#region Methods

		private void CheckState ()
		{
			if (!openWriter) {
				throw new InvalidOperationException ("The Writer is closed.");
			}

			if ((documentStarted == true) && (formatting == Formatting.Indented) && (!IndentingOverriden)) {
				indentFormatting = "\r\n";
				if (indentLevel > 0) {
					for (int i = 0; i < indentLevel; i++)
						indentFormatting += indentChars;
				}
			}
			else
				indentFormatting = "";

			documentStarted = true;
		}

		public override void Close ()
		{
			while (openElements.Count > 0) {
				WriteEndElement();
			}

			w.Close();
			ws = WriteState.Closed;
			openWriter = false;
		}

		private void CloseStartElement ()
		{
			if (openStartElement) 
			{
				w.Write(">");
				ws = WriteState.Content;
				openStartElement = false;
			}
		}

		public override void Flush ()
		{
			w.Flush ();
		}

		[MonoTODO]
		public override string LookupPrefix (string ns)
		{
			throw new NotImplementedException ();
		}

		private void UpdateIndentChars ()
		{
			indentChars = "";
			for (int i = 0; i < indentation; i++)
				indentChars += indentChar;
		}

		[MonoTODO]
		public override void WriteBase64 (byte[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteBinHex (byte[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteCData (string text)
		{
			if (text.IndexOf("]]>") > 0) 
			{
				throw new ArgumentException ();
			}

			CheckState ();
			CloseStartElement ();

			w.Write("<![CDATA[{0}]]>", text);
		}

		[MonoTODO]
		public override void WriteCharEntity (char ch)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteChars (char[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteComment (string text)
		{
			if ((text.EndsWith("-")) || (text.IndexOf("-->") > 0)) {
				throw new ArgumentException ();
			}

			CheckState ();
			CloseStartElement ();

			w.Write ("<!--{0}-->", text);
		}

		[MonoTODO]
		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndAttribute ()
		{
			if (!openAttribute)
				throw new InvalidOperationException("Token EndAttribute in state Start would result in an invalid XML document.");

			CheckState ();

			if (openXmlLang) {
				w.Write (xmlLang);
				openXmlLang = false;
				((XmlTextWriterOpenElement)openElements.Peek()).XmlLang = xmlLang;
			}

			if (openXmlSpace) 
			{
				w.Write (xmlSpace.ToString ().ToLower ());
				openXmlSpace = false;
				((XmlTextWriterOpenElement)openElements.Peek()).XmlSpace = xmlSpace;
			}

			w.Write ("{0}", quoteChar);

			openAttribute = false;
		}

		[MonoTODO]
		public override void WriteEndDocument ()
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndElement ()
		{
			if (openElements.Count == 0)
				throw new InvalidOperationException("There was no XML start tag open.");

			indentLevel--;

			CheckState ();

			if (openStartElement) {
				w.Write (" />");
				openElements.Pop ();
				openStartElement = false;
			}
			else {
				w.Write ("{0}</{1}>", indentFormatting, openElements.Pop ());
				namespaceManager.PopScope();
			}
		}

		[MonoTODO]
		public override void WriteEntityRef (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteFullEndElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteNmToken (string name)
		{
			throw new NotImplementedException ();
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			if ((name == null) || (name == string.Empty) || (name.IndexOf("?>") > 0) || (text.IndexOf("?>") > 0)) {
				throw new ArgumentException ();
			}

			CheckState ();
			CloseStartElement ();

			w.Write ("{0}<?{1} {2}?>", indentFormatting, name, text);
		}

		[MonoTODO]
		public override void WriteQualifiedName (string localName, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteRaw (string data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteRaw (char[] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("haven't tested namespaces on attributes code yet.")]
		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			if ((prefix == "xml") && (localName == "lang"))
				openXmlLang = true;

			if ((prefix == "xml") && (localName == "space"))
				openXmlSpace = true;

			if ((prefix == "xmlns") && (localName == "xmlns"))
				throw new ArgumentException ("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML.");

			CheckState ();

			if (prefix == null)
				prefix = String.Empty;

			if (ns == null)
				ns = String.Empty;

			string formatPrefix = "";

			if (ns != String.Empty) 
			{
				string existingPrefix = namespaceManager.LookupPrefix (ns);

				if (prefix == String.Empty)
					prefix = existingPrefix;
			}

			if (prefix != String.Empty) 
			{
				formatPrefix = prefix + ":";
			}

			w.Write (" {0}{1}={2}", formatPrefix, localName, quoteChar);

			openAttribute = true;
			ws = WriteState.Attribute;
		}

		public override void WriteStartDocument ()
		{
			WriteStartDocument ("");
		}

		public override void WriteStartDocument (bool standalone)
		{
			string standaloneFormatting;

			if (standalone == true)
				standaloneFormatting = String.Format (" standalone={0}yes{0}", quoteChar);
			else
				standaloneFormatting = String.Format (" standalone={0}no{0}", quoteChar);

			WriteStartDocument (standaloneFormatting);
		}

		private void WriteStartDocument (string standaloneFormatting)
		{
			if (documentStarted == true)
				throw new InvalidOperationException("WriteStartDocument should be the first call.");

			CheckState ();

			string encodingFormatting = "";

			if (!nullEncoding)
				encodingFormatting = String.Format (" encoding={0}{1}{0}", quoteChar, w.Encoding.HeaderName);

			w.Write("<?xml version={0}1.0{0}{1}{2}?>", quoteChar, encodingFormatting, standaloneFormatting);
			ws = WriteState.Prolog;
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			if (!Namespaces && (((prefix != null) && (prefix != String.Empty))
				|| ((ns != null) && (ns != String.Empty))))
				throw new ArgumentException ("Cannot set the namespace if Namespaces is 'false'.");

			WriteStartElementInternal (prefix, localName, ns);
		}

		protected override void WriteStartElementInternal (string prefix, string localName, string ns)
		{
			if (prefix == null)
				prefix = String.Empty;

			if (ns == null)
				ns = String.Empty;

			if ((prefix != String.Empty) && ((ns == null) || (ns == String.Empty)))
				throw new ArgumentException ("Cannot use a prefix with an empty namespace.");

			CheckState ();
			CloseStartElement ();

			string formatXmlns = "";
			string formatPrefix = "";

			if (ns != String.Empty) 
			{
				string existingPrefix = namespaceManager.LookupPrefix (ns);

				if (prefix == String.Empty)
					prefix = existingPrefix;

				if (prefix != existingPrefix)
					formatXmlns = String.Format (" xmlns:{0}={1}{2}{1}", prefix, quoteChar, ns);
				else if (existingPrefix == String.Empty)
					formatXmlns = String.Format (" xmlns={0}{1}{0}", quoteChar, ns);
			}
			else if ((prefix == String.Empty) && (namespaceManager.LookupNamespace(prefix) != String.Empty)) {
				formatXmlns = String.Format (" xmlns={0}{0}", quoteChar);
			}

			if (prefix != String.Empty) {
				formatPrefix = prefix + ":";
			}

			w.Write ("{0}<{1}{2}{3}", indentFormatting, formatPrefix, localName, formatXmlns);

			openElements.Push (new XmlTextWriterOpenElement (formatPrefix + localName));
			ws = WriteState.Element;
			openStartElement = true;

			namespaceManager.PushScope ();
			namespaceManager.AddNamespace (prefix, ns);

			indentLevel++;
		}

		public override void WriteString (string text)
		{
			if (ws == WriteState.Prolog)
				throw new InvalidOperationException ("Token content in state Prolog would result in an invalid XML document.");

			if (text == null)
				text = String.Empty;

			if (text != String.Empty) {
				CheckState ();

				text = text.Replace ("&", "&amp;");
				text = text.Replace ("<", "&lt;");
				text = text.Replace (">", "&gt;");
				
				if (openAttribute) {
					if (quoteChar == '"')
						text = text.Replace ("\"", "&quot;");
					else
						text = text.Replace ("'", "&apos;");
				}

				if (!openAttribute)
					CloseStartElement ();

				if (!openXmlLang && !openXmlSpace)
					w.Write (text);
				else {
					if (openXmlLang)
						xmlLang = text;
					else {
						switch (text) {
							case "default":
								xmlSpace = XmlSpace.Default;
								break;
							case "preserve":
								xmlSpace = XmlSpace.Preserve;
								break;
							default:
								throw new ArgumentException ("'{0}' is an invalid xml:space value.");
						}
					}
				}
			}

			IndentingOverriden = true;
		}

		[MonoTODO]
		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			throw new NotImplementedException ();
		}

		public override void WriteWhitespace (string ws)
		{
			foreach (char c in ws) {
				if ((c != ' ') && (c != '\t') && (c != '\r') && (c != '\n'))
					throw new ArgumentException ();
			}

			w.Write (ws);
		}

		#endregion
	}
}
