//
// System.IO.BinaryWriter
//
// Author:
//   Matt Kimball (matt@kimball.net)
//

using System;
using System.Text;
using System.Globalization;

namespace System.IO {
	[Serializable]
	public class BinaryWriter : IDisposable {

		// Null is a BinaryWriter with no backing store.
		public static readonly BinaryWriter Null;

		protected Stream OutStream;
		private Encoding m_encoding;
		private byte [] buffer;
		private bool disposed = false;

		static BinaryWriter() {
			Null = new BinaryWriter();
		}

		protected BinaryWriter() : this (Stream.Null, Encoding.UTF8Unmarked) {
		}

		public BinaryWriter(Stream output) : this(output, Encoding.UTF8Unmarked) {
		}

		public BinaryWriter(Stream output, Encoding encoding) {
			if (output == null || encoding == null) 
				throw new ArgumentNullException(Locale.GetText ("Output or Encoding is a null reference."));
			if (!output.CanWrite)
				throw new ArgumentException(Locale.GetText ("Stream does not support writing or already closed."));

			OutStream = output;
			m_encoding = encoding;
			buffer = new byte [16];
		}

		public virtual Stream BaseStream {
			get {
				return OutStream;
			}
		}

		public virtual void Close() {
			Dispose (true);
		}

		void IDisposable.Dispose() {
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing && OutStream != null)
				OutStream.Close();
			
			buffer = null;
			m_encoding = null;
			disposed = true;
		}

		public virtual void Flush() {
			OutStream.Flush();
		}

		public virtual long Seek(int offset, SeekOrigin origin) {

			return OutStream.Seek(offset, origin);
		}

		public virtual void Write(bool value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) (value ? 1 : 0);
			OutStream.Write(buffer, 0, 1);
		}

		public virtual void Write(byte value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			OutStream.WriteByte(value);
		}

		public virtual void Write(byte[] value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (value == null)
				throw new ArgumentNullException(Locale.GetText ("Byte buffer is a null reference."));
			OutStream.Write(value, 0, value.Length);
		}

		public virtual void Write(byte[] value, int offset, int length) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (value == null)
				throw new ArgumentNullException(Locale.GetText ("Byte buffer is a null reference."));
			OutStream.Write(value, offset, length);
		}

		public virtual void Write(char value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			char[] dec = new char[1];
			dec[0] = value;
			byte[] enc = m_encoding.GetBytes(dec, 0, 1);
			OutStream.Write(enc, 0, enc.Length);
		}
		
		public virtual void Write(char[] value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (value == null)
				throw new ArgumentNullException(Locale.GetText ("Chars is a null reference."));
			byte[] enc = m_encoding.GetBytes(value, 0, value.Length);
			OutStream.Write(enc, 0, enc.Length);
		}

		public virtual void Write(char[] value, int offset, int length) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			if (value == null)
				throw new ArgumentNullException(Locale.GetText ("Chars is a null reference."));
			byte[] enc = m_encoding.GetBytes(value, offset, length);
			OutStream.Write(enc, 0, enc.Length);
		}

		unsafe public virtual void Write(decimal value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			byte* value_ptr = (byte *)&value;
			for (int i = 0; i < 16; i++) {

				/*
				 * decimal in stream is lo32, mi32, hi32, ss32
				 * but its internal structure si ss32, hi32, lo32, mi32
				 */
				if (i < 4) 
					buffer [i + 12] = value_ptr [i];
				else if (i < 8)
					buffer [i + 4] = value_ptr [i];
				else if (i < 12)
					buffer [i - 8] = value_ptr [i];
				else 
					buffer [i - 8] = value_ptr [i];
			}

			OutStream.Write(buffer, 0, 16);
		}

		public virtual void Write(double value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			OutStream.Write(BitConverter.GetBytes(value), 0, 8);
		}
		
		public virtual void Write(short value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			OutStream.Write(buffer, 0, 2);
		}
		
		public virtual void Write(int value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			buffer [2] = (byte) (value >> 16);
			buffer [3] = (byte) (value >> 24);
			OutStream.Write(buffer, 0, 4);
		}

		public virtual void Write(long value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			for (int i = 0, sh = 0; i < 8; i++, sh += 8)
				buffer [i] = (byte) (value >> sh);
			OutStream.Write(buffer, 0, 8);
		}

		[CLSCompliant(false)]
		public virtual void Write(sbyte value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			OutStream.Write(buffer, 0, 1);
		}

		public virtual void Write(float value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			OutStream.Write(BitConverter.GetBytes(value), 0, 4);
		}

		public virtual void Write(string value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			/* The length field is the byte count, not the
			 * char count
			 */
			byte[] enc = m_encoding.GetBytes(value);
			Write7BitEncodedInt(enc.Length);
			OutStream.Write(enc, 0, enc.Length);
		}

		[CLSCompliant(false)]
		public virtual void Write(ushort value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			OutStream.Write(buffer, 0, 2);
		}

		[CLSCompliant(false)]
		public virtual void Write(uint value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			buffer [0] = (byte) value;
			buffer [1] = (byte) (value >> 8);
			buffer [2] = (byte) (value >> 16);
			buffer [3] = (byte) (value >> 24);
			OutStream.Write(buffer, 0, 4);
		}

		[CLSCompliant(false)]
		public virtual void Write(ulong value) {

			if (disposed)
				throw new ObjectDisposedException ("BinaryWriter", "Cannot write to a closed BinaryWriter");

			for (int i = 0, sh = 0; i < 8; i++, sh += 8)
				buffer [i] = (byte) (value >> sh);
			OutStream.Write(buffer, 0, 8);
		}

		protected void Write7BitEncodedInt(int value) {
			do {
				int high = (value >> 7) & 0x01ffffff;
				byte b = (byte)(value & 0x7f);

				if (high != 0) {
					b = (byte)(b | 0x80);
				}

				Write(b);
				value = high;
			} while(value != 0);
		}
	}
}
