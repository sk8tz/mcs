//
// System.Xml.Schema.XmlSchemaCollection.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto      ginga@kit.hi-ho.ne.jp
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
using System.Xml;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaCollection.
	///
	/// It is just a wrapper for XmlSchemaSet (unlike MS.NET, our 
	/// XmlSchemaCollection is originally designed to be conformant to 
	/// W3C specification).
	/// </summary>
#if NET_2_0
	[Obsolete ("Use XmlSchemaSet.")]
#endif
	public sealed class XmlSchemaCollection : ICollection, IEnumerable
	{
		//private fields
		private XmlSchemaSet schemaSet;

		public XmlSchemaCollection ()
			: this (new NameTable ())
		{
		}

		public XmlSchemaCollection (XmlNameTable nameTable)
			: this (new XmlSchemaSet (nameTable))
		{
		}

		internal XmlSchemaCollection (XmlSchemaSet schemaSet)
		{
			this.schemaSet = schemaSet;
		}

		//properties
		internal XmlSchemaSet SchemaSet {
			get { return schemaSet; }
		}

		public int Count {
			get { return schemaSet.Count; }
		}

		public XmlNameTable NameTable { 
			get { return schemaSet.NameTable; }
		}

		public XmlSchema this [ string ns ] { 
			get {
				ICollection col = schemaSet.Schemas (ns);
				if (col == null)
					return null;
				IEnumerator e = col.GetEnumerator ();
				if (e.MoveNext ())
					return (XmlSchema) e.Current;
				else
					return null;
			}
		}

		// Events
		public event ValidationEventHandler ValidationEventHandler;

		// Methods
		public XmlSchema Add (string ns, XmlReader reader)
		{
			return Add (ns, reader, new XmlUrlResolver ());
		}

#if NET_1_1
		public XmlSchema Add (string ns, XmlReader reader, XmlResolver resolver)
#else
		internal XmlSchema Add (string ns, XmlReader reader, XmlResolver resolver)
#endif
		{
			XmlSchema schema = XmlSchema.Read (reader, ValidationEventHandler);
			schema.Compile (ValidationEventHandler, schemaSet, resolver);
			lock (schemaSet) {
				return schemaSet.Add (schema);
			}
		}

		public XmlSchema Add (string ns, string uri)
		{
			lock (schemaSet) {
				return schemaSet.Add (ns, uri);
			}
		}

		public XmlSchema Add (XmlSchema schema)
		{
			return Add (schema, new XmlUrlResolver ());
		}

		public XmlSchema Add (XmlSchema schema, XmlResolver resolver)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			// XmlSchemaCollection.Add() compiles, while XmlSchemaSet.Add() does not
			if (!schema.IsCompiled)
				schema.Compile (ValidationEventHandler, schemaSet, resolver);
			// compilation error -> returns null (and don't add to the collection).
			if (!schema.IsCompiled)
				return null;

			lock (schemaSet) {
				// consider imported schemas.
				schemaSet.Add (schema.Schemas);
				return schema;
			}
		}

		private string GetSafeNs (string ns)
		{
			return ns != null ? ns : String.Empty;
		}

		public void Add (XmlSchemaCollection schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			lock (schemaSet) {
				schemaSet.Add (schema.schemaSet);
			}
		}

		public bool Contains (string ns)
		{
			lock (schemaSet) {
				return schemaSet.Contains (ns);
			}
		}

		public bool Contains (XmlSchema schema)
		{
			lock (schemaSet) {
				return schemaSet.Contains (schema);
			}
		}

		public void CopyTo (XmlSchema[] array, int index)
		{
			lock (schemaSet) {
				schemaSet.CopyTo (array, index);
			}
		}

		public XmlSchemaCollectionEnumerator GetEnumerator ()
		{
			return new XmlSchemaCollectionEnumerator (this);
		}

		// interface Methods
		void ICollection.CopyTo (Array array, int index)
		{
			lock (schemaSet) {
				schemaSet.CopyTo (array, index);
			}
		}

		bool ICollection.IsSynchronized
		{
			get { return true; } // always
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}

		Object ICollection.SyncRoot
		{
			get { return this; }
		}

		internal void OnValidationError (object o, ValidationEventArgs e)
		{
			if (ValidationEventHandler != null)
				ValidationEventHandler (o, e);
			else if (e.Severity == XmlSeverityType.Error)
				throw e.Exception;
		}

	}
}
