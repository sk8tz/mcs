// StreamWriterTest.cs - NUnit Test Cases for the SystemIO.StreamWriter class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System.IO
{

public class StreamWriterTest : TestCase
{
	public StreamWriterTest() : 
		base ("MonoTests.System.StreamWriterTest testsuite") {}
	public StreamWriterTest(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	protected override void TearDown() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(StreamWriterTest)); 
		}
	}

	private string _codeFileName = "StreamWriterTest.temp";
	private string _thisCodeFileName = "resources" + Path.DirectorySeparatorChar + "AFile.txt";

	// TODO - ctors
	public void TestCtor1() {
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter((Stream)null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			FileStream f = new FileStream(_thisCodeFileName, 
						      FileMode.Open, 
						      FileAccess.Read);
			try {
				StreamWriter r = new StreamWriter(f);
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
			FileStream f = new FileStream(_codeFileName, 
						      FileMode.Append, 
						      FileAccess.Write);
			StreamWriter r = new StreamWriter(f);
			AssertNotNull("no stream writer", r);
			r.Close();
			f.Close();
		}
	}
	public void TestCtor2() {
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("");
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
				StreamWriter r = new StreamWriter((string)null);
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
				StreamWriter r = new StreamWriter("nonexistentdir/file");
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("!$what? what? Huh? !$*#");
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		// TODO - Security/Auth exceptions
		{
			StreamWriter r = new StreamWriter(_codeFileName);
			AssertNotNull("no stream writer", r);
			r.Close();
		}
	}
	public void TestCtor3() {
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("", false);
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
				StreamWriter r = new StreamWriter((string)null, false);
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
				StreamWriter r = new StreamWriter("nonexistentdir/file", false);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("!$what? what? Huh? !$*#", false);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			StreamWriter r = new StreamWriter(_codeFileName, false);
			AssertNotNull("no stream writer", r);
			r.Close();
		}
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("", true);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("empty string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter((string)null, true);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 6: " + e.ToString());
			}
			Assert("null string error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("nonexistentdir/file", true);
			} catch (DirectoryNotFoundException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 7: " + e.ToString());
			}
			Assert("dirNotFound error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				StreamWriter r = new StreamWriter("!$what? what? Huh? !$*#", true);
			} catch (IOException) {
				errorThrown = true;
			} catch (ArgumentException) {
				// FIXME - the spec says 'IOExc', but the
				//   compiler says 'ArgExc'...
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 8: " + e.ToString());
			}
			Assert("invalid filename error not thrown", errorThrown);
		}
		{
			try {
				StreamWriter r = new StreamWriter(_codeFileName, true);
				AssertNotNull("no stream writer", r);
				r.Close();
			} catch (Exception e) {
				Fail ("Unxpected exception e=" + e.ToString());
			}
		}
	}

	// TODO - ctors with Encoding

	// TODO - AutoFlush
	public void TestAutoFlush() {
		{
			MemoryStream m = new MemoryStream();
			StreamWriter w = new StreamWriter(m);
			w.AutoFlush = false;
			w.Write(1);
			w.Write(2);
			w.Write(3);
			w.Write(4);
			AssertEquals("Should be nothing before flush",
				     0L, m.Length);
			w.Flush();
			AssertEquals("Should be something after flush",
				     4L, m.Length);
		}		
		{
			MemoryStream m = new MemoryStream();
			StreamWriter w = new StreamWriter(m);
			w.AutoFlush = true;
			w.Write(1);
			w.Write(2);
			w.Write(3);
			w.Write(4);
			AssertEquals("Should be something before flush",
				     4L, m.Length);
			w.Flush();
			AssertEquals("Should be something after flush",
				     4L, m.Length);
		}		
	}

	public void TestBaseStream() {
		FileStream f = new FileStream(_codeFileName, 
					      FileMode.Append, 
					      FileAccess.Write);
		StreamWriter r = new StreamWriter(f);
		AssertEquals("wrong base stream ", f, r.BaseStream);
		r.Close();
		f.Close();
	}

	public void TestEncoding() {
		StreamWriter r = new StreamWriter(_codeFileName);
		AssertEquals("wrong encoding", 
			     Encoding.UTF8.GetType(), r.Encoding.GetType());
		r.Close();
	}

	// TODO - Close - not entirely sure how to test Close
	//public void TestClose() {
	//{
	//MemoryStream m = new MemoryStream();
	//StreamWriter w = new StreamWriter(m);
	//StreamReader r = new StreamReader(m);
	//w.Write(1);
	//w.Write(2);
	//w.Write(3);
	//w.Write(4);
	//AssertEquals("Should be nothing before close",
	//0, m.Length);
	//AssertEquals("Should be nothing in reader",
	//-1, r.Peek());
	//w.Close();
	//AssertEquals("Should be something after close",
	//1, r.Peek());
        //}		
	//}

	// TODO - Flush
	public void TestFlush() {
		{
			bool errorThrown = false;
			try {
				FileStream f = new FileStream(_codeFileName, 
							      FileMode.Append, 
							      FileAccess.Write);
				StreamWriter r = new StreamWriter(f);
				r.Close();
				r.Flush();
			} catch (ObjectDisposedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("can't flush closed error not thrown", errorThrown);
		}
		{
			MemoryStream m = new MemoryStream();
			StreamWriter w = new StreamWriter(m);
			w.Write(1);
			w.Write(2);
			w.Write(3);
			w.Write(4);
			AssertEquals("Should be nothing before flush",
				     0L, m.Length);
			w.Flush();
			AssertEquals("Should be something after flush",
				     4L, m.Length);
		}		
	}

	// TODO - Write - test errors, functionality tested in TestFlush.
}
}
