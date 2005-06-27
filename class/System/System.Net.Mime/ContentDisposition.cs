//
// System.Net.Mail.ContentDisposition.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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

using System.Collections.Specialized;

namespace System.Net.Mime {
	public class ContentDisposition
	{
		#region Fields

		string disposition;
		DateTime creationDate;
		string dispositionType;
		string filename;
		bool inline;
		DateTime modificationDate;
		DateTime readDate;
		int size;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ContentDisposition ()
		{
		}

		[MonoTODO]
		public ContentDisposition (string disposition)
		{
			this.disposition = disposition;
		}

		#endregion // Constructors

		#region Properties

		public DateTime CreationDate {
			get { return creationDate; }
			set { creationDate = value; }
		}

		public string DispositionType {
			get { return dispositionType; }
			set { dispositionType = value; }
		}

		public string FileName {
			get { return filename; }
			set { filename = value; }
		}

		public bool Inline {
			get { return inline; }
			set { inline = value; }
		}

		public DateTime ModificationDate {
			get { return modificationDate; }
			set { modificationDate = value; } 
		}

		[MonoTODO]
		public StringDictionary Parameters {
			get { throw new NotImplementedException (); }
		}

		public DateTime ReadDate {
			get { return readDate; } 
			set { readDate = value; }
		}

		public int Size {
			get { return size; }
			set { size = value; }
		}


		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			return disposition;
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
