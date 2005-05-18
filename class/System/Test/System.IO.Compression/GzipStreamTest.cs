/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
//
// GZipStreamTest.cs - NUnit Test Cases for the System.IO.Compression.GZipStream class
//
// Authors:
// 	Christopher James Lahey  <clahey@ximian.com>
//
// (C) 2004 Novell, Inc. <http://www.novell.com>
// 

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;

namespace MonoTests.System.IO.Compression
{
	[TestFixture]
	public class GZipStreamTest : Assertion
	{
		private static void CopyStream (Stream src, Stream dest)
		{
			byte[] array = new byte[1024];
			int bytes_read;
			bytes_read = src.Read (array, 0, 1024);
			while (bytes_read != 0) {
				dest.Write (array, 0, bytes_read);
				bytes_read = src.Read (array, 0, 1024);
			}
		}

		private static bool compare_buffers (byte[] first, byte[] second, int length)
		{
			if (first.Length < length || second.Length < length) {
				return false;
			}
			for (int i = 0; i < length; i++) {
				if (first[i] != second[i]) {
					return false;
				}
			}
			return true;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null ()
		{
			GZipStream ds = new GZipStream (null, CompressionMode.Compress);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_InvalidCompressionMode ()
		{
			GZipStream ds = new GZipStream (new MemoryStream (), (CompressionMode)Int32.MinValue);
		}

		[Test]
		[Category("NotWorking")] // #72143
		public void CheckCompressDecompress () {
			byte [] data = new byte[100000];
			for (int i = 0; i < 100000; i++) {
				data[i] = (byte) i;
			}
			MemoryStream dataStream = new MemoryStream (data);
			MemoryStream backing = new MemoryStream ();
			GZipStream compressing = new GZipStream (backing, CompressionMode.Compress, true);
			CopyStream (dataStream, compressing);
			dataStream.Close();
			compressing.Close();
			backing.Seek (0, SeekOrigin.Begin);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			MemoryStream output = new MemoryStream ();
			CopyStream (decompressing, output);
			Assert (compare_buffers (data, output.GetBuffer(), (int) output.Length));
			decompressing.Close();
			output.Close();
		}

		[Test]
		public void CheckDecompress () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			StreamReader reader = new StreamReader (decompressing);
			AssertEquals (reader.ReadLine (), "Hello");
			decompressing.Close();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckNullRead () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Read (null, 0, 20);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CheckCompressingRead () {
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream ();
			GZipStream compressing = new GZipStream (backing, CompressionMode.Compress);
			compressing.Read (dummy, 0, 20);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CheckRangeRead () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Read (dummy, 10, 20);
		}

		[Test]
		[ExpectedException (typeof (InvalidDataException))]
		public void CheckInvalidDataRead () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0x11, 0x78, 0x89, 0x91, 0xbe, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Read (dummy, 0, 20);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void CheckClosedRead () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Close ();
			decompressing.Read (dummy, 0, 20);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		[Category("NotWorking")] // #72143
		public void CheckClosedFlush () {
			MemoryStream backing = new MemoryStream ();
			GZipStream compressing = new GZipStream (backing, CompressionMode.Compress);
			compressing.Close ();
			compressing.Flush ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSeek () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Seek (20, SeekOrigin.Current);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLength () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		public void CheckGetCanSeekProp () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			AssertEquals (false, decompressing.CanSeek);
		}

		[Test]
		public void CheckGetCanReadProp () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			AssertEquals (true, decompressing.CanRead);
		}

		[Test]
		public void CheckGetCanWriteProp () {
			MemoryStream backing = new MemoryStream ();
			GZipStream compressing = new GZipStream (backing, CompressionMode.Decompress);
			AssertEquals (false, compressing.CanWrite);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLengthProp () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetLengthProp () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			long length = decompressing.Length;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetPositionProp () {
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			long position = decompressing.Position;
		}
	}
}

#endif
