//
// RijndaelTest.cs - NUnit Test Cases for Rijndael
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

public class RijndaelTest : TestCase {
	public RijndaelTest () : base ("System.Security.Cryptography.Rijndael testsuite") {}
	public RijndaelTest (string name) : base(name) {}

	protected Rijndael aes;

	protected override void SetUp () 
	{
		aes = Rijndael.Create ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (RijndaelTest)); 
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// FIPS197 B 
	public void TestFIPS197_AppendixB () 
	{
		byte[] key = { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c };
		byte[] iv = new byte[16]; // empty - not used for ECB
		byte[] input = { 0x32, 0x43, 0xf6, 0xa8, 0x88, 0x5a, 0x30, 0x8d, 0x31, 0x31, 0x98, 0xa2, 0xe0, 0x37, 0x07, 0x34 };
		byte[] expected = { 0x39, 0x25, 0x84, 0x1d, 0x02, 0xdc, 0x09, 0xfb, 0xdc, 0x11, 0x85, 0x97, 0x19, 0x6a, 0x0b, 0x32 };

		aes.Mode = CipherMode.ECB;
		aes.KeySize = 128;
		aes.Padding = PaddingMode.Zeros;

		byte[] output = new byte [input.Length];
		ICryptoTransform encryptor = aes.CreateEncryptor (key, iv);
		encryptor.TransformBlock (input, 0, input.Length, output, 0);
		AssertEquals ("FIPS197 B Encrypt", expected, output);
	
		byte[] original = new byte [output.Length];
		ICryptoTransform decryptor = aes.CreateDecryptor(key, iv); 
		decryptor.TransformBlock (output, 0, output.Length, original, 0);
		AssertEquals ("FIPS197 B Decrypt", input, original);
	}

	// FIPS197 C.1 AES-128 (Nk=4, Nr=10)
	public void TestFIPS197_AppendixC1 () 
	{
		byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
		byte[] iv = new byte[16]; // empty - not used for ECB
		byte[] input = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
		byte[] expected = { 0x69, 0xc4, 0xe0, 0xd8, 0x6a, 0x7b, 0x04, 0x30, 0xd8, 0xcd, 0xb7, 0x80, 0x70, 0xb4, 0xc5, 0x5a };

		aes.Mode = CipherMode.ECB;
		aes.KeySize = 128;
		aes.Padding = PaddingMode.Zeros;

		byte[] output = new byte [input.Length];
		ICryptoTransform encryptor = aes.CreateEncryptor (key, iv);
		encryptor.TransformBlock(input, 0, input.Length, output, 0);
		AssertEquals ("FIPS197 C1 Encrypt", expected, output);
	
		byte[] original = new byte [output.Length];
		ICryptoTransform decryptor = aes.CreateDecryptor(key, iv); 
		decryptor.TransformBlock(output, 0, output.Length, original, 0);
		AssertEquals ("FIPS197 C1 Decrypt", input, original);
	}

	// FIPS197 C.2 AES-192 (Nk=6, Nr=12)
	public void TestFIPS197_AppendixC2 () 
	{
		byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17 };
		byte[] iv = new byte[16]; // empty - not used for ECB
		byte[] input = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
		byte[] expected = { 0xdd, 0xa9, 0x7c, 0xa4, 0x86, 0x4c, 0xdf, 0xe0, 0x6e, 0xaf, 0x70, 0xa0, 0xec, 0x0d, 0x71, 0x91 };

		aes.Mode = CipherMode.ECB;
		aes.KeySize = 192;
		aes.Padding = PaddingMode.Zeros;

		byte[] output = new byte [input.Length];
		ICryptoTransform encryptor = aes.CreateEncryptor (key, iv);
		encryptor.TransformBlock(input, 0, input.Length, output, 0);
		AssertEquals ("FIPS197 C2 Encrypt", expected, output);
	
		byte[] original = new byte [output.Length];
		ICryptoTransform decryptor = aes.CreateDecryptor(key, iv); 
		decryptor.TransformBlock(output, 0, output.Length, original, 0);
		AssertEquals ("FIPS197 C2 Decrypt", input, original);
	}

	// C.3 AES-256 (Nk=8, Nr=14)
	public void TestFIPS197_AppendixC3 () 
	{
		byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f };
		byte[] iv = new byte[16]; // empty - not used for ECB
		byte[] input = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
		byte[] expected = { 0x8e, 0xa2, 0xb7, 0xca, 0x51, 0x67, 0x45, 0xbf, 0xea, 0xfc, 0x49, 0x90, 0x4b, 0x49, 0x60, 0x89 };

		aes.Mode = CipherMode.ECB;
		aes.KeySize = 256;
		aes.Padding = PaddingMode.Zeros;

		byte[] output = new byte [input.Length];
		ICryptoTransform encryptor = aes.CreateEncryptor (key, iv);
		encryptor.TransformBlock(input, 0, input.Length, output, 0);
		AssertEquals ("FIPS197 C3 Encrypt", expected, output);
	
		byte[] original = new byte [output.Length];
		ICryptoTransform decryptor = aes.CreateDecryptor(key, iv); 
		decryptor.TransformBlock(output, 0, output.Length, original, 0);
		AssertEquals ("FIPS197 C3 Decrypt", input, original);
	}

}

}
