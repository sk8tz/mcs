//
// System.Data.ObjectSpaces.Schema.ObjectSchema.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System.Data.Mapping;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace System.Data.ObjectSpaces.Schema {
	public class ObjectSchema : ICloneable, IDomainSchema
	{
		#region Fields

		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ObjectSchema ()
		{
		}

		[MonoTODO]
		public ObjectSchema (string url)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public SchemaClassCollection Classes {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		MappingDataSourceType IDomainSchema.DomainType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("Verify")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[MonoTODO]
		public string Namespace {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ObjectRelationshipCollection Relationships {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceUri {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public Object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetSchemaXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IDomainConstraint IDomainSchema.GetDomainConstraint (string select, IXmlNamespaceResolver namespaces)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IDomainStructure IDomainSchema.GetDomainStructure (string select, IXmlNamespaceResolver namespaces)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Read (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Read (string url, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Read (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Read (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.ReadExtensions (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.ReadExtensions (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (string schemaLocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (string schemaLocation, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (XmlWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (Stream stream, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.Write (TextWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.WriteExtensions (XmlWriter reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDomainSchema.WriteExtensions (XmlWriter reader, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (string url, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Reset ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string schemaLocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string schemaLocation, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
