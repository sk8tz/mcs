//
// System.Resources.ResXResourceReader.cs
//
// Authors: 
// 	Duncan Mak <duncan@ximian.com>
//	Nick Drochak <ndrochak@gol.com>
//	Paolo Molaro <lupus@ximian.com>
//
// 2001 (C) Ximian Inc, http://www.ximian.com
//
// TODO: Finish this

using System.Collections;
using System.Resources;
using System.IO;
using System.Xml;

namespace System.Resources
{
	public sealed class ResXResourceReader : IResourceReader, IDisposable
	{
		Stream stream;
		XmlTextReader reader;
		Hashtable hasht;

		// Constructors
		public ResXResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value cannot be null.");
			
			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			this.stream = stream;
			basic_setup ();
		}
		
		public ResXResourceReader (string fileName)
		{
			if (fileName == null)
				throw new ArgumentException ("Path cannot be null.");
			
			if (String.Empty == fileName)
				throw new ArgumentException("Empty path name is not legal.");

			if (!System.IO.File.Exists (fileName)) 
				throw new FileNotFoundException ("Could not find file " + Path.GetFullPath(fileName));

			stream = new FileStream (fileName, FileMode.Open);
			basic_setup ();
		}

		void basic_setup () {
			reader = new XmlTextReader (stream);
			hasht = new Hashtable ();

			if (!IsStreamValid()){
				throw new ArgumentException("Stream is not a valid .resx file!  It was possibly truncated.");
			}
			load_data ();
		}
		
		static string get_attr (XmlTextReader reader, string name) {
			if (!reader.HasAttributes)
				return null;
			for (int i = 0; i < reader.AttributeCount; i++) {
				reader.MoveToAttribute (i);
				if (String.Compare (reader.Name, name, true) == 0) {
					string v = reader.Value;
					reader.MoveToElement ();
					return v;
				}
			}
			reader.MoveToElement ();
			return null;
		}

		static string get_value (XmlTextReader reader, string name) {
			bool gotelement = false;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, name, true) == 0) {
					gotelement = true;
					break;
				}
			}
			if (!gotelement)
				return null;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Text) {
					string v = reader.Value;
					return v;
				}
				else if (reader.NodeType == XmlNodeType.EndElement && reader.Value == string.Empty)
				{
					string v = reader.Value;
					return v;
				}
			}
			return null;
		}

		private bool IsStreamValid() {
			bool gotroot = false;
			bool gotmime = false;
			
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "root", true) == 0) {
					gotroot = true;
					break;
				}
			}
			if (!gotroot)
				return false;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "resheader", true) == 0) {
					string v = get_attr (reader, "name");
					if (v != null && String.Compare (v, "resmimetype", true) == 0) {
						v = get_value (reader, "value");
						if (String.Compare (v, "text/microsoft-resx", true) == 0) {
							gotmime = true;
							break;
						}
					}
				} else if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "data", true) == 0) {
					/* resheader apparently can appear anywhere, so we collect
					 * the data even if we haven't validated yet.
					 */
					string n = get_attr (reader, "name");
					if (n != null) {
						string v = get_value (reader, "value");
						hasht [n] = v;
					}
				}
			}
			return gotmime;
		}

		private void load_data ()
		{
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "data", true) == 0) {
					string n = get_attr (reader, "name");
					if (n != null) {
						string v = get_value (reader, "value");
						hasht [n] = v;
					}
				}
			}
		}

		public void Close ()
		{
			stream.Close ();
			stream = null;
		}
		
		public IDictionaryEnumerator GetEnumerator () {
			if (null == stream){
				throw new InvalidOperationException("ResourceReader is closed.");
			}
			else {
				return hasht.GetEnumerator ();
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator();
		}
		
		[MonoTODO]
		void IDisposable.Dispose ()
		{
			// FIXME: is this all we need to do?
			Close();
		}
		
	}  // public sealed class ResXResourceReader
} // namespace System.Resources
