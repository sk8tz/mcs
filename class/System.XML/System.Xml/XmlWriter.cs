//
// System.Xml.XmlTextWriter
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;

namespace System.Xml
{
	public abstract class XmlWriter
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

		public abstract void Flush ();

		public abstract string LookupPrefix (string ns);

		[MonoTODO]
		public virtual void WriteAttributes (XmlReader reader, bool defattr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteAttributeString (string localName, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteAttributeString (string localName, string ns, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteAttributeString (string prefix, string localName, string ns, string value)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public virtual void WriteNode (XmlReader reader, bool defattr)
		{
			throw new NotImplementedException ();
		}

		public abstract void WriteProcessingInstruction (string name, string text);

		public abstract void WriteQualifiedName (string localName, string ns);

		public abstract void WriteRaw (string data);

		public abstract void WriteRaw (char[] buffer, int index, int count);

		[MonoTODO]
		public void WriteStartAttribute (string localName, string ns)
		{
			throw new NotImplementedException ();
		}

		public abstract void WriteStartAttribute (string prefix, string localName, string ns);

		public abstract void WriteStartDocument ();

		public abstract void WriteStartDocument (bool standalone);

		public void WriteStartElement (string localName)
		{
			WriteStartElement ("", localName, "");
		}

		public void WriteStartElement (string localName, string ns)
		{
			WriteStartElement ("", localName, ns);
		}

		public abstract void WriteStartElement (string prefix, string localName, string ns);

		public abstract void WriteString (string text);

		public abstract void WriteSurrogateCharEntity (char lowChar, char highChar);

		public abstract void WriteWhitespace (string ws);

		#endregion
	}
}
