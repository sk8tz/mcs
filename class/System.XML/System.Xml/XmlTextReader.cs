//
// System.Xml.XmlTextReader
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Adam Treat (manyoso@yahoo.com)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
//

// FIXME:
//   This can only parse basic XML: elements, attributes, processing
//   instructions, and comments are OK.
//
//   It barfs on DOCTYPE declarations.
//
//   There's also no checking being done for either well-formedness
//   or validity.
//
//   NameTables aren't being used everywhere yet.
//
//   Some thought needs to be given to performance. There's too many
//   strings being allocated.
//
//   Some of the MoveTo methods haven't been implemented yet.
//
//   LineNumber and LinePosition aren't being tracked.
//
//   xml:space, xml:lang, and xml:base aren't being tracked.
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlTextReader : XmlReader, IXmlLineInfo
	{
		WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
		#region Constructors

		protected XmlTextReader ()
		{
		}

		[MonoTODO]
		public XmlTextReader (Stream input)
		{
			// We can share some code in the constructors (at least for this one and next 2)
			XmlNameTable nt = new NameTable ();
			XmlNamespaceManager nsMgr = new XmlNamespaceManager (nt);
			parserContext = new XmlParserContext (null, nsMgr, null, XmlSpace.None);
			Init ();
			reader = new StreamReader (input);
		}

		[MonoTODO]
		public XmlTextReader (string url)
		{
			XmlNameTable nt = new NameTable ();
			XmlNamespaceManager nsMgr = new XmlNamespaceManager (nt);
			parserContext = new XmlParserContext (null, nsMgr, null, XmlSpace.None);
			Init ();
			// StreamReader does not support url, only filepath;-)
			reader = new StreamReader(url);
		}

		[MonoTODO]
		public XmlTextReader (TextReader input)
		{
			XmlNameTable nt = new NameTable ();
			XmlNamespaceManager nsMgr = new XmlNamespaceManager (nt);
			parserContext = new XmlParserContext (null, nsMgr, null, XmlSpace.None);
			Init ();
			reader = input;
		}

		[MonoTODO]
		protected XmlTextReader (XmlNameTable nt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (Stream input, XmlNameTable nt)
 		{
			XmlNamespaceManager nsMgr = new XmlNamespaceManager (nt);
			parserContext = new XmlParserContext (null, nsMgr, null, XmlSpace.None);
			Init ();
			reader = new StreamReader (input);
		}

		[MonoTODO]
		public XmlTextReader (string url, Stream input)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (string url, TextReader input)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (string url, XmlNameTable nt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (TextReader input, XmlNameTable nt)
		{
			XmlNamespaceManager nsMgr = new XmlNamespaceManager (nt);
			parserContext = new XmlParserContext (null, nsMgr, null, XmlSpace.None);
			Init ();
			reader = input;
		}

		[MonoTODO]
		public XmlTextReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
		{
			parserContext = context;
			reader = new StreamReader(xmlFragment);
			Init();
//			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (string url, Stream input, XmlNameTable nt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (string url, TextReader input, XmlNameTable nt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTextReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
		{
			//Waiting for Validating reader for fragType rules.
			parserContext = context;
			Init ();
			reader = new StringReader(xmlFragment);
		}

		#endregion

		#region Properties

		public override int AttributeCount
		{
			get { return attributes.Count; }
		}

		public override string BaseURI
		{
			get { return parserContext.BaseURI; }
		}

		public override int Depth
		{
			get {
				return elementDepth;
			}
		}

		public Encoding Encoding
		{
			get { return parserContext.Encoding; }
		}

		public override bool EOF
		{
			get
			{
				return
					readState == ReadState.EndOfFile ||
					readState == ReadState.Closed;
			}
		}

		public override bool HasValue
		{
			get { return value != String.Empty;	}
		}

		public override bool IsDefault
		{
			get
			{
				// XmlTextReader does not expand default attributes.
				return false;
			}
		}

		public override bool IsEmptyElement
		{
			get { return isEmptyElement; }
		}

		public override string this [int i]
		{
			get { return GetAttribute (i); }
		}

		public override string this [string name]
		{
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string namespaceName]
		{
			get { return GetAttribute (localName, namespaceName); }
		}

		public int LineNumber
		{
			get { return line; }
		}

		public int LinePosition
		{
			get { return column; }
		}

		public override string LocalName
		{
			get { return localName; }
		}

		public override string Name
		{
			get { return name; }
		}

		[MonoTODO]
		public bool Namespaces
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override string NamespaceURI
		{
			get { return namespaceURI; }
		}

		public override XmlNameTable NameTable
		{
			get { return parserContext.NameTable; }
		}

		public override XmlNodeType NodeType
		{
			get { return nodeType; }
		}

		[MonoTODO]
		public bool Normalization
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override string Prefix
		{
			get { return prefix; }
		}

		[MonoTODO]
		public override char QuoteChar
		{
			get { throw new NotImplementedException (); }
		}

		public override ReadState ReadState
		{
			get { return readState; }
		}

		public override string Value
		{
			get { return value; }
		}

		public WhitespaceHandling WhitespaceHandling
		{
			get { return whitespaceHandling; }
			set { whitespaceHandling = value; }
		}

		[MonoTODO]
		public override string XmlLang
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlResolver XmlResolver
		{
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override XmlSpace XmlSpace
		{
			get { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
			readState = ReadState.Closed;
		}

		[MonoTODO]
		public override string GetAttribute (int i)
		{
			if (i > attributes.Count)
				throw new ArgumentOutOfRangeException ("i is smaller than AttributeCount");
			else
				throw new NotImplementedException ();
		}

		public override string GetAttribute (string name)
		{
			return attributes.ContainsKey (name) ?
				attributes [name] as string : String.Empty;
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			foreach (DictionaryEntry entry in attributes)
			{
				string thisName = entry.Key as string;

				int indexOfColon = thisName.IndexOf (':');

				if (indexOfColon != -1) {
					string thisLocalName = thisName.Substring (indexOfColon + 1);

					if (localName == thisLocalName) {
						string thisPrefix = thisName.Substring (0, indexOfColon);
						string thisNamespaceURI = LookupNamespace (thisPrefix);

						if (namespaceURI == thisNamespaceURI)
							return attributes.ContainsKey (thisName) ?
								attributes [thisName] as string : String.Empty;
					}
				} else if (localName == "xmlns" && namespaceURI == "http://www.w3.org/2000/xmlns/" && thisName == "xmlns")
					return attributes.ContainsKey (thisName) ? 
						attributes [thisName] as string : String.Empty;
			}

			return String.Empty;
		}

		[MonoTODO]
		public TextReader GetRemainder ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IXmlLineInfo.HasLineInfo ()
		{
			return false;
		}

		public override string LookupNamespace (string prefix)
		{
			return parserContext.NamespaceManager.LookupNamespace (prefix);
		}

		[MonoTODO]
		public override void MoveToAttribute (int i)
		{
			throw new NotImplementedException ();
		}

		public override bool MoveToAttribute (string name)
		{
			MoveToElement ();
			bool match = false;

			if (attributes == null)
				return false;

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
				orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			}

			while (orderedAttributesEnumerator.MoveNext ()) {
				if(name == orderedAttributesEnumerator.Current as string) {
					match = true;
					break;
				}
			}

			if (match) {
					
				string value = attributes [name] as string;
				SetProperties (
					XmlNodeType.Attribute, // nodeType
					name, // name
					false, // isEmptyElement
					value, // value
					false // clearAttributes
				);
			}
			
			return match;
		}

		[MonoTODO]
		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			throw new NotImplementedException ();
		}

		public override bool MoveToElement ()
		{
			if (orderedAttributesEnumerator != null) {
				orderedAttributesEnumerator = null;
				RestoreProperties ();
				return true;
			}

			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			MoveToElement ();
			return MoveToNextAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			if (attributes == null)
				return false;

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
				orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			}

			if (orderedAttributesEnumerator.MoveNext ()) {
				string name = orderedAttributesEnumerator.Current as string;
				string value = attributes [name] as string;
				SetProperties (
					XmlNodeType.Attribute, // nodeType
					name, // name
					false, // isEmptyElement
					value, // value
					false // clearAttributes
				);
				return true;
			}

			return false;
		}

		public override bool Read ()
		{
			bool more = false;

			readState = ReadState.Interactive;

			more = ReadContent ();

			return more;
		}

		[MonoTODO]
		public override bool ReadAttributeValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadChars (char [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ReadInnerXml ()
		{
			// Still need a Well Formedness check.
			// Will wait for Validating reader ;-)
			if (NodeType == XmlNodeType.Attribute) {
				return Value;
			} else {
   				saveToXmlBuffer = true;
				string startname = this.Name;
				string endname = string.Empty;
				readState = ReadState.Interactive;

				while (startname != endname) {
					ReadContent ();
					endname = this.Name;
				}

				xmlBuffer.Replace(currentTag.ToString (), "");
				saveToXmlBuffer = false;
				string InnerXml = xmlBuffer.ToString ();
				xmlBuffer.Length = 0;
				return InnerXml;
			}
		}

		[MonoTODO]
		public override string ReadOuterXml ()
		{
			if (NodeType == XmlNodeType.Attribute) {
				return Name+"=\""+Value+"\"";
			} else {
   				saveToXmlBuffer = true;
				xmlBuffer.Append(currentTag.ToString ());
				string startname = this.Name;
				string endname = string.Empty;
				readState = ReadState.Interactive;

				while (startname != endname) {
					ReadContent ();
					endname = this.Name;
				}
				saveToXmlBuffer = false;
				string OuterXml = xmlBuffer.ToString ();
				xmlBuffer.Length = 0;
				return OuterXml;
			}
		}

		[MonoTODO]
		public override string ReadString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetState ()
		{
			throw new NotImplementedException ();
		}

		public override void ResolveEntity ()
		{
			// XmlTextReaders don't resolve entities.
			throw new InvalidOperationException ("XmlTextReaders don't resolve entities.");
		}

		#endregion

		// privates

		private XmlParserContext parserContext;

		private TextReader reader;
		private ReadState readState;

		private int depth;
		private int elementDepth;
		private bool depthDown;

		private bool popScope;

		private XmlNodeType nodeType;
		private string name;
		private string prefix;
		private string localName;
		private string namespaceURI;
		private bool isEmptyElement;
		private string value;

		private XmlNodeType saveNodeType;
		private string saveName;
		private string savePrefix;
		private string saveLocalName;
		private string saveNamespaceURI;
		private bool saveIsEmptyElement;

		private Hashtable attributes;
		private ArrayList orderedAttributes;
		private IEnumerator orderedAttributesEnumerator;

		private bool returnEntityReference;
		private string entityReferenceName;

		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 256;

		private char [] valueBuffer;
		private int valueLength;
		private int valueCapacity;
		private const int initialValueCapacity = 8192;

		private StringBuilder xmlBuffer; // This is for Read(Inner|Outer)Xml
		private StringBuilder currentTag; // A buffer for ReadContent for ReadOuterXml
		private bool saveToXmlBuffer;
		private int line = 1;
		private int column = 1;

		private void Init ()
		{
			readState = ReadState.Initial;

			depth = 0;
			depthDown = false;

			popScope = false;

			nodeType = XmlNodeType.None;
			name = String.Empty;
			prefix = String.Empty;
			localName = string.Empty;
			isEmptyElement = false;
			value = String.Empty;

			attributes = new Hashtable ();
			orderedAttributes = new ArrayList ();
			orderedAttributesEnumerator = null;

			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;

			valueBuffer = new char [initialValueCapacity];
			valueLength = 0;
			valueCapacity = initialValueCapacity;

			xmlBuffer = new StringBuilder ();
			currentTag = new StringBuilder ();
		}

		// Use this method rather than setting the properties
		// directly so that all the necessary properties can
		// be changed in harmony with each other. Maybe the
		// fields should be in a seperate class to help enforce
		// this.
		private void SetProperties (
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			this.nodeType = nodeType;
			this.name = name;
			this.isEmptyElement = isEmptyElement;
			this.value = value;
			this.elementDepth = depth;

			if (clearAttributes)
				ClearAttributes ();

			int indexOfColon = name.IndexOf (':');

			if (indexOfColon == -1) {
				prefix = String.Empty;
				localName = name;
			} else {
				prefix = name.Substring (0, indexOfColon);
				localName = name.Substring (indexOfColon + 1);
			}

			namespaceURI = LookupNamespace (prefix);
		}

		private void SaveProperties ()
		{
			saveNodeType = nodeType;
			saveName = name;
			savePrefix = prefix;
			saveLocalName = localName;
			saveNamespaceURI = namespaceURI;
			saveIsEmptyElement = isEmptyElement;
			// An element's value is always String.Empty.
		}

		private void RestoreProperties ()
		{
			nodeType = saveNodeType;
			name = saveName;
			prefix = savePrefix;
			localName = saveLocalName;
			namespaceURI = saveNamespaceURI;
			isEmptyElement = saveIsEmptyElement;
			value = String.Empty;
		}

		private void AddAttribute (string name, string value)
		{
			attributes.Add (name, value);
			orderedAttributes.Add (name);
		}

		private void ClearAttributes ()
		{
			if (attributes.Count > 0) {
				attributes.Clear ();
				orderedAttributes.Clear ();
			}

			orderedAttributesEnumerator = null;
		}

		private int PeekChar ()
		{
			return reader.Peek ();
		}

		private int ReadChar ()
		{
			int ch = reader.Read ();
			if (ch == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
			if (saveToXmlBuffer) {
				xmlBuffer.Append ((char) ch);
			}
			currentTag.Append ((char) ch);
			return ch;
		}

		// This should really keep track of some state so
		// that it's not possible to have more than one document
		// element or text outside of the document element.
		private bool ReadContent ()
		{
			currentTag.Length = 0;
			if (popScope) {
				parserContext.NamespaceManager.PopScope ();
				popScope = false;
			}

			if (returnEntityReference) {
				SetEntityReferenceProperties ();
			} else {
    			switch (PeekChar ())
				{
				case '<':
					ReadChar ();
					ReadTag ();
					break;
				case '\r':
					if (whitespaceHandling == WhitespaceHandling.All ||
					    whitespaceHandling == WhitespaceHandling.Significant)
						return ReadWhitespace ();

					ReadChar ();
					return ReadContent ();
				case '\n':
					if (whitespaceHandling == WhitespaceHandling.All ||
					    whitespaceHandling == WhitespaceHandling.Significant)
						return ReadWhitespace ();

					ReadChar ();
					return ReadContent ();
				case ' ':
					if (whitespaceHandling == WhitespaceHandling.All ||
					    whitespaceHandling == WhitespaceHandling.Significant)
						return ReadWhitespace ();

					SkipWhitespace ();
					return ReadContent ();
				case -1:
					readState = ReadState.EndOfFile;
					SetProperties (
						XmlNodeType.None, // nodeType
						String.Empty, // name
						false, // isEmptyElement
						String.Empty, // value
						true // clearAttributes
					);
					break;
				default:
					ReadText (true);
					break;
				}
			}

//			return (PeekChar () != -1);
			return this.ReadState != ReadState.EndOfFile;
		}

		private void SetEntityReferenceProperties ()
		{
			SetProperties (
				XmlNodeType.EntityReference, // nodeType
				entityReferenceName, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);

			returnEntityReference = false;
			entityReferenceName = String.Empty;
		}

		// The leading '<' has already been consumed.
		private void ReadTag ()
		{
			switch (PeekChar ())
			{
			case '/':
				ReadChar ();
				ReadEndTag ();
				break;
			case '?':
				ReadChar ();
				ReadProcessingInstruction ();
				break;
			case '!':
				ReadChar ();
				ReadDeclaration ();
				break;
			default:
				ReadStartTag ();
				break;
			}
		}

		// The leading '<' has already been consumed.
		private void ReadStartTag ()
		{
			parserContext.NamespaceManager.PushScope ();

			string name = ReadName ();
			SkipWhitespace ();

			bool isEmptyElement = false;

			ClearAttributes ();

			if (XmlChar.IsFirstNameChar (PeekChar ()))
				ReadAttributes ();

			if (PeekChar () == '/') {
				ReadChar ();
				isEmptyElement = true;
				depthDown = true;
				popScope = true;
			}

			Expect ('>');

			SetProperties (
				XmlNodeType.Element, // nodeType
				name, // name
				isEmptyElement, // isEmptyElement
				String.Empty, // value
				false // clearAttributes
			);

			if (!depthDown)
				++depth;
			else
				depthDown = false;

		}

		// The reader is positioned on the first character
		// of the element's name.
		private void ReadEndTag ()
		{
			string name = ReadName ();
			SkipWhitespace ();
			Expect ('>');

			--depth;

			SetProperties (
				XmlNodeType.EndElement, // nodeType
				name, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);

			popScope = true;
		}

		private void AppendNameChar (int ch)
		{
			CheckNameCapacity ();
			nameBuffer [nameLength++] = (char)ch;
		}

		private void CheckNameCapacity ()
		{
			if (nameLength == nameCapacity) {
				nameCapacity = nameCapacity * 2;
				char [] oldNameBuffer = nameBuffer;
				nameBuffer = new char [nameCapacity];
				Array.Copy (oldNameBuffer, nameBuffer, nameLength);
			}
		}

		private string CreateNameString ()
		{
			return new String (nameBuffer, 0, nameLength);
		}

		private void AppendValueChar (int ch)
		{
			CheckValueCapacity ();
			valueBuffer [valueLength++] = (char)ch;
		}

		private void CheckValueCapacity ()
		{
			if (valueLength == valueCapacity) {
				valueCapacity = valueCapacity * 2;
				char [] oldValueBuffer = valueBuffer;
				valueBuffer = new char [valueCapacity];
				Array.Copy (oldValueBuffer, valueBuffer, valueLength);
			}
		}

		private string CreateValueString ()
		{
			return new String (valueBuffer, 0, valueLength);
		}

		// The reader is positioned on the first character
		// of the text.
		private void ReadText (bool cleanValue)
		{
			if (cleanValue)
				valueLength = 0;

			int ch = PeekChar ();

			while (ch != '<' && ch != -1) {
				if (ch == '&') {
					ReadChar ();
					if (ReadReference (false))
						break;
				} else
					AppendValueChar (ReadChar ());

				ch = PeekChar ();
			}

			if (returnEntityReference && valueLength == 0) {
				SetEntityReferenceProperties ();
			} else {
				SetProperties (
					XmlNodeType.Text, // nodeType
					String.Empty, // name
					false, // isEmptyElement
					CreateValueString (), // value
					true // clearAttributes
				);
			}
		}

		// The leading '&' has already been consumed.
		// Returns true if the entity reference isn't a simple
		// character reference or one of the predefined entities.
		// This allows the ReadText method to break so that the
		// next call to Read will return the EntityReference node.
		private bool ReadReference (bool ignoreEntityReferences)
		{
			if (PeekChar () == '#') {
				ReadChar ();
				ReadCharacterReference ();
			} else
				ReadEntityReference (ignoreEntityReferences);

			return returnEntityReference;
		}

		private void ReadCharacterReference ()
		{
			int value = 0;

			if (PeekChar () == 'x') {
				ReadChar ();

				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = (value << 4) + ch - '0';
					else if (ch >= 'A' && ch <= 'F')
						value = (value << 4) + ch - 'A' + 10;
					else if (ch >= 'a' && ch <= 'f')
						value = (value << 4) + ch - 'a' + 10;
					else
						throw new XmlException (
							String.Format (
								"invalid hexadecimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
				}
			} else {
				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = value * 10 + ch - '0';
					else
						throw new XmlException (
							String.Format (
								"invalid decimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
				}
			}

			ReadChar (); // ';'

			AppendValueChar (value);
		}

		private void ReadEntityReference (bool ignoreEntityReferences)
		{
			nameLength = 0;

			int ch = PeekChar ();

			while (ch != ';' && ch != -1) {
				AppendNameChar (ReadChar ());
				ch = PeekChar ();
			}

			Expect (';');

			string name = CreateNameString ();

			switch (name)
			{
				case "lt":
					AppendValueChar ('<');
					break;
				case "gt":
					AppendValueChar ('>');
					break;
				case "amp":
					AppendValueChar ('&');
					break;
				case "apos":
					AppendValueChar ('\'');
					break;
				case "quot":
					AppendValueChar ('"');
					break;
				default:
					if (ignoreEntityReferences) {
						AppendValueChar ('&');

						foreach (char ch2 in name) {
							AppendValueChar (ch2);
						}

						AppendValueChar (';');
					} else {
						returnEntityReference = true;
						entityReferenceName = name;
					}
					break;
			}
		}

		// The reader is positioned on the first character of
		// the attribute name.
		private void ReadAttributes ()
		{
			do {
				string name = ReadName ();
				SkipWhitespace ();
				Expect ('=');
				SkipWhitespace ();
				string value = ReadAttribute ();
				SkipWhitespace ();

				if (name == "xmlns")
					parserContext.NamespaceManager.AddNamespace (String.Empty, value);
				else if (name.StartsWith ("xmlns:"))
					parserContext.NamespaceManager.AddNamespace (name.Substring (6), value);

				AddAttribute (name, value);
			} while (PeekChar () != '/' && PeekChar () != '>' && PeekChar () != -1);
		}

		// The reader is positioned on the quote character.
		private string ReadAttribute ()
		{
			int quoteChar = ReadChar ();

			if (quoteChar != '\'' && quoteChar != '\"')
				throw new XmlException ("an attribute value was not quoted");

			valueLength = 0;

			while (PeekChar () != quoteChar) {
				int ch = ReadChar ();

				switch (ch)
				{
				case '<':
					throw new XmlException ("attribute values cannot contain '<'");
				case '&':
					ReadReference (true);
					break;
				case -1:
					throw new XmlException ("unexpected end of file in an attribute value");
				default:
					AppendValueChar (ch);
					break;
				}
			}

			ReadChar (); // quoteChar

			return CreateValueString ();
		}

		// The reader is positioned on the first character
		// of the target.
		//
		// Now it also reads XmlDeclaration, this method name became improper...
		private void ReadProcessingInstruction ()
		{
			string target = ReadName ();
			SkipWhitespace ();

			valueLength = 0;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '?' && PeekChar () == '>') {
					ReadChar ();
					break;
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				target == "xml" ?
				XmlNodeType.XmlDeclaration :
				XmlNodeType.ProcessingInstruction, // nodeType
				target, // name
				false, // isEmptyElement
				CreateValueString (), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!'.
		private void ReadDeclaration ()
		{
			int ch = PeekChar ();

			switch (ch)
			{
			case '-':
				Expect ('-');
				Expect ('-');
				ReadComment ();
				break;
			case '[':
				ReadChar ();
				Expect ('C');
				Expect ('D');
				Expect ('A');
				Expect ('T');
				Expect ('A');
				Expect ('[');
				ReadCDATA ();
				break;
			}
		}

		// The reader is positioned on the first character after
		// the leading '<!--'.
		private void ReadComment ()
		{
			valueLength = 0;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '-' && PeekChar () == '-') {
					ReadChar ();

					if (PeekChar () != '>')
						throw new XmlException ("comments cannot contain '--'");

					ReadChar ();
					break;
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				CreateValueString (), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<![CDATA['.
		private void ReadCDATA ()
		{
			valueLength = 0;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == ']' && PeekChar () == ']') {
					ch = ReadChar (); // ']'

					if (PeekChar () == '>') {
						ReadChar (); // '>'
						break;
					} else {
						AppendValueChar (']');
						AppendValueChar (']');
						ch = ReadChar ();
					}
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.CDATA, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				CreateValueString (), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadName ()
		{
			if (!XmlChar.IsFirstNameChar (PeekChar ()))
				throw new XmlException ("a name did not start with a legal character");

			nameLength = 0;

			AppendNameChar (ReadChar ());

			while (XmlChar.IsNameChar (PeekChar ())) {
				AppendNameChar (ReadChar ());
			}

			return CreateNameString ();
		}

		// Read the next character and compare it against the
		// specified character.
		private void Expect (int expected)
		{
			int ch = ReadChar ();

			if (ch != expected) {
				throw new XmlException (
					String.Format (
						"expected '{0}' ({1:X}) but found '{2}' ({3:X})",
						(char)expected,
						expected,
						(char)ch,
						ch));
			}
		}

		// Does not consume the first non-whitespace character.
		private void SkipWhitespace ()
		{
			//FIXME: Should not skip if whitespaceHandling == WhiteSpaceHandling.None
			while (XmlChar.IsWhitespace (PeekChar ()))
				ReadChar ();
		}

		private bool ReadWhitespace ()
		{
			valueLength = 0;
			int ch = PeekChar ();
			do {
				AppendValueChar (ReadChar ());
			} while ((ch = PeekChar ()) != -1 && XmlChar.IsWhitespace (ch));

			if (ch != -1 && ch != '<')
				ReadText (false);
			else
				SetProperties (XmlNodeType.Whitespace,
					       String.Empty,
					       false,
					       CreateValueString (),
					       true);

			return (PeekChar () != -1);
		}
	}
}
