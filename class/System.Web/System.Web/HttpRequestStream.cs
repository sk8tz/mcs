// 
// System.Web.HttpRequestStream
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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
using System;
using System.IO;

namespace System.Web
{
	class HttpRequestStream : Stream
	{
		byte [] _arrData;
		int _iLength;
		int _iOffset;
		int _iPos;

		internal HttpRequestStream ()
		{
		}

		internal HttpRequestStream (byte [] buffer, int offset, int length)
		{
			Set (buffer, offset, length);
		}

		void Reset ()
		{
			Set (null, 0, 0);
		}

		internal void Set (byte [] buffer, int offset, int length)
		{
			_iPos = 0;
			_iOffset = offset;
			_iLength = length;
			_arrData = buffer;
		}

		public override void Flush ()
		{
		}

		public override void Close ()
		{
			Reset ();
		}

		public override int Read (byte [] buffer, int offset, int length)
		{
			int iBytes = length;

			if (_iPos < _iOffset)
				_iPos = _iOffset;

			if (_iPos + length > _iOffset + _iLength) {
				iBytes = (int) _iOffset + _iLength - _iPos;
			}

			if (iBytes <= 0) {
				return 0;
			}

			Buffer.BlockCopy (_arrData, _iPos, buffer, offset, iBytes);
			_iPos += iBytes;

			return iBytes;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			switch (origin) {
			case SeekOrigin.Begin:
				if (offset > _arrData.Length) {
					throw new ArgumentException ();
				}
				_iPos = (int) offset;
				break;
			case SeekOrigin.Current:
				if (((long) _iPos + offset > _arrData.Length) || (_iPos + (int) offset < 0)) {
					throw new ArgumentException ();
				}
				_iPos += Convert.ToInt32 (offset);
				break;

			case SeekOrigin.End:
				if (_arrData.Length - offset < 0) {
					throw new ArgumentException();
				}
								
				_iPos = Convert.ToInt32 ( _arrData.Length - offset);
				break;
			}

			return (long) _iPos;										
		}

		public override void SetLength (long length)
		{
			throw new NotSupportedException ();
		}

		public override void Write (byte [] buffer, int offset, int length)
		{
			throw new NotSupportedException ();
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override bool CanWrite {
			get { return false; }
		}

		public byte [] Data {
			get { return _arrData; }
		}

		public int DataLength {
			get { return _iLength; }
		}

		public int DataOffset {
			get { return _iOffset; }
		}

		public override long Length {
			get { return (long) _arrData.Length; }
		}

		public override long Position {
			get { return (long) _iPos; }

			set { Seek (value, SeekOrigin.Begin); }
		}
	}
}

