//
// DataContractJsonSerializer.cs (for Moonlight profile)
//
// Authors:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace System.Runtime.Serialization.Json {

	public sealed class DataContractJsonSerializer {

		private Type type;
		private ReadOnlyCollection<Type> known_types;

		public DataContractJsonSerializer (Type type) :
			this (type, null)
		{
		}

		[MonoTODO ("The 'knownTypes' parameter is unused (except as a property)")]
		public DataContractJsonSerializer (Type type, IEnumerable<Type> knownTypes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			this.type = type;
			List<Type> types = new List<Type> ();
			if (knownTypes != null)
				types.AddRange (knownTypes);
			known_types = new ReadOnlyCollection<Type> (types);
		}

		public ReadOnlyCollection<Type> KnownTypes {
			get { return known_types; }
		}

		// used by JsonSerializationReader.cs
		internal int MaxItemsInObjectGraph {
			get { return Int32.MaxValue; }
		}

		public object ReadObject (Stream stream)
		{
			var r = (JsonReader) JsonReaderWriterFactory.CreateJsonReader (stream, XmlDictionaryReaderQuotas.Max);
			r.LameSilverlightLiteralParser = true;

			try {
				r.MoveToContent ();
				if (!r.IsStartElement ("root", String.Empty)) {
					throw new SerializationException (
						String.Format ("Expected element was '{0}', but the actual input element was '{1}' in namespace '{2}'", 
						"root", r.LocalName, r.NamespaceURI));
				}
				return new JsonSerializationReader (this, r, type, true).ReadRoot ();
			} catch (SerializationException) {
				throw;
			} catch (Exception ex) {
				throw new SerializationException ("Deserialization has failed", ex);
			}
		}

		public void WriteObject (Stream stream, object graph)
		{
			using (var writer = JsonReaderWriterFactory.CreateJsonWriter (stream)) {
				try {
					writer.WriteStartElement ("root");
					new JsonSerializationWriter (this, writer, type, false).WriteObjectContent (graph, true, false);
					writer.WriteEndElement ();
				} catch (NotImplementedException) {
					throw;
				} catch (InvalidDataContractException) {
					throw;
				} catch (Exception ex) {
					throw new SerializationException (String.Format ("There was an error during serialization for object of type {0}", graph != null ? graph.GetType () : null), ex);
				}
			}
		}
	}
}

