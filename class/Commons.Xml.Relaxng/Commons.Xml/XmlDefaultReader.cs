//
// Commons.Xml.XmlDefaultReader.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// 2003 Atsushi Enomoto "No rights reserved."
//

//
// Similar to SAX DefaultHandler
//

using System;
using System.Xml;

namespace Commons.Xml
{
	public class XmlDefaultReader : XmlReader, IXmlLineInfo
	{
		XmlReader reader;
		IXmlLineInfo lineInfo;

		public XmlDefaultReader (XmlReader reader)
		{
			this.reader = reader;
			this.lineInfo = reader as IXmlLineInfo;
		}

		#region Properties
		// This is the only one non-overriden property.
		public XmlReader Reader {
			get { return reader; }
		}

		public int LineNumber {
			get { return lineInfo != null ? lineInfo.LineNumber : 0; }
		}
		
		public int LinePosition {
			get { return lineInfo != null ? lineInfo.LinePosition : 0; }
		}
		
		public override XmlNodeType NodeType 
		{
			get { return reader.NodeType; }
		}

		public override string Name {
			get { return reader.Name; }
		}

		public override string LocalName {
			get { return reader.LocalName; }
		}

		public override string NamespaceURI {
			get { return reader.NamespaceURI; }
		}

		public override string Prefix {
			get { return reader.Prefix; }
		}

		public override bool HasValue {
			get { return reader.HasValue; }
		}

		public override int Depth {
			get { return reader.Depth; }
		}

		public override string Value {
			get { return reader.Value; }
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		public override bool IsEmptyElement {
			get { return reader.IsEmptyElement; }
		}

		public override bool IsDefault {
			get { return reader.IsDefault; }
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override string XmlLang {
			get { return reader.XmlLang; }
		}

		public override XmlSpace XmlSpace {
			get { return reader.XmlSpace; }
		}

		public override int AttributeCount {
			get { return reader.AttributeCount; }
		}

		public override string this [int i] {
			get { return reader [i]; }
		}

		public override string this [string name] {
			get { return reader [name]; }
		}

		public override string this [string localName, string namespaceURI] {
			get { return reader [localName, namespaceURI]; }
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

		public override ReadState ReadState {
			get { return reader.ReadState; }
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}
		#endregion

		#region Methods

		public override string GetAttribute (string name)
		{
			return reader.GetAttribute (name);
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return reader.GetAttribute (localName, namespaceURI);
		}

		public override string GetAttribute (int i)
		{
			return reader.GetAttribute (i);
		}

		public bool HasLineInfo ()
		{
			return lineInfo != null ? lineInfo.HasLineInfo () : false;
		}

		public override bool MoveToAttribute (string name)
		{
			return reader.MoveToAttribute (name);
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			return reader.MoveToAttribute (localName, namespaceURI);
		}

		public override void MoveToAttribute (int i)
		{
			reader.MoveToAttribute (i);
		}

		public override bool MoveToFirstAttribute ()
		{
			return reader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			return reader.MoveToNextAttribute ();
		}

		public override bool MoveToElement ()
		{
			return reader.MoveToElement ();
		}

		public override void Close ()
		{
			reader.Close ();
		}

		public override bool Read ()
		{
			return Reader.Read ();
		}

		public override string ReadString ()
		{
			return reader.ReadString ();
		}

		public override string ReadInnerXml ()
		{
			return reader.ReadInnerXml ();
		}

		public override string ReadOuterXml ()
		{
			return reader.ReadOuterXml ();
		}

		public override string LookupNamespace (string prefix)
		{
			return reader.LookupNamespace (prefix);
		}

		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}

		public override bool ReadAttributeValue () {
			return reader.ReadAttributeValue ();
		}
		#endregion
	}
}
