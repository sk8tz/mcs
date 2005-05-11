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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ClassInterface(ClassInterfaceType.None)]
	public class DataObject : IDataObject {
		#region DataObject.Entry Class
		private class Entry {
			#region Local Variables
			private static Entry	entries;
			private string		type;
			private object		data;
			private bool		autoconvert;
			private Entry		next;
			#endregion	// Local Variables

			#region Constructors
			internal Entry(string type, object data, bool autoconvert) {
				Entry	e;

				this.type = type;
				this.data = data;
				this.autoconvert = autoconvert;

				lock (typeof(DataObject.Entry)) {
					if (entries == null) {
						entries = this;
					} else {
						// Insert into the list of known/defined formats
						e = entries;

						while (e.next != null) {
							e = e.next;
						}
						e.next = this;
					}
				}
			}
			#endregion	// Constructors

			#region Properties
			public static int Count {
				get {
					int	result;
					Entry	e;

					result = 0;
					e = entries;

					while (e != null) {
						result++;
						e = e.next;
					}

					return result;
				}
			}

			public object Data {
				get {
					return data;
				}
			}
			#endregion	// Properties

			#region Methods
			public static Entry Find(string type) {
				Entry e;

				e = entries;
				while (e != null) {
					if (e.type.Equals(type)) {
						return e;
					}
					e = e.next;
				}

				return null;
			}

			public static Entry FindConvertible(string type) {
				Entry e;

				e = Find(type);
				if (e != null) {
					return e;
				}

				e = entries;
				while (e != null) {
					if (type == DataFormats.Text) {
						if (e.type == DataFormats.UnicodeText) {
							return e;
						}
					} else if (type == DataFormats.UnicodeText) {
						if (e.type == DataFormats.Text) {
							return e;
						}
					} else if (type == DataFormats.StringFormat) {
						if (e.type == DataFormats.Text) {
							return e;
						} else if (e.type == DataFormats.UnicodeText) {
							return e;
						}
					}
					e = e.next;
				}

				return null;
			}

			public static string[] Entries(bool convertible) {
				Entry		e;
				ArrayList	list;
				string[]	result;

				// Initially store into something that we can grow easily
				list = new ArrayList(Entry.Count);
				e = entries;

				while (e != null) {
					list.Add(e.type);
					e = e.next;
				}

				if (convertible) {
					// Add the convertibles
					if ((Entry.Find(DataFormats.Text) != null) && (Entry.Find(DataFormats.UnicodeText) == null)) {
						list.Add(DataFormats.UnicodeText);
					}

					if ((Entry.Find(DataFormats.Text) == null) && (Entry.Find(DataFormats.UnicodeText) != null)) {
						list.Add(DataFormats.Text);
					}

					if (((Entry.Find(DataFormats.Text) != null) || (Entry.Find(DataFormats.UnicodeText) != null)) && (Entry.Find(DataFormats.StringFormat) == null)) {
						list.Add(DataFormats.StringFormat);
					}
				}

				// Copy the results into a string array
				result = new string[list.Count];
				for (int i = 0; i < list.Count; i++) {
					result[i] = (string)list[i];
				}

				return result;
			}
			#endregion	// Methods
		}
		#endregion	// DataObject.Entry class

		#region Local Variables
		#endregion	// Local Variables

		#region Public Constructors
		public DataObject() {
		}

		public DataObject(object data) {
			SetData(data);
		}

		public DataObject(string format, object data) {
			SetData(format, data);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public virtual object GetData(string format) {
			return GetData(format, true);
		}

		public virtual object GetData(string format, bool autoConvert) {
			if (autoConvert) {
				return Entry.FindConvertible(format).Data;
			} else {
				return Entry.Find(format).Data;
			}
		}

		public virtual object GetData(Type format) {
			return GetData(format.FullName, true);
		}

		public virtual bool GetDataPresent(string format) {
			return GetDataPresent(format, true);
		}

		public virtual bool GetDataPresent(string format, bool autoConvert) {
			if (autoConvert) {
				return Entry.FindConvertible(format) != null;
			} else {
				return Entry.Find(format) != null;
			}
		}

		public virtual bool GetDataPresent(Type format) {
			return GetDataPresent(format.FullName, true);
		}

		public virtual string[] GetFormats() {
			return GetFormats(true);
		}

		public virtual string[] GetFormats(bool autoConvert) {
			return Entry.Entries(autoConvert);
		}

		public virtual void SetData(object data) {
			SetData(data.GetType(), data); 
		}

		public virtual void SetData(string format, bool autoConvert, object data) {
			new DataObject.Entry(format, data, autoConvert);
		}

		public virtual void SetData(string format, object data) {
			SetData(format, true, data);
		}

		public virtual void SetData(Type format, object data) {
			SetData(EnsureFormat(format), true, data);
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		#endregion	// Public Instance Methods

		#region Private Methods
		internal string EnsureFormat(string name) {
			DataFormats.Format f;

			f = DataFormats.Format.Find(name);
			if (f == null) {
				// Register the format
				f = DataFormats.Format.Add(name);
			}

			return f.Name;
		}

		internal string EnsureFormat(Type type) {
			return EnsureFormat(type.FullName);
		}

		#endregion	// Private Methods
	}
}
