//
// PrivateKeyTest.cs - NUnit Test Cases for Private Key File
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Text;

using Mono.Security.Authenticode;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Authenticode {

// HOWTO create a PVK file (on Windows using MS tools)
// makecert -n "CN=PVK1" -sv 1.pvk 1.cer

[TestFixture]
public class PrivateKeyTest {

	// because most crypto stuff works with byte[] buffers
	static public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		if ((array1 == null) && (array2 == null))
			return;
		if (array1 == null)
			Assertion.Fail (msg + " -> First array is NULL");
		if (array2 == null)
			Assertion.Fail (msg + " -> Second array is NULL");

		bool a = (array1.Length == array2.Length);
		if (a) {
			for (int i = 0; i < array1.Length; i++) {
				if (array1 [i] != array2 [i]) {
					a = false;
					break;
				}
			}
		}
		if (array1.Length > 0) {
			msg += " -> Expected " + BitConverter.ToString (array1, 0);
			msg += " is different than " + BitConverter.ToString (array2, 0);
		}
		Assertion.Assert (msg, a);
	}

	private const string testfile = "test.pvk";

	[TearDown]
	public void TearDown () 
	{
		File.Delete (testfile);
	}

	private void WriteBuffer (byte[] buffer) 
	{
		FileStream fs = File.Create (testfile);
		fs.Write (buffer, 0, buffer.Length);
		fs.Close ();
	}

	static byte[] nopwd = { 
	0x1E, 0xF1, 0xB5, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x01, 0x00, 0x00, 
	0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 
	0x00, 0x02, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0xDB, 0x27, 0x34, 0xCB, 
	0x3D, 0x27, 0xA0, 0x4F, 0x50, 0x68, 0xC7, 0x95, 0x4B, 0x7B, 0x69, 0xD1, 
	0xFD, 0x30, 0x58, 0x72, 0x6B, 0xFF, 0x77, 0x64, 0x96, 0x35, 0x72, 0x36, 
	0x98, 0xCE, 0x56, 0xDD, 0x94, 0x43, 0x7C, 0x0D, 0x61, 0x5C, 0x3A, 0xD6, 
	0x1E, 0xD1, 0x89, 0x6C, 0xD5, 0x9B, 0x3E, 0xD3, 0x60, 0x3E, 0x28, 0x3F, 
	0xC6, 0x51, 0x35, 0x0D, 0x4F, 0x7E, 0x79, 0xE6, 0xAE, 0xE4, 0xC8, 0xE9, 
	0xA9, 0x14, 0x6E, 0xD2, 0xBD, 0x42, 0xB2, 0x14, 0x82, 0xEE, 0x26, 0x8F, 
	0x21, 0x33, 0x1A, 0xD5, 0xD7, 0x6D, 0x90, 0xED, 0xC1, 0xA4, 0x1C, 0x84, 
	0x3F, 0xA3, 0x8A, 0xFB, 0x33, 0x30, 0x32, 0xF6, 0xE3, 0xE6, 0xC8, 0x81, 
	0x54, 0x88, 0x1A, 0x92, 0xF0, 0xBA, 0xB8, 0x4F, 0x52, 0x8D, 0xBD, 0x04, 
	0x47, 0xBC, 0x55, 0xBC, 0xD0, 0x3D, 0x2C, 0x7F, 0x4F, 0xAB, 0x99, 0xDC, 
	0xFB, 0x2D, 0x18, 0xF3, 0x99, 0x77, 0x10, 0x82, 0x48, 0xF3, 0xDE, 0x36, 
	0xD7, 0x62, 0xA9, 0xCB, 0x58, 0x01, 0x97, 0x79, 0x66, 0x0D, 0x01, 0x1F, 
	0xCC, 0x0B, 0xAB, 0x02, 0xA9, 0xE3, 0xF5, 0x85, 0xA8, 0x52, 0xBC, 0x10, 
	0xD7, 0x90, 0x60, 0x60, 0x50, 0xB1, 0x08, 0x01, 0x85, 0x52, 0xAC, 0x05, 
	0xF1, 0xCE, 0xF9, 0xE7, 0xBE, 0xDE, 0x46, 0x64, 0x40, 0xE5, 0x07, 0x82, 
	0x20, 0xDD, 0x48, 0xF1, 0xE1, 0x85, 0x29, 0x8C, 0xFE, 0x57, 0x7C, 0x65, 
	0xF5, 0x5C, 0x51, 0x9F, 0x63, 0xDE, 0xFC, 0x9C, 0xF9, 0x3F, 0x3D, 0xF2, 
	0xDC, 0x9F, 0x65, 0x27, 0xEC, 0x50, 0x54, 0xB9, 0xCE, 0xF2, 0xC3, 0x10, 
	0x93, 0x8B, 0xBE, 0x6A, 0xC1, 0x35, 0x19, 0xBB, 0x66, 0xA5, 0x5E, 0xEA, 
	0x91, 0x1D, 0xFB, 0x26, 0xF8, 0x0F, 0x5C, 0x13, 0x73, 0xCC, 0x9A, 0x68, 
	0x4C, 0x08, 0x9C, 0x02, 0xE5, 0xD5, 0x91, 0x37, 0x13, 0x68, 0x3D, 0xFC, 
	0x3E, 0xA7, 0x43, 0x94, 0xBC, 0xFC, 0x4F, 0xB1, 0x8E, 0xC5, 0x5F, 0x24, 
	0x9A, 0x6C, 0xDB, 0xC2, 0x49, 0x91, 0xEC, 0x2B, 0xB9, 0x3D, 0x2B, 0x96, 
	0xA3, 0x60, 0xE3, 0xA8, 0x8C, 0x28, 0xB7, 0x53 };

	[Test]
	public void MSNoPassword ()
	{
		WriteBuffer (nopwd);
		PrivateKey pvk = PrivateKey.CreateFromFile (testfile);
		Assertion.AssertNotNull ("msnopwd.RSA", pvk.RSA);
		Assertion.Assert ("msnopwd.Encrypted", !pvk.Encrypted);
		Assertion.Assert ("msnopwd.Weak", pvk.Weak);
	}

	// this will convert a PVK file without a password to a PVK file
	// with a password (weak)
	[Test]
	public void ConvertToPasswordWeak () 
	{
		WriteBuffer (nopwd);
		PrivateKey pvk = PrivateKey.CreateFromFile (testfile);
		string rsa1 = pvk.RSA.ToXmlString (true);
		pvk.Save (testfile, "password");
		pvk = PrivateKey.CreateFromFile (testfile, "password");
		Assertion.AssertNotNull ("topwd.RSA", pvk.RSA);
		string rsa2 = pvk.RSA.ToXmlString (true);
		Assertion.AssertEquals ("topwd.RSA identical", rsa1, rsa2);
		Assertion.Assert ("topwd.Encrypted", pvk.Encrypted);
		Assertion.Assert ("topwd.Weak", pvk.Weak);
	}

	// this will convert a PVK file without a password to a PVK file
	// with a password (strong)
	[Test]
	public void ConvertToPasswordStrong () 
	{
		WriteBuffer (nopwd);
		PrivateKey pvk = PrivateKey.CreateFromFile (testfile);
		string rsa1 = pvk.RSA.ToXmlString (true);
		pvk.Weak = false; // we want strong crypto
		pvk.Save (testfile, "password");
		pvk = PrivateKey.CreateFromFile (testfile, "password");
		Assertion.AssertNotNull ("topwd.RSA", pvk.RSA);
		string rsa2 = pvk.RSA.ToXmlString (true);
		Assertion.AssertEquals ("topwd.RSA identical", rsa1, rsa2);
		Assertion.Assert ("topwd.Encrypted", pvk.Encrypted);
		Assertion.Assert ("topwd.Weak", !pvk.Weak);
	}

	static byte[] pwd = { 
	0x1E, 0xF1, 0xB5, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
	0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x34, 0x01, 0x00, 0x00, 
	0x37, 0x53, 0x7C, 0x99, 0x01, 0xB5, 0x50, 0xF3, 0x79, 0x6E, 0xDE, 0xD5, 
	0x8A, 0x1B, 0xED, 0x05, 0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 
	0x33, 0x98, 0xE8, 0x81, 0xA2, 0x9A, 0xEB, 0x36, 0xEF, 0x1B, 0x52, 0xFD, 
	0xC3, 0x9B, 0xB7, 0x32, 0x02, 0xC0, 0x9F, 0xE0, 0x6A, 0x50, 0x81, 0x61, 
	0x37, 0xBE, 0xEC, 0x1B, 0xC3, 0x34, 0x7B, 0x03, 0x0B, 0xC8, 0x31, 0x7B, 
	0x0D, 0xA6, 0x7A, 0x05, 0xEA, 0xD1, 0xCA, 0x9A, 0xF3, 0x71, 0x84, 0x77, 
	0x9E, 0x6F, 0xD1, 0xD0, 0xA0, 0x62, 0xFF, 0x3D, 0x24, 0x31, 0x01, 0xD7, 
	0x02, 0x38, 0x11, 0xB6, 0x5E, 0x4A, 0xCC, 0x33, 0xF0, 0xEB, 0x0B, 0x38, 
	0x51, 0x27, 0xCF, 0xAD, 0x20, 0x20, 0x9A, 0x80, 0x80, 0x37, 0xBE, 0x4C, 
	0xBC, 0xA4, 0xC8, 0xE1, 0x5B, 0x57, 0x02, 0xC9, 0x04, 0x53, 0x82, 0x6E, 
	0x0B, 0x06, 0x94, 0xCF, 0xC2, 0xEF, 0x1A, 0x6C, 0xC8, 0x78, 0x41, 0xB1, 
	0x63, 0xBD, 0x52, 0x1C, 0x05, 0x2C, 0x97, 0x83, 0x10, 0xD0, 0xFE, 0x22, 
	0x2F, 0x29, 0xAF, 0xC0, 0xCA, 0xC7, 0x96, 0x0A, 0x9A, 0xC8, 0x69, 0x58, 
	0xBF, 0xA9, 0xDD, 0x75, 0xE4, 0xAB, 0xC8, 0xFE, 0xF5, 0xFE, 0xC5, 0x18, 
	0x2B, 0x93, 0xC0, 0x67, 0xFF, 0xDC, 0xE3, 0xAF, 0xAC, 0x5F, 0x7E, 0x5F, 
	0x0D, 0xEA, 0x41, 0xEB, 0x57, 0x1A, 0x4D, 0xB3, 0x10, 0x07, 0x09, 0xDC, 
	0x3F, 0xC1, 0xB7, 0x9F, 0xC5, 0x79, 0xCD, 0x6E, 0x79, 0x48, 0x4F, 0x51, 
	0xD8, 0x4B, 0x3A, 0x32, 0x40, 0x05, 0x6B, 0x74, 0xC9, 0xF4, 0xD9, 0x67, 
	0x9D, 0x65, 0xFF, 0x4C, 0x4E, 0xAB, 0xC0, 0xC5, 0x65, 0x49, 0xEB, 0x6D, 
	0xAB, 0xB9, 0x30, 0x5A, 0xFC, 0x5D, 0xD4, 0xE7, 0xB5, 0xDB, 0xD3, 0xF1, 
	0xBF, 0x6F, 0xD4, 0x18, 0xD6, 0xE7, 0x76, 0x12, 0xCE, 0x57, 0xDF, 0x63, 
	0x2C, 0x88, 0x2F, 0x0F, 0x31, 0x3A, 0x78, 0xA0, 0xB9, 0x5A, 0x11, 0x50, 
	0x18, 0x98, 0xA4, 0xA3, 0x9D, 0xC7, 0xC4, 0x5C, 0xE7, 0xDF, 0xFD, 0x4B, 
	0x96, 0x84, 0x27, 0x4C, 0x84, 0x92, 0xE9, 0x5E, 0x93, 0x65, 0x0C, 0xC9, 
	0xB4, 0x5B, 0xE7, 0xC0, 0x26, 0x66, 0x36, 0x7A, 0x36, 0x56, 0xB0, 0xC8, 
	0x34, 0xBB, 0x4F, 0xBD, 0x0E, 0xDA, 0x04, 0x82, 0x74, 0x07, 0x3D, 0xD3, 
	0x1D, 0x8F, 0x9B, 0x34, 0x4F, 0xA8, 0xD5, 0x58, 0x12, 0xE8, 0x96, 0x20 };

	[Test]
	public void MSPasswordWeak () 
	{
		WriteBuffer (pwd);
		PrivateKey pvk = PrivateKey.CreateFromFile (testfile, "password");
		Assertion.AssertNotNull ("mspwd.RSA", pvk.RSA);
		Assertion.Assert ("mspwd.Encrypted", pvk.Encrypted);
		Assertion.Assert ("mspwd.Weak", pvk.Weak);
	}

	// this will convert a PVK file with a password to a PVK file
	// without a password
	[Test]
	public void RemovePassword () 
	{
		WriteBuffer (nopwd);
		PrivateKey pvk = PrivateKey.CreateFromFile (testfile, "password");
		string rsa1 = pvk.RSA.ToXmlString (true);
		pvk.Save (testfile);
		pvk = PrivateKey.CreateFromFile (testfile);
		Assertion.AssertNotNull ("nomorepwd.RSA", pvk.RSA);
		string rsa2 = pvk.RSA.ToXmlString (true);
		Assertion.AssertEquals ("nomorepwd.RSA identical", rsa1, rsa2);
		Assertion.Assert ("nomorepwd.Encrypted", !pvk.Encrypted);
		Assertion.Assert ("nomorepwd.Weak", pvk.Weak);
	}
}

}
