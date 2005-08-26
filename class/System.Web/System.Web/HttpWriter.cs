//
// System.Web.HttpWriter.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
	
namespace System.Web {
	
	public sealed class HttpWriter : TextWriter {
		HttpResponseStream output_stream;
		HttpResponse response;
		Encoding encoding;

		internal HttpWriter (HttpResponse response)
		{
			this.response = response;
			encoding = response.ContentEncoding;
			output_stream = response.output_stream;
		}

		public override Encoding Encoding {
			get {
				return encoding;
			}
		}

		internal void SetEncoding (Encoding new_encoding)
		{
			encoding = new_encoding;
		}

		public Stream OutputStream {
			get {
				return output_stream;
			}
		}

		//
		// Flush data, and closes socket
		//
		public override void Close ()
		{
			output_stream.Close ();
		}

		public override void Flush ()
		{
			output_stream.Flush ();
		}

		public override void Write (char ch)
		{
			Write (new string (ch, 1));
		}

		public override void Write (object obj)
		{
			if (obj == null)
				return;
			
			Write (obj.ToString ());
		}
		
		public override void Write (string s)
		{
			if (s == null)
				return;
			
			byte [] xx = encoding.GetBytes (s);

			output_stream.Write (xx, 0, xx.Length);
			
			if (response.buffer)
				return;

			response.Flush ();
		}
		
		public override void Write (char [] buffer, int index, int count)
		{
			byte [] xx = encoding.GetBytes (buffer, index, count);
			output_stream.Write (xx, 0, xx.Length);

			if (response.buffer)
				return;

			response.Flush ();
		}

		static byte [] newline = new byte [2] { 13, 10 };
		
		public override void WriteLine ()
		{
			output_stream.Write (newline, 0, 2);
			
			if (response.buffer)
				return;

			response.Flush ();
		}

		public void WriteString (string s, int index, int count)
		{
			char [] a = s.ToCharArray (index, count);

			byte [] xx = encoding.GetBytes (a, 0, count);
			
			output_stream.Write (xx, 0, xx.Length);

			if (response.buffer)
				return;

			response.Flush ();
		}

		public void WriteBytes (byte [] buffer, int index, int count)
		{
			output_stream.Write (buffer, index, count);

			if (response.buffer)
				return;

			response.Flush ();
		}
	}
}

