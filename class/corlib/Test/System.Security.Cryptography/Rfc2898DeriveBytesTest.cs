//
// Rfc2898DeriveBytesTest.cs - NUnit Test Cases for Rfc2898DeriveBytes
//
// Author:
//	Sebastien Pouliot  <spouliot@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	// References:
	// a.	PKCS#5: Password-Based Cryptography Standard 
	//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html
	// b.	RFC 3211 - Password-based Encryption for CMS
	//	http://www.faqs.org/rfcs/rfc3211.html

	[TestFixture]
	public class Rfc2898DeriveBytesTest : Assertion {

		public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			AllTests.AssertEquals (msg, array1, array2);
		}

		static private byte[] salt = { 0x12, 0x34, 0x56, 0x78, 0x78, 0x56, 0x34, 0x12 };

		[Test]
		public void ConstructorPasswordSalt () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt);
			AssertEquals ("IterationCount", 1000, pkcs5.IterationCount);
			AssertEquals ("Salt", salt, pkcs5.Salt);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPasswordNullSalt () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes (null, salt);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPasswordSaltNull () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", null);
		}

		[Test]
		public void ConstructorPasswordSaltIterations () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 5);
			AssertEquals ("IterationCount", 5, pkcs5.IterationCount);
			AssertEquals ("Salt", salt, pkcs5.Salt);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPasswordNullSaltIterations () 
		{
			string password = null;
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes (password, salt, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPasswordSaltNullIterations () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", null, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorPasswordSaltIterationsZero () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorPasswordSaltIterationsNegative () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, Int32.MinValue);
		}

		[Test]
		public void ConstructorPasswordSaltLength () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", 8);
			AssertEquals ("IterationCount", 1000, pkcs5.IterationCount);
			AssertEquals ("Salt", 8, pkcs5.Salt.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPasswordNullSaltLength () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes (null, 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorPasswordSaltLengthZero () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorPasswordSaltLengthNegative () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", Int32.MinValue);
		}

		[Test]
		public void ConstructorPasswordSaltLengthIterations () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", 8, 5);
			AssertEquals ("IterationCount", 5, pkcs5.IterationCount);
			AssertEquals ("Salt", 8, pkcs5.Salt.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPasswordNullSaltLengthIterations () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes (null, 8, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorPasswordSaltLengthZeroIterations () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", 0, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorPasswordSaltLengthNegativeIterations () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", Int32.MinValue, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorPasswordSaltLengthIterationsZero () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", 8, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorPasswordSaltLengthIterationsNegative () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", 8, Int32.MinValue);
		}

		[Test]
		public void IterationCount () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 5);
			AssertEquals ("IterationCount", 5, pkcs5.IterationCount);
			pkcs5.IterationCount = Int32.MaxValue;
			AssertEquals ("IterationCount", Int32.MaxValue, pkcs5.IterationCount);
		}

		[Test]
		public void SaltNew () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt);
			AssertEquals ("Salt", salt, pkcs5.Salt);
			byte[] newSalt = (byte[]) salt.Clone ();
			newSalt [0] = 0xFF;
			pkcs5.Salt = newSalt;
			AssertEquals ("Salt(new)", newSalt, pkcs5.Salt);
		}

		[Test]
		public void SaltCantModifyInternal () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt);
			AssertEquals ("Salt", salt, pkcs5.Salt);
			byte[] modSalt = (byte[]) salt.Clone ();
			modSalt [0] = 0xFF;
			Assert ("Can't modify internal salt", (BitConverter.ToString(pkcs5.Salt) != BitConverter.ToString(modSalt)));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SaltNull () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt);
			pkcs5.Salt = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SaltTooSmall () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt);
			byte[] smallSalt = { 0x01, 0x02, 0x03, 0x04, 0x05 };
			pkcs5.Salt = smallSalt;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetBytesZero ()
		{
			byte[] expected = { 0xd1, 0xda, 0xa7, 0x86, 0x15, 0xf2, 0x87, 0xe6 };
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 5);
			byte[] key = pkcs5.GetBytes (0);
			AssertEquals ("GetBytesZero", 0, key.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetBytesNegative () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 5);
			byte[] key = pkcs5.GetBytes (Int32.MinValue);
		}

		[Test]
		public void GetBytes_Endian ()
		{
			string expected = "0B-40-39-04-B9-2B-F1-B9-C0-A0-64-E6-03-0A-5E-42-4B-88-1E-5E-94-8F-77-87-16-A6-C4-9E-E6-C7-6D-38";
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 5);
			// this will overflow F on multiple bytes (where endianess comes to play)
			byte[] key = pkcs5.GetBytes (32768);
			// just check the last 32 bytes
			string actual = BitConverter.ToString (key, key.Length - 32);
			AssertEquals ("Endian", expected, actual);
		}

		[Test]
		public void RFC3211_TC1 () 
		{
			byte[] expected = { 0xd1, 0xda, 0xa7, 0x86, 0x15, 0xf2, 0x87, 0xe6 };
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("password", salt, 5);
			byte[] key = pkcs5.GetBytes (8);
			AssertEquals ("RFC3211_TC1", expected, key);
		}

		[Test]
		public void RFC3211_TC2 () 
		{
			byte[] expected = { 0x6A, 0x89, 0x70, 0xBF, 0x68, 0xC9, 0x2C, 0xAE, 0xA8, 0x4A, 0x8D, 0xF2, 0x85, 0x10, 0x85, 0x86 };
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("All n-entities must communicate with other n-entities via n-1 entiteeheehees", salt, 500);
			byte[] key = pkcs5.GetBytes (16);
			AssertEquals ("RFC3211_TC2", expected, key);
		}

		[Test]
		public void RFC3211_TC2_TwoBlocks () 
		{
			byte[] expected = { 0x6A, 0x89, 0x70, 0xBF, 0x68, 0xC9, 0x2C, 0xAE, 0xA8, 0x4A, 0x8D, 0xF2, 0x85, 0x10, 0x85, 0x86, 0x07, 0x12, 0x63, 0x80, 0xcc, 0x47, 0xab, 0x2d, 0xa6, 0xcc, 0xda, 0xfb, 0x26, 0x83, 0xdf, 0xe8 };
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("All n-entities must communicate with other n-entities via n-1 entiteeheehees", salt, 500);
			byte[] key = pkcs5.GetBytes (32);
			AssertEquals ("RFC3211_TC2_TwoBlocks", expected, key);
		}

		[Test]
		public void RFC3211_TC2_Splitted_OneBlock () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("All n-entities must communicate with other n-entities via n-1 entiteeheehees", salt, 500);
			byte[] key1 = pkcs5.GetBytes (8);
			byte[] expected_part1 = { 0x6A, 0x89, 0x70, 0xBF, 0x68, 0xC9, 0x2C, 0xAE };
			AssertEquals ("RFC3211_TC2_Splitted_OneBlock-1", expected_part1, key1);
			byte[] expected_part2 = { 0xA8, 0x4A, 0x8D, 0xF2, 0x85, 0x10, 0x85, 0x86 };
			byte[] key2 = pkcs5.GetBytes (8);
			AssertEquals ("RFC3211_TC2_Splitted_OneBlock-2", expected_part2, key2);
		}

		[Test]
		public void RFC3211_TC2_Splitted_TwoBlocks () 
		{
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("All n-entities must communicate with other n-entities via n-1 entiteeheehees", salt, 500);
			byte[] key1 = pkcs5.GetBytes (16);
			byte[] expected_part1 = { 0x6A, 0x89, 0x70, 0xBF, 0x68, 0xC9, 0x2C, 0xAE, 0xA8, 0x4A, 0x8D, 0xF2, 0x85, 0x10, 0x85, 0x86 };
			AssertEquals ("RFC3211_TC2_Splitted_TwoBlocks-1", expected_part1, key1);
			byte[] expected_part2 = { 0x07, 0x12, 0x63, 0x80, 0xcc, 0x47, 0xab, 0x2d, 0xa6, 0xcc, 0xda, 0xfb, 0x26, 0x83, 0xdf, 0xe8 };
			byte[] key2 = pkcs5.GetBytes (16);
			AssertEquals ("RFC3211_TC2_Splitted_TwoBlocks-2", expected_part2, key2);
		}

		[Test]
		public void RFC3211_TC2_Reset () 
		{
			byte[] expected = { 0x6A, 0x89, 0x70, 0xBF, 0x68, 0xC9, 0x2C, 0xAE };
			Rfc2898DeriveBytes pkcs5 = new Rfc2898DeriveBytes ("All n-entities must communicate with other n-entities via n-1 entiteeheehees", salt, 500);
			byte[] key1 = pkcs5.GetBytes (8);
			AssertEquals ("RFC3211_TC2_part1", expected, key1);
			pkcs5.Reset ();
			byte[] key2 = pkcs5.GetBytes (8);
			AssertEquals ("RFC3211_TC2_part2", expected, key2);
		}
	}
}

#endif
