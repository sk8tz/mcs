//
// Mono.Unix/StdioFileStream.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005 Jonathan Pryor
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
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public class StdioFileStream : Stream, IDisposable
	{
		public static readonly IntPtr InvalidFileStream  = IntPtr.Zero;
		public static readonly IntPtr StandardInput  = Stdlib.stdin;
		public static readonly IntPtr StandardOutput = Stdlib.stdout;
		public static readonly IntPtr StandardError  = Stdlib.stderr;

		public StdioFileStream (IntPtr fileStream)
			: this (fileStream, true) {}

		public StdioFileStream (IntPtr fileStream, bool ownsHandle)
		{
			if (InvalidFileStream == fileStream)
				throw new ArgumentException (Locale.GetText ("Invalid file stream"), "fileStream");
			
			this.file = fileStream;
			this.owner = ownsHandle;
			
			long offset = Stdlib.fseek (file, 0, SeekFlags.SEEK_CUR);
			if (offset != -1)
				canSeek = true;
			Stdlib.fread (IntPtr.Zero, 0, 0, file);
			if (Stdlib.ferror (file) == 0)
				canRead = true;
			Stdlib.fwrite (IntPtr.Zero, 0, 0, file);
			if (Stdlib.ferror (file) == 0)
				canWrite = true;  
			Stdlib.clearerr (file);
		}

		private void AssertNotDisposed ()
		{
			if (file == InvalidFileStream)
				throw new ObjectDisposedException ("Invalid File Stream");
		}

		public IntPtr FileStream {
			get {return file;}
		}

		public override bool CanRead {
			get {return canRead;}
		}

		public override bool CanSeek {
			get {return canSeek;}
		}

		public override bool CanWrite {
			get {return canWrite;}
		}

		public override long Length {
			get {
				AssertNotDisposed ();
				if (!CanSeek)
					throw new NotSupportedException ("File Stream doesn't support seeking");
				long curPos = Stdlib.ftell (file);
				if (curPos == -1)
					throw new NotSupportedException ("Unable to obtain current file position");
				int r = Stdlib.fseek (file, 0, SeekFlags.SEEK_END);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);

				long endPos = Stdlib.ftell (file);
				if (endPos == -1)
					UnixMarshal.ThrowExceptionForLastError ();

				r = Stdlib.fseek (file, curPos, SeekFlags.SEEK_SET);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);

				return endPos - curPos;
			}
		}

		public override long Position {
			get {
				AssertNotDisposed ();
				if (!CanSeek)
					throw new NotSupportedException ("The stream does not support seeking");
				long pos = Stdlib.ftell (file);
				if (pos == -1)
					UnixMarshal.ThrowExceptionForLastError ();
				return (long) pos;
			}
			set {
				Seek (value, SeekOrigin.Begin);
			}
		}

		public FilePosition FilePosition {
			get {
				FilePosition pos = new FilePosition ();
				int r = Stdlib.fgetpos (file, pos);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
				return pos;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				int r = Stdlib.fsetpos (file, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public override void Flush ()
		{
			int r = Stdlib.fflush (file);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		public override unsafe int Read ([In, Out] byte[] buffer, int offset, int count)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
				 
			ulong r = 0;
			fixed (byte* buf = &buffer[offset]) {
				r = Stdlib.fread (buf, 1, (ulong) count, file);
			}
			if (r != (ulong) count) {
				if (Stdlib.feof (file) != 0)
					throw new EndOfStreamException ();
				if (Stdlib.ferror (file) != 0)
					throw new IOException ();
			}
			return (int) r;
		}

		private void AssertValidBuffer (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			if (offset > buffer.Length)
				throw new ArgumentException ("destination offset is beyond array size");
			if (offset > (buffer.Length - count))
				throw new ArgumentException ("would overrun buffer");
		}

		public void Rewind ()
		{
			Stdlib.rewind (file);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			AssertNotDisposed ();
			if (!CanSeek)
				throw new NotSupportedException ("The File Stream does not support seeking");

			SeekFlags sf = SeekFlags.SEEK_CUR;
			switch (origin) {
				case SeekOrigin.Begin:   sf = SeekFlags.SEEK_SET; break;
				case SeekOrigin.Current: sf = SeekFlags.SEEK_CUR; break;
				case SeekOrigin.End:     sf = SeekFlags.SEEK_END; break;
			}

			int r = Stdlib.fseek (file, offset, sf);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastError ();

			long pos = Stdlib.ftell (file);
			if (pos == -1)
				UnixMarshal.ThrowExceptionForLastError ();

			return pos;
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ("ANSI C doesn't provide a way to truncate a file");
		}

		public override unsafe void Write (byte[] buffer, int offset, int count)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanWrite)
				throw new NotSupportedException ("File Stream does not support writing");

			ulong r = 0;
			fixed (byte* buf = &buffer[offset]) {
				r = Stdlib.fwrite (buf, (ulong) 1, (ulong) count, file);
			}
			if (r != (ulong) count)
				UnixMarshal.ThrowExceptionForLastError ();
		}
		
		~StdioFileStream ()
		{
			Close ();
		}

		public override void Close ()
		{
			if (file == InvalidFileStream)
				return;
				
			Flush ();
			int r = Stdlib.fclose (file);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastError ();
			file = InvalidFileStream;
		}
		
		void IDisposable.Dispose ()
		{
			AssertNotDisposed ();
			GC.SuppressFinalize (this);
			if (owner) {
				Close ();
			}
		}

		private bool canSeek  = false;
		private bool canRead  = false;
		private bool canWrite = false;
		private bool owner    = true;
		private IntPtr file   = InvalidFileStream;
	}
}

// vim: noexpandtab
