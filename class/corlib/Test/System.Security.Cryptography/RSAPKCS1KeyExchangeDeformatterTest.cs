//
// RSAPKCS1KeyExchangeDeformatterTest.cs - NUnit Test Cases for RSAPKCS1KeyExchangeDeformatter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class RSAPKCS1KeyExchangeDeformatterTest : Assertion {

	protected static RSA key;

	[SetUp]
	void SetUp () 
	{
		// generating a keypair is REALLY long and the framework
		// makes sure that we generate one (even if create an object
		// to import an existing key)
		if (key == null) {
			key = RSA.Create ();
			key.ImportParameters (AllTests.GetRsaKey (true));
		}
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// LAMESPEC: RSAPKCS1KeyExchangeDeformatter.RNG versus RSAPKCS1KeyExchangeFormatter.Rng
	[Test]
	public void Properties () 
	{
		RSAPKCS1KeyExchangeDeformatter keyex = new RSAPKCS1KeyExchangeDeformatter ();
		keyex.SetKey (key);
		AssertNull("RSAPKCS1KeyExchangeDeformatter.Parameters", keyex.Parameters);
		// null (default)
		AssertNull("RSAPKCS1KeyExchangeDeformatter.RNG", keyex.RNG);
		AssertEquals("RSAPKCS1KeyExchangeDeformatter.ToString()", "System.Security.Cryptography.RSAPKCS1KeyExchangeDeformatter", keyex.ToString ());
	}

	// TestExchangeMin (1)
	// Test vector (EM) generated by CryptoAPI on Windows
	[Test]
	public void CapiKeyExchangeMin () 
	{
		byte[] M = { 0x01 };
		byte[] EM = { 0x50, 0x33, 0xF3, 0x42, 0x52, 0x59, 0x71, 0x2D, 0x6E, 0x25, 0x5E, 0x06, 0xC3, 0x27, 0x94, 0xA6, 0xD1, 0x8E, 0x13, 0x90, 0x54, 0x5C, 0x12, 0x58, 0x7A, 0xC9, 0xB6, 0x3F, 0x4D, 0x2E, 0x97, 0xCC, 0x3A, 0x94, 0x24, 0xE8, 0x11, 0x1F, 0xD6, 0x7F, 0x37, 0x36, 0xAB, 0x6F, 0x3F, 0xB4, 0x1B, 0xB8, 0x13, 0x87, 0xC8, 0xBE, 0x00, 0x24, 0x02, 0x0F, 0xF6, 0x2E, 0xEA, 0x48, 0x8A, 0x6F, 0xC8, 0xF6, 0x0B, 0xAB, 0xF4, 0x02, 0xA5, 0xE2, 0x5A, 0xAA, 0xB5, 0x9E, 0xC2, 0x6E, 0xFF, 0xA6, 0xEC, 0xEC, 0xD0, 0xA2, 0x3F, 0x00, 0x93, 0xE9, 0xF3, 0xAA, 0x08, 0xA2, 0xD2, 0x11, 0x1B, 0x3F, 0x3E, 0x59, 0xB0, 0xBA, 0x47, 0x17, 0x8F, 0xF4, 0xEB, 0x34, 0xA5, 0xC4, 0xA4, 0x09, 0x43, 0xC4, 0x7B, 0x71, 0x2C, 0x4B, 0x9E, 0x2D, 0x22, 0x96, 0xBB, 0x52, 0xDD, 0x2B, 0x59, 0xED, 0xD6, 0xCA, 0xEB, 0xE6 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeDeformatter 1", M, Mback);
	}

	// test with a message 128 bits (16 bytes) long
	// Test vector (EM) generated by CryptoAPI on Windows
	[Test]
	public void CapiKeyExchange128 () 
	{
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49 };
		byte[] EM = { 0x2D, 0xA3, 0xB0, 0xED, 0x1F, 0x13, 0x13, 0xBA, 0xAA, 0x26, 0xA7, 0x00, 0x76, 0x94, 0x0A, 0xDA, 0xFB, 0x4E, 0x14, 0x98, 0xD3, 0xF6, 0x26, 0x65, 0xCE, 0x7E, 0xB9, 0x23, 0xEF, 0xDE, 0x6E, 0xAB, 0x72, 0x33, 0xF3, 0x6F, 0xA9, 0x9B, 0xEC, 0x18, 0xC9, 0xB7, 0xC7, 0xE8, 0xE8, 0x55, 0xC4, 0x83, 0x1E, 0xF5, 0xDA, 0xCF, 0x5A, 0x53, 0xB0, 0x60, 0x42, 0xF4, 0x55, 0xEE, 0x00, 0x80, 0x92, 0x28, 0xA9, 0x0E, 0x2D, 0x9D, 0x49, 0x10, 0x65, 0x00, 0x21, 0x82, 0xCC, 0x05, 0xA3, 0x62, 0xAD, 0xCC, 0x5B, 0xE3, 0x8E, 0xAE, 0x01, 0x96, 0x81, 0xF6, 0x7B, 0x52, 0xB9, 0x6F, 0xE3, 0x06, 0x3A, 0x48, 0x4D, 0x87, 0xB9, 0xA3, 0xEA, 0x69, 0xD1, 0xFE, 0x8D, 0x82, 0x33, 0xE3, 0x05, 0xEB, 0x00, 0xA2, 0xA6, 0xDC, 0x95, 0xE4, 0xAC, 0x4E, 0xF4, 0x03, 0xC3, 0xFE, 0xA2, 0xE8, 0xB6, 0xBB, 0xBE, 0xD1 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// test with a message 160 bits (20 bytes) long
	// Test vector (EM) generated by CryptoAPI on Windows
	[Test]
	public void CapiKeyExchange160 () 
	{
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49, 0x00, 0x00, 0x00, 0x00 };
		byte[] EM = { 0x10, 0x79, 0x3A, 0x88, 0x04, 0x4B, 0xA5, 0x18, 0xD6, 0xCE, 0x97, 0x9B, 0xFF, 0xE8, 0xB4, 0xF5, 0x8D, 0x60, 0x07, 0xCD, 0x5F, 0x89, 0xA6, 0xCF, 0x5B, 0x90, 0x96, 0xC7, 0xF6, 0xD7, 0xF2, 0xCA, 0x7C, 0x13, 0x5A, 0x62, 0xB4, 0xED, 0xF4, 0xD7, 0x5C, 0x99, 0x4C, 0x07, 0xF4, 0x9F, 0x96, 0xE6, 0xBF, 0x2B, 0x82, 0x85, 0x38, 0x2C, 0x03, 0xBD, 0x61, 0x07, 0xF6, 0x05, 0x15, 0x55, 0xBF, 0xA9, 0x3B, 0xF5, 0x10, 0x96, 0x81, 0x01, 0x58, 0x5F, 0x61, 0x43, 0x52, 0x77, 0x71, 0x9C, 0x92, 0xEF, 0xD5, 0xE2, 0x60, 0x3F, 0x82, 0x69, 0x9F, 0xAF, 0xC2, 0xE1, 0x68, 0xB7, 0x5E, 0x62, 0xAC, 0x61, 0x6A, 0x1B, 0x46, 0x03, 0xF6, 0x7C, 0x20, 0x47, 0xF7, 0x6E, 0x7D, 0x35, 0x2A, 0xF6, 0x9C, 0xDA, 0x8A, 0xED, 0xAC, 0x1A, 0xC8, 0xF6, 0x4E, 0x7D, 0x21, 0xAC, 0x18, 0xEB, 0xA7, 0x68, 0xE0, 0xE2 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// Max = k - m - 11
	// Test vector (EM) generated by CryptoAPI on Windows
	[Test]
	public void CapiKeyExchangeMax () 
	{
		byte[] M = new byte [(key.KeySize >> 3)- 11];
		byte[] EM = { 0x4B, 0x3F, 0x77, 0xE1, 0xA0, 0x6C, 0xD9, 0xFA, 0x19, 0x69, 0x21, 0xC4, 0x67, 0x2B, 0x0F, 0x2A, 0x0E, 0xCB, 0xAF, 0xAD, 0x08, 0xA5, 0xD2, 0x9B, 0xDC, 0x04, 0xDE, 0x8F, 0x13, 0xE4, 0x81, 0x25, 0xAF, 0xC5, 0x82, 0x51, 0xA9, 0x39, 0xAF, 0x82, 0xFF, 0xC7, 0x4F, 0x04, 0xE4, 0x21, 0xAC, 0xEE, 0x2F, 0x44, 0x78, 0x11, 0x29, 0x74, 0x3F, 0x74, 0xC1, 0x38, 0xC5, 0x43, 0x29, 0x2F, 0x0C, 0x7B, 0xDB, 0x2E, 0xE5, 0xA8, 0x6A, 0xEE, 0x6A, 0x14, 0xCC, 0x4E, 0x53, 0x8C, 0x0C, 0xEE, 0x23, 0x24, 0xDC, 0x9B, 0x75, 0x7C, 0xAD, 0x0C, 0xAC, 0x13, 0xC5, 0x02, 0x9E, 0x5D, 0x65, 0x76, 0xCB, 0xD4, 0xBF, 0x70, 0x43, 0xBE, 0x28, 0x67, 0x3F, 0x5D, 0x93, 0x38, 0x67, 0x4B, 0x25, 0x59, 0xF7, 0x8E, 0x4F, 0xCE, 0x2B, 0x2F, 0xA7, 0x4C, 0x68, 0x4C, 0xCC, 0x5F, 0xF3, 0x0A, 0xB7, 0xAA, 0x54, 0x7C };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// TestExchangeMin (1)
	// Test vector (EM) generated by Mono on Windows
	[Test]
	public void MonoKeyExchangeMin () 
	{
		byte[] M = { 0x01 };
		byte[] EM = { 0x73, 0x34, 0xAF, 0xE5, 0x45, 0x53, 0x4A, 0x93, 0x25, 0x77, 0x6F, 0x80, 0x06, 0xAD, 0x7C, 0x87, 0xB9, 0xE8, 0x1E, 0x5C, 0xBB, 0x9B, 0x3F, 0xDC, 0x9C, 0x65, 0x71, 0xE6, 0x50, 0x82, 0xDC, 0x77, 0x6C, 0x6B, 0xA6, 0x39, 0x18, 0x0B, 0x33, 0x54, 0x4E, 0x65, 0x32, 0x6C, 0x53, 0x70, 0x9B, 0xEA, 0x7C, 0x83, 0x0D, 0xBF, 0x8B, 0x48, 0x5B, 0x0F, 0xCB, 0x27, 0x7D, 0x8D, 0x18, 0xD7, 0xA5, 0x13, 0x33, 0x3C, 0xC8, 0xB0, 0xF4, 0x12, 0x52, 0x24, 0x3C, 0x2A, 0xD2, 0xDF, 0x7C, 0x0B, 0xCB, 0x7C, 0x26, 0x28, 0x5F, 0x88, 0x1E, 0x22, 0x98, 0x68, 0x04, 0x12, 0x6E, 0x9F, 0x2D, 0xFE, 0x7A, 0xEF, 0xC3, 0x9D, 0x87, 0x44, 0x46, 0xCA, 0xA2, 0x81, 0xF2, 0xE7, 0xBA, 0x9D, 0x17, 0x68, 0x96, 0xA2, 0x3F, 0xB3, 0xB4, 0x43, 0x34, 0x2D, 0x7D, 0x56, 0xF5, 0xFC, 0x40, 0xEB, 0x31, 0xB0, 0x0C, 0x99 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeDeformatter 1", M, Mback);
	}

	// test with a message 128 bits (16 bytes) long
	// Test vector (EM) generated by Mono on Windows
	[Test]
	public void MonoKeyExchange128 () 
	{
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49 };
		byte[] EM = { 0xAA, 0x95, 0x6D, 0x40, 0xA7, 0x26, 0x23, 0x4E, 0xA9, 0xCB, 0x83, 0x55, 0xCE, 0x2F, 0xDD, 0x80, 0xEA, 0xC8, 0x61, 0x25, 0x57, 0xF9, 0x86, 0x46, 0x2E, 0xD9, 0xAD, 0xA1, 0x90, 0x22, 0x6A, 0x1F, 0xCF, 0x24, 0x9D, 0x3A, 0x65, 0x75, 0xF6, 0x9E, 0xBD, 0xC0, 0xBB, 0x8F, 0xC0, 0xC3, 0x20, 0x45, 0xC9, 0x8C, 0x5F, 0xEA, 0xF9, 0xE3, 0x1E, 0x95, 0xA0, 0xAD, 0xD6, 0xB6, 0x3C, 0x9B, 0x03, 0x9F, 0xB0, 0x57, 0x32, 0x2F, 0x98, 0x0E, 0x94, 0x8C, 0x6E, 0xA7, 0x9F, 0x40, 0xCF, 0xAD, 0x6E, 0xDB, 0x38, 0x9F, 0xF5, 0x43, 0xD1, 0x70, 0xF9, 0xCA, 0x3A, 0x2E, 0x0B, 0xB9, 0x34, 0x12, 0x0F, 0x09, 0x5B, 0x6B, 0xB9, 0xFD, 0x7E, 0xC6, 0xFC, 0xA1, 0x9A, 0x48, 0xEA, 0x3A, 0xED, 0x77, 0x24, 0xA5, 0x3B, 0x8B, 0xFB, 0xF1, 0x2B, 0x9D, 0xED, 0x0A, 0xB5, 0x05, 0xDC, 0x59, 0xA8, 0x1F, 0x17, 0xC9 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// test with a message 160 bits (20 bytes) long
	// Test vector (EM) generated by Mono on Windows
	[Test]
	public void MonoKeyExchange160 () 
	{
		byte[] M = { 0xd4, 0x36, 0xe9, 0x95, 0x69, 0xfd, 0x32, 0xa7, 0xc8, 0xa0, 0x5b, 0xbc, 0x90, 0xd3, 0x2c, 0x49, 0x00, 0x00, 0x00, 0x00 };
		byte[] EM = { 0x31, 0x2B, 0x21, 0x0F, 0x1D, 0x75, 0xCE, 0xDF, 0x00, 0xC4, 0xC2, 0x50, 0x59, 0x13, 0xDA, 0xF4, 0xE4, 0x73, 0xD3, 0x26, 0xC7, 0xBD, 0xAF, 0xDC, 0x73, 0xB1, 0xC0, 0x32, 0xE3, 0xE9, 0x91, 0x4C, 0x1F, 0x74, 0x29, 0x8C, 0xD6, 0xFD, 0x4C, 0x8C, 0xD2, 0x30, 0xED, 0xEF, 0x97, 0xF1, 0x91, 0xFF, 0xD8, 0x3D, 0x04, 0xD2, 0x2D, 0xB7, 0x20, 0x25, 0x1D, 0x47, 0xBA, 0xEA, 0x3D, 0xE2, 0x7D, 0x9C, 0x41, 0x0C, 0x5C, 0x63, 0xBC, 0xB7, 0xFA, 0xDD, 0x30, 0x19, 0x3E, 0xD2, 0x5F, 0x1B, 0xBC, 0x59, 0x0A, 0x54, 0x0A, 0xE0, 0x82, 0x5D, 0x05, 0xA4, 0xDC, 0x23, 0x71, 0x33, 0x84, 0x68, 0xDA, 0x8C, 0x7A, 0x23, 0x2E, 0x16, 0x28, 0x3E, 0x43, 0x24, 0x30, 0x69, 0xD4, 0x43, 0x7F, 0x82, 0xA8, 0xAC, 0xFF, 0xCC, 0xA6, 0x62, 0x20, 0x61, 0x5F, 0x03, 0xEE, 0x7C, 0x9E, 0x5C, 0xB2, 0xA0, 0xE4, 0xC6 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// Max = k - m - 11
	// Test vector (EM) generated by Mono on Windows
	[Test]
	public void MonoKeyExchangeMax () 
	{
		byte[] M = new byte [(key.KeySize >> 3)- 11];
		byte[] EM = { 0xB4, 0x17, 0xE4, 0x8A, 0x14, 0xB1, 0x9B, 0x08, 0xBE, 0xBF, 0xD3, 0xD1, 0xCD, 0xE5, 0xB1, 0x0D, 0x38, 0x08, 0x01, 0x31, 0x10, 0xDA, 0x8A, 0xB9, 0xE9, 0x4E, 0x2F, 0x94, 0x2F, 0x40, 0x36, 0x04, 0x57, 0x54, 0xAC, 0x22, 0xC1, 0x6B, 0x35, 0x10, 0xF9, 0xA9, 0xEA, 0x36, 0xC9, 0x13, 0x84, 0x95, 0xCB, 0xDE, 0x9C, 0x01, 0x66, 0x32, 0x01, 0xA1, 0xB2, 0xDB, 0x4F, 0x11, 0x10, 0x2D, 0x13, 0x36, 0x52, 0x30, 0x78, 0x65, 0x00, 0x7A, 0xD8, 0x5B, 0x47, 0xA6, 0x19, 0x9C, 0xFA, 0x76, 0x1A, 0x44, 0x92, 0x3E, 0xE3, 0x5A, 0x0B, 0x56, 0x4D, 0x2D, 0x54, 0x7B, 0x07, 0x5C, 0xA7, 0x14, 0x86, 0x52, 0x0A, 0x8F, 0x11, 0xE2, 0x32, 0xED, 0x3C, 0x21, 0xF8, 0x56, 0x0D, 0x38, 0xAC, 0x24, 0x4A, 0x32, 0xB3, 0x4F, 0xA3, 0xB1, 0x02, 0xC7, 0x8A, 0x22, 0xE6, 0x9C, 0x78, 0xEB, 0x98, 0x4B, 0x92, 0x24 };

		AsymmetricKeyExchangeDeformatter keyback = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] Mback = keyback.DecryptKeyExchange (EM);
		AssertEquals ("RSAPKCS1KeyExchangeFormatter 1", M, Mback);
	}

	// TestExchangeTooBig
	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void KeyExchangeTooBig () 
	{
		AsymmetricKeyExchangeDeformatter keyex = new RSAPKCS1KeyExchangeDeformatter (key);
		byte[] EM = new byte [(key.KeySize >> 3) + 1];
		// invalid format
		byte[] M = keyex.DecryptKeyExchange (EM);
	}
}

}
