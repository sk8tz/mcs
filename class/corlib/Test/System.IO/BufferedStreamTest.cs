//
// System.IO.BufferedStream
//
// Authors: 
//	Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2003 Ville Palo
//

using NUnit.Framework;
using System.IO;
using System.Text;
using System;

namespace MonoTests.System.IO {

[TestFixture]
public class BufferedStreamTest : Assertion {
	
	private MemoryStream mem;
	
        [SetUp]
        protected void SetUp ()
        {
		mem = new MemoryStream ();
        }

        [TearDown]
       	protected void TearDown ()
        {
		mem.Close ();        	
        }


	[Test]
	public void Ctor ()
	{
		MemoryStream str = new MemoryStream ();
		str.Write (new byte [] {1, 2, 3, 4, 5, 6}, 0, 6);
		BufferedStream stream = new BufferedStream (str);
		
		AssertEquals ("test#01", true, stream.CanRead);
		AssertEquals ("test#02", true, stream.CanSeek);
		AssertEquals ("test#03", true, stream.CanWrite);
		AssertEquals ("test#04", 6, stream.Length);
		AssertEquals ("test#05", 6, stream.Position);
		
		string path = Path.GetTempFileName ();
		if (File.Exists (path))
			File.Delete (path);
		
		FileStream file = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
		stream = new BufferedStream (file);		
		AssertEquals ("test#06", false, stream.CanRead);
		AssertEquals ("test#07", true, stream.CanSeek);
		AssertEquals ("test#08", true, stream.CanWrite);
		AssertEquals ("test#09", 0, stream.Length);
		AssertEquals ("test#10", 0, stream.Position);		
		file.Close ();
		
		if (File.Exists (path))
			File.Delete (path);
		
		file = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
		stream = new BufferedStream (file, 12);		
		AssertEquals ("test#11", false, stream.CanRead);
		AssertEquals ("test#12", true, stream.CanSeek);
		AssertEquals ("test#13", true, stream.CanWrite);
		AssertEquals ("test#14", 0, stream.Length);
		AssertEquals ("test#15", 0, stream.Position);		

		file.Close ();
		if (File.Exists (path))
			File.Delete (path);
	}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream () 
	{
		BufferedStream stream = new BufferedStream (null);
	}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream1 () 
	{
		BufferedStream stream = new BufferedStream (null, 12);
	}
	
	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void CtorOutOfRangeExceptionStream1 () 
	{
		MemoryStream str = new MemoryStream ();
		BufferedStream stream = new BufferedStream (str, -12);
	}


	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]
	public void CtorOutOfRangeException2 () 
	{
		MemoryStream str = new MemoryStream ();
		str.Close ();
		BufferedStream stream = new BufferedStream (str);		
	}
	
	[Test]	
	public void Close1 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		stream.Close ();
	}

	[Test]	
	public void Close2 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		AssertEquals ("test#01", false, stream.CanRead);
		AssertEquals ("test#02", false, stream.CanSeek);
		AssertEquals ("test#03", false, stream.CanWrite);
	}

	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close3 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		long l = stream.Position;
	}

	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close4 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		long l = stream.Length;
	}
	
	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close5 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		stream.WriteByte (1);
	}
	
	[Test]	
	[ExpectedException(typeof(ObjectDisposedException))]	
	public void Close6 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Close ();
		stream.ReadByte ();
	}

	[Test]	
	[ExpectedException(typeof(NotSupportedException))]	
	public void Close7 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		mem.Close ();
		stream.WriteByte (1);
	}
	
	[Test]
	public void Read ()
	{
		mem.Write (new byte [] {0, 1, 2, 3, 4, 5, 6, 7}, 0, 8);
		BufferedStream stream = new BufferedStream (mem);

		byte [] bytes = new byte [10];
		stream.Read (bytes, 0, 3);
		AssertEquals ("test#01", 0, bytes [0]);
		AssertEquals ("test#02", 0, bytes [1]);
		AssertEquals ("test#03", 0, bytes [2]);

		stream.Seek (0, SeekOrigin.Begin);
		stream.Read (bytes, 0, 3);
		AssertEquals ("test#04", 0, bytes [0]);
		AssertEquals ("test#05", 1, bytes [1]);
		AssertEquals ("test#06", 2, bytes [2]);
		AssertEquals ("test#07", 0, bytes [0]);		

		stream.Read (bytes, 5, 3);
		AssertEquals ("test#08", 3, bytes [5]);
		AssertEquals ("test#09", 4, bytes [6]);
		AssertEquals ("test#10", 5, bytes [7]);
		AssertEquals ("test#11", 0, bytes [8]);		

		stream.Read (bytes, 0, 10);
		AssertEquals ("test#12", 3, bytes [5]);
		AssertEquals ("test#13", 4, bytes [6]);
		AssertEquals ("test#14", 5, bytes [7]);
		AssertEquals ("test#15", 0, bytes [9]);				
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]		
	public void ReadException ()
	{
		mem.Write (new byte [] {0, 1, 2, 3, 4, 5, 6, 7}, 0, 8);
		BufferedStream stream = new BufferedStream (mem);

		byte [] bytes = new byte [10];
		stream.Read (bytes, 0, 30);		
	}

	[Test]
	[ExpectedException(typeof (ArgumentOutOfRangeException))]
	public void ReadException2 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		
		byte [] bytes = new byte [10];
		stream.Read (bytes, -10, 4);
	}
	
	[Test]
	public void ReadByte ()
	{
		mem.Write (new byte [] {0, 1, 2, 3, 4, 5, 6, 7}, 0, 8);
		BufferedStream stream = new BufferedStream (mem);

		AssertEquals ("test#01", -1, stream.ReadByte ());
		AssertEquals ("test#02", -1, stream.ReadByte ());
		AssertEquals ("test#03", -1, stream.ReadByte ());

		stream.Seek (0, SeekOrigin.Begin);
		AssertEquals ("test#04", 0, stream.ReadByte ());
		AssertEquals ("test#05", 1, stream.ReadByte ());
		AssertEquals ("test#06", 2, stream.ReadByte ());
		AssertEquals ("test#07", 3, stream.ReadByte ());		
	}
	
	[Test]
	public void Write ()
	{
		BufferedStream stream = new BufferedStream (mem);
		
		stream.Write (new byte [] {0, 1, 2, 3, 4}, 0, 4);
		AssertEquals ("test#01", 4, stream.Length);
		byte [] bytes = mem.GetBuffer ();
		AssertEquals ("test#02", 0, bytes [0]);
		AssertEquals ("test#03", 1, bytes [1]);
		AssertEquals ("test#04", 2, bytes [2]);
		AssertEquals ("test#05", 3, bytes [3]);

		bytes = new byte [] {1, 4, 3};
		stream.Write (bytes, 0, 3);
		stream.Flush ();
		bytes = mem.GetBuffer ();		
		AssertEquals ("test#06", 0, bytes [0]);
		AssertEquals ("test#07", 1, bytes [1]);
		AssertEquals ("test#08", 2, bytes [2]);
		AssertEquals ("test#09", 3, bytes [3]);
		AssertEquals ("test#10", 1, bytes [4]);
		AssertEquals ("test#11", 4, bytes [5]);
		AssertEquals ("test#10", 3, bytes [6]);
		AssertEquals ("test#11", 0, bytes [7]);
		AssertEquals ("test#12", 7, stream.Length);
	}
		
	[Test]
	[ExpectedException(typeof (ArgumentException))]
	public void WriteException ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0,1,2,3}, 0, 10);
	}

	[Test]
	[ExpectedException(typeof (ArgumentOutOfRangeException))]
	public void WriteException2 ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0,1,2,3}, -10, 4);
	}
	
	[Test]
	public void WriteByte ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.WriteByte (1);
		stream.WriteByte (2);
		stream.WriteByte (3);
		stream.Flush ();
		AssertEquals ("test#01", 256, mem.GetBuffer ().Length);
		AssertEquals ("test#02", 3, stream.Length);
		AssertEquals ("test#03", 1, mem.GetBuffer () [0]);
		AssertEquals ("test#04", 2, mem.GetBuffer () [1]);
		AssertEquals ("test#05", 3, mem.GetBuffer () [2]);		
	}
	
	[Test]
	public void Flush ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.WriteByte (1);
		stream.WriteByte (2);
		
		byte [] bytes = mem.GetBuffer ();
		AssertEquals ("test#01", 0, bytes.Length);
		stream.Flush ();
		
		bytes = mem.GetBuffer ();
		AssertEquals ("test#02", 256, bytes.Length);
		AssertEquals ("test#03", 1, bytes [0]);
		AssertEquals ("test#04", 2, bytes [1]);
		mem.Close ();
		mem = new MemoryStream ();
		bytes = new byte [] {0, 1, 2, 3, 4, 5};
		stream = new BufferedStream (mem);
		stream.Write (bytes, 0, 2);
		AssertEquals ("test#05", 2, stream.Length);
		bytes = mem.GetBuffer ();
		AssertEquals ("test#06", 256, bytes.Length);

		AssertEquals ("test#07", 0, bytes [0]);
		AssertEquals ("test#08", 1, bytes [1]);
		
		stream.Write (bytes, 0, 2);
		
		bytes = mem.GetBuffer ();
		AssertEquals ("test#09", 0, bytes [0]);
		AssertEquals ("test#10", 1, bytes [1]);
		AssertEquals ("test#11", 0, bytes [2]);
		AssertEquals ("test#12", 0, bytes [3]);
		stream.Flush ();
		bytes = mem.GetBuffer ();
		AssertEquals ("test#13", 0, bytes [2]);
		AssertEquals ("test#14", 1, bytes [3]);
	}
	
	[Test]
	public void Seek ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0, 1, 2, 3, 4, 5}, 0, 6);
		
		AssertEquals ("test#01", 6, stream.Position);
		
		stream.Seek (-5, SeekOrigin.End);		
		AssertEquals ("test#02", 1, stream.Position);
		
		stream.Seek (3, SeekOrigin.Current);
		AssertEquals ("test#03", 4, stream.Position);
		
		stream.Seek (300, SeekOrigin.Current);		
		AssertEquals ("test#04", 304, stream.Position);		
	}
	
	[Test]
	[ExpectedException(typeof (IOException))]
	public void SeekException ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Seek (-1, SeekOrigin.Begin);
	}
	
	[Test]
	public void SetLength ()
	{
		BufferedStream stream = new BufferedStream (mem);
		stream.Write (new byte [] {0,1,2,3,4,5}, 0, 6);
		
		AssertEquals ("test#01", 6, stream.Length);
		stream.SetLength (60);
		AssertEquals ("test#02", 60, stream.Length);
		
		stream.SetLength (2);
		AssertEquals ("test#03", 2, stream.Length);	
	}
	
	 [Test]
	 [ExpectedException(typeof(ArgumentOutOfRangeException))]
	 public void SetLengthException ()
	 {
		BufferedStream stream = new BufferedStream (mem);
		stream.SetLength (-1);	 	
	 }

	 [Test]
	 [ExpectedException(typeof(NotSupportedException))]
	 public void SetLengthException2 ()
	 {
		BufferedStream stream = new BufferedStream (mem);
	 	mem.Close ();
		stream.SetLength (1);	 	
	 }

	 [Test]
	 [ExpectedException(typeof(NullReferenceException))]
	 public void SetLengthException3 ()
	 {
		BufferedStream stream = new BufferedStream (mem);
	 	mem = null;
		stream.SetLength (1);	 	
	 }
	 
}
}
