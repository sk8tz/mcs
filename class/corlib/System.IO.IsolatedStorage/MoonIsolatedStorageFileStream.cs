//
// System.IO.IsolatedStorage.MoonIsolatedStorageFileStream
//
// Moonlight's implementation for the IsolatedStorageFileStream
// 
// Authors
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2007, 2008 Novell, Inc (http://www.novell.com)
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
#if NET_2_1
using System;
using System.IO;

namespace System.IO.IsolatedStorage {

	// NOTES: 
	// * Silverlight allows extending to more than AvailableFreeSpace (by up to 1024 bytes).
	//   This looks like a safety buffer.

	[MonoTODO ("this needs to be quota-enabled")]
	public class IsolatedStorageFileStream : FileStream {

		IsolatedStorageFile container;

		internal static string Verify (IsolatedStorageFile isf, string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("path");
			if (isf == null)
				throw new ArgumentNullException ("isf");

			isf.PreCheck ();
			return isf.Verify (path);
		}

		public IsolatedStorageFileStream (string path, FileMode mode, IsolatedStorageFile isf)
			: base (Verify (isf, path), mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), 
				FileShare.Read, DefaultBufferSize, false, true)
		{
			container = isf;
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
			: base (Verify (isf, path), mode, access, FileShare.Read, DefaultBufferSize, false, true)
		{
			container = isf;
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
			: base (Verify (isf, path), mode, access, share, DefaultBufferSize, false, true)
		{
			container = isf;
		}

		protected override void Dispose (bool disposing)
		{
			// no PreCheck required
			base.Dispose (disposing);
		}

		public override void Flush ()
		{
			container.PreCheck ();
			base.Flush ();
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			container.PreCheck ();
			return base.Read (buffer, offset, count);
		}

		public override int ReadByte ()
		{
			container.PreCheck ();
			return base.ReadByte ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			container.PreCheck ();
			return base.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			container.PreCheck ();
			// if we're getting bigger then we must ensure we fit in the available free space of our container
			if (!container.CanExtend (value - Length))
				throw new IsolatedStorageException ();

			base.SetLength (value);
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			container.PreCheck ();
			// FIXME: check quota, if we grow the file
			base.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value)
		{
			container.PreCheck ();
			// if we are in position to grow the file make sure we can extend it by one byte
			if ((Position == Length) && !container.CanExtend (1))
				throw new IsolatedStorageException ();
			base.WriteByte (value);
		}

		public override bool CanRead {
			get {
				// no PreCheck required
				return base.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				// no PreCheck required
				return base.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				// no PreCheck required
				return base.CanWrite;
			}
		}

		public override long Length {
			get {
				container.PreCheck ();
				return base.Length;
			}
		}

		public override long Position {
			get {
				container.PreCheck ();
				return base.Position;
			}
			set {
				container.PreCheck ();
				base.Position = value;
			}
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			container.PreCheck ();
			return base.BeginRead (buffer, offset, numBytes, userCallback, stateObject);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			container.PreCheck ();
			// FIXME: check quota, if we grow the file
			return base.BeginWrite (buffer, offset, numBytes, userCallback, stateObject);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			container.PreCheck ();
			return base.EndRead (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			container.PreCheck ();
			base.EndWrite (asyncResult);
		}
	}
}
#endif
