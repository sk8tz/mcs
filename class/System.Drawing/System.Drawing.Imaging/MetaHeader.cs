//
// System.Drawing.Imaging.MetaHeader.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[StructLayout (LayoutKind.Sequential)]
	public sealed class MetaHeader {

		// field order match: http://wvware.sourceforge.net/caolan/ora-wmf.html
		// for WMFHEAD structure
		private short file_type;
		private short header_size;
		private short version;
		private int file_size;
		private short num_of_objects;
		private int max_record_size;
		private short num_of_params;


		public MetaHeader ()
		{
		}


		public short HeaderSize {
			get { return header_size; }
			set { header_size = value; }
		}
		
		public int MaxRecord {
			get { return max_record_size; }
			set { max_record_size = value; }
		}
		
		public short NoObjects {
			get { return num_of_objects; }
			set { num_of_objects = value; }
		}
		
		public short NoParameters {
			get { return num_of_params; }
			set { num_of_params = value; }
		}
		
		public int Size {
			get { return file_size; }
			set { file_size = value; }
		}

		public short Type {
			get { return file_type; }
			set { file_type = value; }
		}

		public short Version {
			get { return version; }
			set { version = value; }
		}
	}
}
