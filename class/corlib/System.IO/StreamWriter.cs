//
// System.IO.StreamWriter.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Text;

namespace System.IO {
	
	[Serializable]
        public class StreamWriter : TextWriter {

		private Encoding internalEncoding;

		private Stream internalStream;
		private bool closed = false;

		private bool iflush;
		
                // new public static readonly StreamWriter Null;

		public StreamWriter (Stream stream)
			: this (stream, Encoding.UTF8, 0) {}

		public StreamWriter (Stream stream, Encoding encoding)
			: this (stream, encoding, 0) {}

		[MonoTODO("Nothing is done with bufferSize")]
		public StreamWriter (Stream stream, Encoding encoding, int bufferSize)
		{
			if (null == stream)
				throw new ArgumentNullException("stream");
			if (null == encoding)
				throw new ArgumentNullException("encoding");
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException("bufferSize");
			if (!stream.CanWrite)
				throw new ArgumentException("bufferSize");

			internalStream = stream;
			internalEncoding = encoding;
		}

		public StreamWriter (string path)
			: this (path, false, Encoding.UTF8, 0) {}

		public StreamWriter (string path, bool append)
			: this (path, append, Encoding.UTF8, 0) {}

		public StreamWriter (string path, bool append, Encoding encoding)
			: this (path, append, encoding, 0) {}
		
		public StreamWriter (string path, bool append, Encoding encoding, int bufferSize)
		{
			if (null == path)
				throw new ArgumentNullException("path");
			if (String.Empty == path)
				throw new ArgumentException("path cannot be empty string");
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException("path contains invalid characters");

			if (null == encoding)
				throw new ArgumentNullException("encoding");
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException("bufferSize");

			string DirName = Path.GetDirectoryName(path);
			if (DirName != String.Empty && !Directory.Exists(DirName))
				throw new DirectoryNotFoundException();

			FileMode mode;

			if (append)
				mode = FileMode.Append;
			else
				mode = FileMode.Create;
			
			internalStream = new FileStream (path, mode, FileAccess.Write);

			if (append)
				internalStream.Position = internalStream.Length;
			else
				internalStream.SetLength (0);

			internalEncoding = encoding;
			
		}

		public virtual bool AutoFlush
		{

			get {
				return iflush;
			}

			set {
				iflush = value;
			}
		}

		public virtual Stream BaseStream
		{
			get {
				return internalStream;
			}
		}

		public override Encoding Encoding
		{
			get {
				return internalEncoding;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && internalStream != null) {
				internalStream.Close ();
				internalStream = null;
			}
		}

		public override void Flush ()
		{
			if (closed)
				throw new ObjectDisposedException("TextWriter");

			internalStream.Flush ();
		}
		
		public override void Write (char[] buffer, int index, int count)
		{
			byte[] res = new byte [internalEncoding.GetMaxByteCount (buffer.Length)];
			int len;

			len = internalEncoding.GetBytes (buffer, index, count, res, 0);

			internalStream.Write (res, 0, len);

			if (iflush)
				Flush ();
			
		}

		public override void Write(string value)
		{
			Write (value.ToCharArray (), 0, value.Length);
		}

		public override void Close()
		{
			Dispose(true);
			closed = true;
		}
        }
}
                        
                        
