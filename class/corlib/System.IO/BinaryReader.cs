//
// System.IO.BinaryReader
//
// Author:
//   Matt Kimball (matt@kimball.net)
//   Dick Porter (dick@ximian.com)
//

using System;
using System.Text;
using System.Globalization;

namespace System.IO {
	public class BinaryReader : IDisposable {
		Stream m_stream;
		Encoding m_encoding;
		int m_encoding_max_byte;

		byte[] m_buffer;
		
		private bool m_disposed = false;

		public BinaryReader(Stream input) : this(input, Encoding.UTF8Unmarked) {
		}

		public BinaryReader(Stream input, Encoding encoding) {
			if (input == null || encoding == null) 
				throw new ArgumentNullException(Locale.GetText ("Input or Encoding is a null reference."));
			if (!input.CanRead)
				throw new ArgumentException(Locale.GetText ("The stream doesn't support reading."));

			m_stream = input;
			m_encoding = encoding;
			m_encoding_max_byte = m_encoding.GetMaxByteCount(1);
			m_buffer = new byte [32];
		}

		public virtual Stream BaseStream {
			get {
				return m_stream;
			}
		}

		public virtual void Close() {
			Dispose (true);
			m_disposed = true;
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing && m_stream != null)
				m_stream.Close ();

			m_disposed = true;
			m_buffer = null;
			m_encoding = null;
			m_stream = null;
		}

		void IDisposable.Dispose() 
		{
			Dispose (true);
		}

		protected virtual void FillBuffer(int bytes) {
			if(m_stream==null) {
				throw new IOException("Stream is invalid");
			}
			
			CheckBuffer(bytes);

			/* Cope with partial reads */
			int pos=0;
			while(pos<bytes) {
				int n=m_stream.Read(m_buffer, pos, bytes-pos);
				if(n==0) {
					throw new EndOfStreamException();
				}

				pos+=n;
			}
		}

		public virtual int PeekChar() {
			if(m_stream==null) {
				
				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}

			if(!m_stream.CanSeek) {
				return(-1);
			}

			long pos=m_stream.Position;
			int ch=Read();
			m_stream.Position=pos;

			return(ch);
		}

		public virtual int Read() {
			char[] decode = new char[1];

			int count=Read(decode, 0, 1);
			if(count==0) {
				/* No chars available */
				return(-1);
			}
			
			return decode[0];
		}

		public virtual int Read(byte[] buffer, int index, int count) {
			if(m_stream==null) {

				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}
			
			if (buffer == null) {
				throw new ArgumentNullException("buffer is null");
			}
			if (buffer.Length - index < count) {
				throw new ArgumentException("buffer is too small");
			}
			if (index < 0) {
				throw new ArgumentOutOfRangeException("index is less than 0");
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}

			int bytes_read=m_stream.Read(buffer, index, count);

			return(bytes_read);
		}

		public virtual int Read(char[] buffer, int index, int count) {
			if(m_stream==null) {

				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}
			
			if (buffer == null) {
				throw new ArgumentNullException("buffer is null");
			}
			if (buffer.Length - index < count) {
				throw new ArgumentException("buffer is too small");
			}
			if (index < 0) {
				throw new ArgumentOutOfRangeException("index is less than 0");
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}

			int chars_read=0;
			int bytes_read=0;
			
			while(chars_read < count) {
				CheckBuffer(bytes_read);

				int read_byte=m_stream.ReadByte();
				if(read_byte==-1) {
					/* EOF */
					return(chars_read);
				}

				m_buffer[bytes_read]=(byte)read_byte;
				bytes_read++;
				
				chars_read=m_encoding.GetChars(m_buffer, 0,
							       bytes_read,
							       buffer, index);
				
			}

			return(chars_read);
		}

		protected int Read7BitEncodedInt() {
			int ret = 0;
			int shift = 0;
			byte b;

			do {
				b = ReadByte();
				
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			} while ((b & 0x80) == 0x80);

			return ret;
		}

		public virtual bool ReadBoolean() {
			FillBuffer(1);

			// Return value:
			//  true if the byte is non-zero; otherwise false.
			return(m_buffer[0] != 0);
		}

		public virtual byte ReadByte() {
			FillBuffer(1);

			return(m_buffer[0]);
		}

		public virtual byte[] ReadBytes(int count) {
			if(m_stream==null) {

				if (m_disposed)
					throw new ObjectDisposedException ("BinaryReader", "Cannot read from a closed BinaryReader.");

				throw new IOException("Stream is invalid");
			}
			
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}

			/* Can't use FillBuffer() here, because it's OK to
			 * return fewer bytes than were requested
			 */

			byte[] buf = new byte[count];
			int pos=0;
			
			while(pos < count) {
				int n=m_stream.Read(buf, pos, count-pos);
				if(n==0) {
					/* EOF */
					break;
				}

				pos+=n;
			}
				
			if (pos!=count) {
				byte[] new_buffer=new byte[pos];
				Array.Copy(buf, new_buffer, pos);
				return(new_buffer);
			}
			
			return(buf);
		}

		public virtual char ReadChar() {
			int ch=Read();

			if(ch==-1) {
				throw new EndOfStreamException();
			}

			return((char)ch);
		}

		public virtual char[] ReadChars(int count) {
			if (count < 0) {
				throw new ArgumentOutOfRangeException("count is less than 0");
			}

			char[] full = new char[count];
			int chars = Read(full, 0, count);
			
			if (chars == 0) {
				throw new EndOfStreamException();
			} else if (chars != full.Length) {
				char[] ret = new char[chars];
				Array.Copy(full, 0, ret, 0, chars);
				return ret;
			} else {
				return full;
			}
		}

		unsafe public virtual decimal ReadDecimal() {
			FillBuffer(16);

			decimal ret;
			byte* ret_ptr = (byte *)&ret;
			for (int i = 0; i < 16; i++) {
				ret_ptr[i] = m_buffer[i];
			}

			return ret;
		}

		public virtual double ReadDouble() {
			FillBuffer(8);

			return(BitConverter.ToDouble(m_buffer, 0));
		}

		public virtual short ReadInt16() {
			FillBuffer(2);

			return((short) (m_buffer[0] | (m_buffer[1] << 8)));
		}

		public virtual int ReadInt32() {
			FillBuffer(4);

			return(m_buffer[0] | (m_buffer[1] << 8) |
			       (m_buffer[2] << 16) | (m_buffer[3] << 24));
		}

		public virtual long ReadInt64() {
			FillBuffer(8);

			uint ret_low  = (uint) (m_buffer[0]            |
			                       (m_buffer[1] << 8)  |
			                       (m_buffer[2] << 16) |
			                       (m_buffer[3] << 24)
			                       );
			uint ret_high = (uint) (m_buffer[4]        |
			                       (m_buffer[5] << 8)  |
			                       (m_buffer[6] << 16) |
			                       (m_buffer[7] << 24)
			                       );
			return (long) ((((ulong) ret_high) << 32) | ret_low);
		}

		[CLSCompliant(false)]
		public virtual sbyte ReadSByte() {
			FillBuffer(1);

			return((sbyte)m_buffer[0]);
		}

		public virtual string ReadString() {
			/* Inspection of BinaryWriter-written files
			 * shows that the length is given in bytes,
			 * not chars
			 */
			int len = Read7BitEncodedInt();

			FillBuffer(len);
			
			char[] str = m_encoding.GetChars(m_buffer, 0, len);

			return(new String(str));
		}

		public virtual float ReadSingle() {
			FillBuffer(4);

			return(BitConverter.ToSingle(m_buffer, 0));
		}

		[CLSCompliant(false)]
		public virtual ushort ReadUInt16() {
			FillBuffer(2);

			return((ushort) (m_buffer[0] | (m_buffer[1] << 8)));
		}

		[CLSCompliant(false)]
		public virtual uint ReadUInt32() {
			FillBuffer(4);
				

			return((uint) (m_buffer[0] |
				       (m_buffer[1] << 8) |
				       (m_buffer[2] << 16) |
				       (m_buffer[3] << 24)));
		}

		[CLSCompliant(false)]
		public virtual ulong ReadUInt64() {
			FillBuffer(8);

			uint ret_low  = (uint) (m_buffer[0]            |
			                       (m_buffer[1] << 8)  |
			                       (m_buffer[2] << 16) |
			                       (m_buffer[3] << 24)
			                       );
			uint ret_high = (uint) (m_buffer[4]        |
			                       (m_buffer[5] << 8)  |
			                       (m_buffer[6] << 16) |
			                       (m_buffer[7] << 24)
			                       );
			return (((ulong) ret_high) << 32) | ret_low;
		}

		/* Ensures that m_buffer is at least length bytes
		 * long, growing it if necessary
		 */
		private void CheckBuffer(int length)
		{
			if(m_buffer.Length <= length) {
				byte[] new_buffer=new byte[length];
				Array.Copy(m_buffer, new_buffer,
					   m_buffer.Length);
				m_buffer=new_buffer;
			}
		}
	}
}
