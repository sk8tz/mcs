// StreamReaderTest.cs - NUnit Test Cases for the SystemIO.StreamReader class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System.IO
{

[TestFixture]
public class StreamReaderTest : Assertion
{
	static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
	private string _codeFileName = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";

	[SetUp]
 	public void SetUp ()
	{	
		if (!Directory.Exists (TempFolder))				
			Directory.CreateDirectory (TempFolder);
		
		if (!File.Exists (_codeFileName))
			File.Create (_codeFileName).Close ();
	}

	[TearDown]
	public void TearDown ()
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}


	[Test]
	public void TestCtor1() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((Stream)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_codeFileName, FileMode.Open, FileAccess.Write);
			try {
				StreamReader r = new StreamReader(f);
				r.Close();
			} catch (ArgumentException) {
				errorThrown = true;
			}
			f.Close();
			Assert("no read error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f);
			AssertNotNull("no stream reader", r);
			r.Close();
			f.Close();
		}
	}

	[Test]
	public void TestCtor2() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("");
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((string)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentfile");
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("fileNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("nonexistentdir/file");
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0]);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName);
			AssertNotNull("no stream reader", r);
			r.Close();
		}
	}

	[Test]
	public void TestCtor3() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((Stream)null, false);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("null stream error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_codeFileName, FileMode.Open, FileAccess.Write);
			try {
				StreamReader r = new StreamReader(f, false);
				r.Close();
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			f.Close();
			Assert("no read error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f, false);
			AssertNotNull("no stream reader", r);
			r.Close();
			f.Close();
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((Stream)null, true);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_codeFileName, FileMode.Open, FileAccess.Write);
			try {
				StreamReader r = new StreamReader(f, true);
				r.Close();
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			f.Close();
			Assert("no read error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			StreamReader r = new StreamReader(f, true);
			AssertNotNull("no stream reader", r);
			r.Close();
			f.Close();
		}
	}

	[Test]
	public void TestCtor4() {
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("", false);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((string)null, false);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader(TempFolder + "/nonexistentfile", false);
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("fileNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader(TempFolder + "/nonexistentdir/file", false);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], false);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName, false);
			AssertNotNull("no stream reader", r);
			r.Close();
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("", true);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 6: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader((string)null, true);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 7: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader(TempFolder + "/nonexistentfile", true);
			} catch (FileNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 8: " + e.ToString());
			}
			Assert("fileNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader(TempFolder + "/nonexistentdir/file", true);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 9: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamReader r = new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], true);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 10: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			// this is probably incestuous, but, oh well.
			StreamReader r = new StreamReader(_codeFileName, true);
			AssertNotNull("no stream reader", r);
			r.Close();
		}
	}

	// TODO - Ctor with Encoding
	
	[Test]
	public void TestBaseStream() {
		string progress = "beginning";
		try {
			Byte[] b = {};
			MemoryStream m = new MemoryStream(b);
			StreamReader r = new StreamReader(m);
			AssertEquals("wrong base stream ", m, r.BaseStream);
			progress = "Closing StreamReader";
			r.Close();
			progress = "Closing MemoryStream";
			m.Close();
		} catch (Exception e) {
			Fail ("At '" + progress + "' an unexpected exception was thrown: " + e.ToString());
		}
	}

	public void TestCurrentEncoding() {
		try {
			Byte[] b = {};
			MemoryStream m = new MemoryStream(b);
			StreamReader r = new StreamReader(m);
			AssertEquals("wrong encoding", 
				     Encoding.UTF8.GetType (), r.CurrentEncoding.GetType ());
		} catch (Exception e) {
			Fail ("Unexpected exception thrown: " + e.ToString());
		}
	}

	// TODO - Close - annoying spec - won't commit to any exceptions. How to test?
	// TODO - DiscardBufferedData - I have no clue how to test this function.

	[Test]
	public void TestPeek() {
		// FIXME - how to get an IO Exception?
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				MemoryStream m = new MemoryStream(b);
				StreamReader r = new StreamReader(m);
				m.Close();
				int nothing = r.Peek();
			} catch (ObjectDisposedException) {
				errorThrown = true;
			}
			Assert("nothing-to-peek-at error not thrown", errorThrown);
		}
		{
			Byte[] b = {1, 2, 3, 4, 5, 6};
			MemoryStream m = new MemoryStream(b);
			
			StreamReader r = new StreamReader(m);
			for (int i = 1; i <= 6; i++) {
				AssertEquals("peek incorrect", i, r.Peek());
				r.Read();
			}
			AssertEquals("should be none left", -1, r.Peek());
		}
	}

	[Test]
	public void TestRead() {
		// FIXME - how to get an IO Exception?
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				MemoryStream m = new MemoryStream(b);
				StreamReader r = new StreamReader(m);
				m.Close();
				int nothing = r.Read();
			} catch (ObjectDisposedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("nothing-to-read error not thrown", errorThrown);
		}
		{
			Byte[] b = {1, 2, 3, 4, 5, 6};
			MemoryStream m = new MemoryStream(b);
			
			StreamReader r = new StreamReader(m);
			for (int i = 1; i <= 6; i++) {
				AssertEquals("read incorrect", i, r.Read());
			}
			AssertEquals("Should be none left", -1, r.Read());
		}

		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				r.Read(null, 0, 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("null buffer error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				Char[] c = new Char[1];
				r.Read(c, 0, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("too-long range error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				Char[] c = new Char[1];
				r.Read(c, -1, 2);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("out of range error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Byte[] b = {};
				StreamReader r = new StreamReader(new MemoryStream(b));
				Char[] c = new Char[1];
				r.Read(c, 0, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("out of range error not thrown", errorThrown);
		}
		{
			int ii = 1;
			try {
				Byte[] b = {(byte)'a', (byte)'b', (byte)'c', 
					    (byte)'d', (byte)'e', (byte)'f', 
					    (byte)'g'};
				MemoryStream m = new MemoryStream(b);
				ii++;
				StreamReader r = new StreamReader(m);
				ii++;

				char[] buffer = new Char[7];
				ii++;
				char[] target = {'g','d','e','f','b','c','a'};
				ii++;
				r.Read(buffer, 6, 1);
				ii++;
				r.Read(buffer, 4, 2);
				ii++;
				r.Read(buffer, 1, 3);
				ii++;
				r.Read(buffer, 0, 1);
				ii++;
				for (int i = 0; i < target.Length; i++) {
					AssertEquals("read no work", 
						     target[i], buffer[i]);
				i++;
				}
						    
			} catch (Exception e) {
				Fail ("Caught when ii=" + ii + ". e:" + e.ToString());
			}
		}
	}

	[Test]
	public void TestReadLine() {
		// TODO Out Of Memory Exc? IO Exc?
		Byte[] b = new Byte[8];
		b[0] = (byte)'a';
		b[1] = (byte)'\n';
		b[2] = (byte)'b';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", "a", r.ReadLine());
		AssertEquals("line doesn't match", "b", r.ReadLine());
		AssertEquals("line doesn't match", "c", r.ReadLine());
		AssertEquals("line doesn't match", "d", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	[Test]
	public void ReadLine1() {
		Byte[] b = new Byte[10];
		b[0] = (byte)'a';
		b[1] = (byte)'\r';
		b[2] = (byte)'b';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[5] = (byte)'\r';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		b[8] = (byte)'\r';
		b[9] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", "a", r.ReadLine());
		AssertEquals("line doesn't match", "b", r.ReadLine());
		AssertEquals("line doesn't match", "c", r.ReadLine());
		AssertEquals("line doesn't match", "d", r.ReadLine());
		AssertEquals("line doesn't match", "", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	[Test]
	public void ReadLine2() {
		Byte[] b = new Byte[10];
		b[0] = (byte)'\r';
		b[1] = (byte)'\r';
		b[2] = (byte)'\n';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[5] = (byte)'\r';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		b[8] = (byte)'\r';
		b[9] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", "", r.ReadLine());
		AssertEquals("line doesn't match", "", r.ReadLine());
		AssertEquals("line doesn't match", "", r.ReadLine());
		AssertEquals("line doesn't match", "c", r.ReadLine());
		AssertEquals("line doesn't match", "d", r.ReadLine());
		AssertEquals("line doesn't match", "", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	[Test]
	public void ReadLine3() {
		StringBuilder sb = new StringBuilder ();
		sb.Append (new string ('1', 32767));
		sb.Append ('\r');
		sb.Append ('\n');
		sb.Append ("Hola\n");
		byte [] bytes = Encoding.Default.GetBytes (sb.ToString ());
		MemoryStream m = new MemoryStream(bytes);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", new string ('1', 32767), r.ReadLine());
		AssertEquals("line doesn't match", "Hola", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	[Test]
	public void ReadLine4() {
		StringBuilder sb = new StringBuilder ();
		sb.Append (new string ('1', 32767));
		sb.Append ('\r');
		sb.Append ('\n');
		sb.Append ("Hola\n");
		sb.Append (sb.ToString ());
		byte [] bytes = Encoding.Default.GetBytes (sb.ToString ());
		MemoryStream m = new MemoryStream(bytes);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", new string ('1', 32767), r.ReadLine());
		AssertEquals("line doesn't match", "Hola", r.ReadLine());
		AssertEquals("line doesn't match", new string ('1', 32767), r.ReadLine());
		AssertEquals("line doesn't match", "Hola", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	[Test]
	public void ReadLine5() {
		StringBuilder sb = new StringBuilder ();
		sb.Append (new string ('1', 32768));
		sb.Append ('\r');
		sb.Append ('\n');
		sb.Append ("Hola\n");
		byte [] bytes = Encoding.Default.GetBytes (sb.ToString ());
		MemoryStream m = new MemoryStream(bytes);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", new string ('1', 32768), r.ReadLine());
		AssertEquals("line doesn't match", "Hola", r.ReadLine());
		AssertEquals("line doesn't match", null, r.ReadLine());
	}

	public void TestReadToEnd() {
		// TODO Out Of Memory Exc? IO Exc?
		Byte[] b = new Byte[8];
		b[0] = (byte)'a';
		b[1] = (byte)'\n';
		b[2] = (byte)'b';
		b[3] = (byte)'\n';
		b[4] = (byte)'c';
		b[5] = (byte)'\n';
		b[6] = (byte)'d';
		b[7] = (byte)'\n';
		MemoryStream m = new MemoryStream(b);
		StreamReader r = new StreamReader(m);
		AssertEquals("line doesn't match", "a\nb\nc\nd\n", r.ReadToEnd());
		AssertEquals("line doesn't match", "", r.ReadToEnd());
	}

	[Test]
	public void TestBaseStreamClosed ()
	{
		byte [] b = {};
		MemoryStream m = new MemoryStream (b);
		StreamReader r = new StreamReader (m);
		m.Close ();
		bool thrown = false;
		try {
			r.Peek ();
		} catch (ObjectDisposedException) {
			thrown = true;
		}

		AssertEquals ("#01", true, thrown);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Contructor_Stream_NullEncoding () 
	{
		StreamReader r = new StreamReader (new MemoryStream (), null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Contructor_Path_NullEncoding () 
	{
		StreamReader r = new StreamReader (_codeFileName, null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Read_Null () 
	{
		StreamReader r = new StreamReader (new MemoryStream ());
		r.Read (null, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_IndexOverflow () 
	{
		char[] array = new char [16];
		StreamReader r = new StreamReader (new MemoryStream (16));
		r.Read (array, 1, Int32.MaxValue);
	}	

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_CountOverflow () 
	{
		char[] array = new char [16];
		StreamReader r = new StreamReader (new MemoryStream (16));
		r.Read (array, Int32.MaxValue, 1);
	}

	[Test]
	public void Read_DoesntStopAtLineEndings ()
	{
		MemoryStream ms = new MemoryStream (Encoding.ASCII.GetBytes ("Line1\rLine2\r\nLine3\nLine4"));
		StreamReader reader = new StreamReader (ms);
		AssertEquals (24, reader.Read (new char[24], 0, 24));
	}

	[Test]
	public void bug75526 ()
	{
		StreamReader sr = new StreamReader (new Bug75526Stream ());
		int len = sr.Read (new char [10], 0, 10);
		AssertEquals (2, len);
	}

	class Bug75526Stream : MemoryStream
	{
		public override int Read (byte [] buffer, int offset, int count)
		{
			buffer [offset + 0] = (byte) 'a';
			buffer [offset + 1] = (byte) 'b';
			return 2;
		}
	}
}
}
