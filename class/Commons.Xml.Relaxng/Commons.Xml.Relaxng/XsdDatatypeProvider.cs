//
// XsdDatatypeProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using XSchema = System.Xml.Schema.XmlSchema;

namespace Commons.Xml.Relaxng.XmlSchema
{
	public class XsdDatatypeProvider : RelaxngDatatypeProvider
	{
		static XsdDatatypeProvider instance = new XsdDatatypeProvider ();
		static Hashtable table;
		static XsdQNameWrapper qnameType = new XsdQNameWrapper ();

		private XsdDatatypeProvider ()
		{
			if (table != null)
				return;

			table = new Hashtable ();

			// TODO: fill all type names.
			string [] names = new string [] {
				"anySimpleType",
				"string",
				"normalizedString",
				"token",
				"language",
				"NMTOKEN", "NMTOKENS",
				"Name", "NCName",
				"ID", "IDREF", "IDREFS",
				"ENTITY", "ENTITIES", "NOTATION",
				"decimal", 
				"integer", "long", "int", "short", "byte",
				"nonPositiveInteger", "negativeInteger",
				"nonNegativeInteger", "positiveInteger",
				"unsignedLong", "unsignedInt", 
				"unsignedShort", "unsignedByte",
				"double", "float", 
				"base64Binary", "hexBinary",
				"boolean",
				"anyURI",
				"duration", "dateTime", "date", "time",
//				"QName",
				"gYearMonth", "gMonthDay",
				"gYear", "gMonth", "gDay",
			};

			StringBuilder sb = new StringBuilder ();
			sb.Append ("<xs:schema xmlns:xs='" + XSchema.Namespace + "'>");
			foreach (string name in names)
				sb.Append ("<xs:element name='" + name + "' type='xs:" + name + "'/>");
			sb.Append ("</xs:schema>");
			XSchema schema = XSchema.Read (new XmlTextReader (sb.ToString (), XmlNodeType.Document, null), null);
			schema.Compile (null);
			foreach (XmlSchemaElement el in schema.Elements.Values)
				table.Add (el.Name, new XsdPrimitiveType (el.Name, el.ElementType as XmlSchemaDatatype));
		}

		public static XsdDatatypeProvider Instance {
			get { return instance; }
		}

		public override RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters)
		{
			// TODO: parameter support

			switch (ns) {
			case System.Xml.Schema.XmlSchema.Namespace:
			case "http://www.w3.org/2001/XMLSchema-datatypes":
				break;
			default:
				return null;
			}
			if (name == "QName")
				return qnameType;
			return table [name] as RelaxngDatatype;
		}

	}

	public class XsdPrimitiveType : RelaxngDatatype
	{
		XmlSchemaDatatype dt;
		string name;

		public XsdPrimitiveType (string name, XmlSchemaDatatype xstype)
		{
			this.name = name;
			dt = xstype;
		}

		public override string Name {
			get { return name; }
		}

		public override string NamespaceURI {
			get { return "http://www.w3.org/2001/XMLSchema-datatypes"; }
		}

		public override object Parse (string text, XmlReader reader) 
		{
			return dt.ParseValue (text,
				reader != null ? reader.NameTable : null,
				null);
		}
	}

	// since QName resolution will fail, it must be implemented differently.
	public class XsdQNameWrapper : RelaxngDatatype
	{
		public XsdQNameWrapper ()
		{
		}

		public override string Name {
			get { return "QName"; }
		}

		public override string NamespaceURI {
			get { return "http://www.w3.org/2001/XMLSchema-datatypes"; }
		}

		public override object Parse (string s, XmlReader reader) 
		{
			int colonAt = s.IndexOf (':');
			string localName = colonAt < 0 ? s : s.Substring (colonAt + 1);
//			string localName = nameTable.Add (colonAt < 0 ? s : s.Substring (colonAt + 1));
			return new XmlQualifiedName (localName, reader.LookupNamespace (
				colonAt < 0 ? "" : s.Substring (0, colonAt - 1)));
		}

	}
}
