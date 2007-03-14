//
// AssemblyNameTest.cs - NUnit Test Cases for AssemblyName
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;
#if !TARGET_JVM
using System.Reflection.Emit;
#endif
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

namespace MonoTests.System.Reflection {

[TestFixture]
public class AssemblyNameTest {
	static public void AssertEqualsByteArrays (string msg, byte[] array1, byte[] array2) 
	{
		if ((array1 == null) && (array2 == null))
			return;
		if (array1 == null)
			Assert.Fail (msg + " -> First array is NULL");
		if (array2 == null)
			Assert.Fail (msg + " -> Second array is NULL");

		bool a = (array1.Length == array2.Length);
		if (a) {
			for (int i = 0; i < array1.Length; i++) {
				if (array1 [i] != array2 [i]) {
					a = false;
					break;
				}
			}
		}
		msg += " -> Expected " + (array1.Length > 0 ? BitConverter.ToString (array1, 0) : "<empty>");
		msg += " is different than " + (array2.Length > 0 ? BitConverter.ToString (array2, 0) : "<empty>");
		Assert.IsTrue (a, msg);
	}

	private AssemblyName an;

	private string tempDir = Path.Combine (Path.GetTempPath (), "MonoTests.System.Reflection.AssemblyNameTest");

#if !TARGET_JVM // Thread.GetDomain is not supported for TARGET_JVM.
	private AppDomain domain;
#endif // TARGET_JVM

	// created with "sn -o test.snk test.txt"
	static byte[] publicKey = { 0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3D, 0xBD, 0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0, 0x58, 0x07, 0x2B, 0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22, 0xDB, 0x20, 0xB2, 0xA9, 0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64, 0x2F, 0x5D, 0x8D, 0xEB, 0x78, 0x02, 0xF7, 0xA5, 0x36, 0x77, 0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68, 0xDB, 0xEB, 0x24, 0x09, 0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E, 0x59, 0x54, 0x4A, 0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F, 0xA8, 0x3E, 0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D, 0x10, 0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB, 0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F, 0xF5, 0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D, 0xD3, 0x56, 0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07, 0x93, 0x44, 0x00, 0x4A, 0xF8, 0xFF, 0xA4, 0xCB, 0x73, 0xC0, 0x6A, 0x62, 0xB4, 0xB7, 0xC8, 0x92, 0x58, 0x87, 0xCD, 0x07,
		0x0C, 0x7D, 0x6C, 0xC1, 0x4A, 0xFC, 0x82, 0x57, 0x0E, 0x43, 0x85, 0x09, 0x75, 0x98, 0x51, 0xBB, 0x35, 0xF5, 0x64, 0x83, 0xC7, 0x79, 0x89, 0x5C, 0x55, 0x36, 0x66, 0xAB, 0x27, 0xA4, 0xD9, 0xD4, 0x7E, 0x6B, 0x67, 0x64, 0xC1, 0x54, 0x4E, 0x37, 0xF1, 0x4E, 0xCA, 0xB3, 0xE5, 0x63, 0x91, 0x57, 0x12, 0x14, 0xA6, 0xEA, 0x8F, 0x8F, 0x2B, 0xFE, 0xF3, 0xE9, 0x16, 0x08, 0x2B, 0x86, 0xBC, 0x26, 0x0D, 0xD0, 0xC6, 0xC4, 0x1A, 0x72, 0x43, 0x76, 0xDC, 0xFF, 0x28, 0x52, 0xA1, 0xDE, 0x8D, 0xFA, 0xD5, 0x1F, 0x0B, 0xB5, 0x4F, 0xAF, 0x06, 0x79, 0x11, 0xEE, 0xA8, 0xEC, 0xD3, 0x74, 0x55, 0xA2, 0x80, 0xFC, 0xF8, 0xD9, 0x50, 0x69, 0x48, 0x01, 0xC2, 0x5A, 0x04, 0x56, 0xB4, 0x3E, 0x24, 0x32, 0x20, 0xB5, 0x2C, 0xDE, 0xBB, 0xBD, 0x13, 0xFD, 0x13, 0xF7, 0x03, 0x3E, 0xE3, 0x37, 0x84, 0x74, 0xE7, 0xD0, 0x5E, 0x9E, 0xB6, 0x26, 0xAE, 0x6E, 0xB0, 0x55, 0x6A, 0x52, 0x63, 0x6F, 0x5A, 0x9D, 0xF2, 0x67, 0xD6, 0x61, 0x4F, 0x7A, 0x45, 0xEE, 0x5C, 0x3D, 0x2B, 0x7C, 0xB2, 0x40, 0x79, 0x54, 0x84, 0xD1, 
		0xBE, 0x61, 0x3E, 0x5E, 0xD6, 0x18, 0x8E, 0x14, 0x98, 0xFC, 0x35, 0xBF, 0x5F, 0x1A, 0x20, 0x2E, 0x1A, 0xD8, 0xFF, 0xC4, 0x6B, 0xC0, 0xC9, 0x7D, 0x06, 0xEF, 0x09, 0xF9, 0xF3, 0x69, 0xFC, 0xBC, 0xA2, 0xE6, 0x80, 0x22, 0xB9, 0x79, 0x7E, 0xEF, 0x57, 0x9F, 0x49, 0xE1, 0xBC, 0x0D, 0xB6, 0xA1, 0xFE, 0x8D, 0xBC, 0xBB, 0xA3, 0x05, 0x02, 0x6B, 0x04, 0x45, 0xF7, 0x5D, 0xEE, 0x43, 0x06, 0xD6, 0x9C, 0x94, 0x48, 0x1A, 0x0B, 0x9C, 0xBC, 0xB4, 0x4E, 0x93, 0x60, 0x87, 0xCD, 0x58, 0xD6, 0x9A, 0x39, 0xA6, 0xC0, 0x7F, 0x8E, 0xFF, 0x25, 0xC1, 0xD7, 0x2C, 0xF6, 0xF4, 0x6F, 0x24, 0x52, 0x0B, 0x39, 0x42, 0x1B, 0x0D, 0x04, 0xC1, 0x93, 0x2A, 0x19, 0x1C, 0xF0, 0xB1, 0x9B, 0xC1, 0x24, 0x6D, 0x1B, 0x0B, 0xDA, 0x1C, 0x8B, 0x72, 0x48, 0xF0, 0x3E, 0x52, 0xBF, 0x0A, 0x84, 0x3A, 0x9B, 0xC8, 0x6D, 0x13, 0x1E, 0x72, 0xF4, 0x46, 0x93, 0x88, 0x1A, 0x5F, 0x4C, 0x3C, 0xE5, 0x9D, 0x6E, 0xBB, 0x4E, 0xDD, 0x5D, 0x1F, 0x11, 0x40, 0xF4, 0xD7, 0xAF, 0xB3, 0xAB, 0x9A, 0x99, 0x15, 0xF0, 0xDC, 0xAA, 0xFF,
		0x9F, 0x2D, 0x9E, 0x56, 0x4F, 0x35, 0x5B, 0xBA, 0x06, 0x99, 0xEA, 0xC6, 0xB4, 0x48, 0x51, 0x17, 0x1E, 0xD1, 0x95, 0x84, 0x81, 0x18, 0xC0, 0xF1, 0x71, 0xDE, 0x44, 0x42, 0x02, 0x06, 0xAC, 0x0E, 0xA8, 0xE2, 0xF3, 0x1F, 0x96, 0x1F, 0xBE, 0xB6, 0x1F, 0xB5, 0x3E, 0xF6, 0x81, 0x05, 0x20, 0xFA, 0x2E, 0x40, 0x2E, 0x4D, 0xA0, 0x0E, 0xDA, 0x42, 0x9C, 0x05, 0xAA, 0x9E, 0xAF, 0x5C, 0xF7, 0x3A, 0x3F, 0xBB, 0x91, 0x73, 0x45, 0x27, 0xA8, 0xA2, 0x07, 0x4A, 0xEF, 0x59, 0x1E, 0x97, 0x9D, 0xE0, 0x30, 0x5A, 0x83, 0xCE, 0x1E, 0x57, 0x32, 0x89, 0x43, 0x41, 0x28, 0x7D, 0x14, 0x8D, 0x8B, 0x41, 0x1A, 0x56, 0x76, 0x43, 0xDB, 0x64, 0x86, 0x41, 0x64, 0x8D, 0x4C, 0x91, 0x83, 0x4E, 0xF5, 0x6C };

	static byte[] token = { 0xFF, 0xEF, 0x94, 0x53, 0x67, 0x69, 0xDA, 0x06 };

	[SetUp]
	public void SetUp () 
	{
		try {
			if (Directory.Exists (tempDir))
				Directory.Delete (tempDir, true);
		}
		catch (Exception) {
		}

		Directory.CreateDirectory (tempDir);

#if !TARGET_JVM // Thread.GetDomain is not supported for TARGET_JVM.
		domain = Thread.GetDomain ();
#endif // TARGET_JVM
	}

	[TearDown]
	public void TearDown () 
	{
		try {
			if (Directory.Exists (tempDir))
				Directory.Delete (tempDir, true);
		}
		catch (Exception) {
			// This can happen on windows when the directory contains
			// files opened by the CLR
		}
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
#endif
	public void EmptyAssembly () 
	{
		an = new AssemblyName ();
		Assert.IsNull (an.CodeBase, "CodeBase");
		Assert.IsNull (an.CultureInfo, "CultureInfo");
		Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
#if NET_2_0
		Assert.AreEqual (String.Empty, an.FullName, "FullName");
#else
		Assert.IsNull (an.FullName, "FullName");
#endif
		Assert.AreEqual (AssemblyHashAlgorithm.None, an.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.IsNull (an.Name, "Name");
		Assert.IsNull (an.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			an.VersionCompatibility, "VersionCompatibility");
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
	[ExpectedException (typeof (SecurityException))]
#endif
	public void PublicKey () 
	{
		an = new AssemblyName ();
		Assert.IsNull (an.GetPublicKey (), "PublicKey(empty)");
		an.SetPublicKey (publicKey);

		Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "Flags");
		// strangly it doesn't affect the KeyPair ?
		Assert.IsNull (an.KeyPair, "KeyPair");
		AssertEqualsByteArrays ("PublicKey", publicKey, an.GetPublicKey ());
		AssertEqualsByteArrays ("PublicKeyToken", token, an.GetPublicKeyToken ());
	}

	[Test]
	public void PublicKeyToken () 
	{
		an = new AssemblyName ();
		an.SetPublicKeyToken (token);

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.IsNull (an.KeyPair, "KeyPair");
		Assert.IsNull (an.GetPublicKey (), "PublicKey");
		AssertEqualsByteArrays ("PublicKeyToken", token, an.GetPublicKeyToken ());
	}

	[Test]
	public void KeyPair () 
	{
		an = new AssemblyName ();
		an.KeyPair = new StrongNameKeyPair (publicKey);

		Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
		Assert.IsNotNull (an.KeyPair, "KeyPair");
		Assert.IsNull (an.GetPublicKey (), "PublicKey");
		Assert.IsNull (an.GetPublicKeyToken (), "PublicKeyToken");
	}

	// !!! this assembly MUST NOT use a StrongName !!!
	[Test]
	[Category ("NotWorking")] // in other cases null is returned
	public void Self () 
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		AssemblyName an = a.GetName ();

		Assert.IsNotNull (an.GetPublicKey (), "PublicKey(self)");
		Assert.AreEqual (0, an.GetPublicKey ().Length, "PublicKey.Length");
	}

	[Test]
	public void FullName1 ()
	{
		// !!! we assume the mscorlib has a strong name !!!
		AssemblyName an = typeof(int).Assembly.GetName ();
		Assert.IsNotNull (an.FullName, "FullName1#1");
		Assert.IsTrue (an.FullName.IndexOf ("Version=") != -1, "FullName1#2");
		Assert.IsTrue (an.FullName.IndexOf("Culture=") != -1, "FullName1#3");
		Assert.IsTrue (an.FullName.IndexOf ("PublicKeyToken=") != -1, "FullName1#4");
	}

	[Test]
	public void FullName2 ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with only name
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		Assert.IsNotNull (an.FullName, "FullName2#1");
		Assert.AreEqual (an.Name, an.FullName, "FullName2#2");
		Assert.AreEqual (-1, an.FullName.IndexOf ("Culture="), "FullName2#3");
		Assert.AreEqual (-1, an.FullName.IndexOf ("PublicKeyToken="), "FullName2#4");
	}

	[Test]
	public void FullName3 ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2";

		// tests for AssemblyName with name and version
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		an.Version = new Version (assemblyVersion);
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion, an.FullName, "FullName3#1");
	}

	[Test]
	public void FullName4 ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name and neutral culture
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		an.CultureInfo = CultureInfo.InvariantCulture;
		Assert.AreEqual (assemblyName + ", Culture=neutral", an.FullName, "FullName4#1");
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
	[ExpectedException (typeof (SecurityException))]
#endif
	public void FullName5 ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name and public key
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		an.SetPublicKey (publicKey);
		Assert.AreEqual (assemblyName + ", PublicKeyToken=" + GetTokenString(token), an.FullName, "FullName5#1");
	}

	[Test]
	public void FullName6 ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2";

		// tests for AssemblyName with name, version and neutral culture
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		an.Version = new Version (assemblyVersion);
		an.CultureInfo = CultureInfo.InvariantCulture;
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion 
			+ ", Culture=neutral", an.FullName, "FullName6#1");
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
	[ExpectedException (typeof (SecurityException))]
#endif
	public void FullName7 ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2";

		// tests for AssemblyName with name, version and public key
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		an.Version = new Version (assemblyVersion);
		an.SetPublicKey (publicKey);
		Assert.AreEqual (assemblyName + ", Version=" + assemblyVersion
			+ ", PublicKeyToken=" + GetTokenString (token), 
			an.FullName, "FullName7#1");
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
	[ExpectedException (typeof (SecurityException))]
#endif
	public void FullName8 ()
	{
		const string assemblyName = "TestAssembly";

		// tests for AssemblyName with name, culture and public key
		AssemblyName an = new AssemblyName ();
		an.Name = assemblyName;
		an.CultureInfo = CultureInfo.InvariantCulture;
		an.SetPublicKey (publicKey);
		Assert.AreEqual (assemblyName + ", Culture=neutral"
			+ ", PublicKeyToken=" + GetTokenString (token),
			an.FullName, "FullName8#1");
	}

	static int nameIndex = 0;

	private AssemblyName GenAssemblyName () 
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.AssemblyNameTest" + (nameIndex ++);
		return assemblyName;
	}

#if !TARGET_JVM // Reflection.Emit is not supported for TARGET_JVM.
	private Assembly GenerateAssembly (AssemblyName name) 
	{
		AssemblyBuilder ab = domain.DefineDynamicAssembly (
			name,
			AssemblyBuilderAccess.RunAndSave,
			tempDir);
		ab.DefineDynamicModule ("def_module");
		ab.Save (name.Name + ".dll");

		return Assembly.LoadFrom (Path.Combine (tempDir, name.Name + ".dll"));
	}

	private AssemblyBuilder GenerateDynamicAssembly (AssemblyName name)
	{
		AssemblyBuilder ab = domain.DefineDynamicAssembly (
				name,
				AssemblyBuilderAccess.Run);

		return ab;
	}

	[Test]
	public void TestCultureInfo ()
	{
		AssemblyName name = GenAssemblyName ();
		name.CultureInfo = CultureInfo.CreateSpecificCulture ("ar-DZ");

		Assembly a = GenerateAssembly (name);
		Assert.AreEqual (a.GetName ().CultureInfo.Name, "ar-DZ");
	}

	[Test]
	public void Version ()
	{
		AssemblyName name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3, 4);

		Assembly a = GenerateAssembly (name);
		Assert.AreEqual ("1.2.3.4", a.GetName ().Version.ToString (), "1.2.3.4 normal");

		name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3);

		a = GenerateAssembly (name);
		Assert.AreEqual ("1.2.3.0", a.GetName ().Version.ToString (), "1.2.3.0 normal");

		name = GenAssemblyName ();
		name.Version = new Version (1, 2);

		a = GenerateAssembly (name);
		Assert.AreEqual ("1.2.0.0", a.GetName ().Version.ToString (), "1.2.0.0 normal");
	}

	[Test]
	[Category ("NotWorking")]
	public void Version_Dynamic ()
	{
		AssemblyName name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3, 4);

		AssemblyBuilder ab = GenerateDynamicAssembly (name);
		Assert.AreEqual ("1.2.3.4", ab.GetName ().Version.ToString (), "1.2.3.4 dynamic");

		name = GenAssemblyName ();
		name.Version = new Version (1, 2, 3);

		ab = GenerateDynamicAssembly (name);
#if NET_2_0
		Assert.AreEqual ("1.2.3.0", ab.GetName ().Version.ToString (), "1.2.3.0 dynamic");
#else
		Assert.AreEqual ("1.2.3.65535", ab.GetName ().Version.ToString (), "1.2.3.0 dynamic");
#endif

		name = GenAssemblyName ();
		name.Version = new Version (1, 2);

		ab = GenerateDynamicAssembly (name);
#if NET_2_0
		Assert.AreEqual ("1.2.0.0", ab.GetName ().Version.ToString (), "1.2.0.0 dynamic");
#else
		Assert.AreEqual ("1.2.65535.65535", ab.GetName ().Version.ToString (), "1.2.0.0 dynamic");
#endif
	}
#endif // TARGET_JVM

	[Test]
	public void HashAlgorithm ()
	{
		Assert.AreEqual (AssemblyHashAlgorithm.SHA1, 
			typeof (int).Assembly.GetName ().HashAlgorithm);
	}

	[Test]
	public void Serialization ()
	{
		AssemblyName an = new AssemblyName ();
		an.CodeBase = "http://www.test.com/test.dll";
		an.CultureInfo = CultureInfo.InvariantCulture;
		an.Flags = AssemblyNameFlags.PublicKey;
		an.HashAlgorithm = AssemblyHashAlgorithm.MD5;
		an.KeyPair = new StrongNameKeyPair (publicKey);
		an.Name = "TestAssembly";
		an.Version = new Version (1, 5);
		an.VersionCompatibility = AssemblyVersionCompatibility.SameProcess;

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, an);

		// reset position of memorystream
		ms.Position = 0;

		// deserialze assembly name
		AssemblyName dsAssemblyName = (AssemblyName) bf.Deserialize (ms);

		// close the memorystream
		ms.Close ();

		// compare orginal and deserialized assembly name
		Assert.AreEqual (an.CodeBase, dsAssemblyName.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, dsAssemblyName.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.Flags, dsAssemblyName.Flags, "Flags");
		Assert.AreEqual (an.HashAlgorithm, dsAssemblyName.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.Name, dsAssemblyName.Name, "Name");
		Assert.AreEqual (an.Version, dsAssemblyName.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, dsAssemblyName.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (an.EscapedCodeBase, dsAssemblyName.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.FullName, dsAssemblyName.FullName, "FullName");
		Assert.AreEqual (an.ToString (), dsAssemblyName.ToString (), "ToString");
		AssertEqualsByteArrays ("PublicKey", an.GetPublicKey (), dsAssemblyName.GetPublicKey ());
		AssertEqualsByteArrays ("PublicToken", an.GetPublicKeyToken (), dsAssemblyName.GetPublicKeyToken ());
	}

	[Test]
	public void Serialization_WithoutStrongName ()
	{
		AssemblyName an = new AssemblyName ();
		an.CodeBase = "http://www.test.com/test.dll";
		an.CultureInfo = CultureInfo.InvariantCulture;
		an.Flags = AssemblyNameFlags.None;
		an.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
		an.KeyPair = null;
		an.Name = "TestAssembly2";
		an.Version = new Version (1, 5, 0, 0);
		an.VersionCompatibility = AssemblyVersionCompatibility.SameMachine;

		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, an);

		// reset position of memorystream
		ms.Position = 0;

		// deserialze assembly name
		AssemblyName dsAssemblyName = (AssemblyName) bf.Deserialize (ms);

		// close the memorystream
		ms.Close ();

		// compare orginal and deserialized assembly name
		Assert.AreEqual (an.CodeBase, dsAssemblyName.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, dsAssemblyName.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.Flags, dsAssemblyName.Flags, "Flags");
		Assert.AreEqual (an.HashAlgorithm, dsAssemblyName.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.Name, dsAssemblyName.Name, "Name");
		Assert.AreEqual (an.Version, dsAssemblyName.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, dsAssemblyName.VersionCompatibility, "VersionCompatibility");
		Assert.AreEqual (an.EscapedCodeBase, dsAssemblyName.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.FullName, dsAssemblyName.FullName, "FullName");
		Assert.AreEqual (an.ToString (), dsAssemblyName.ToString (), "ToString");
		AssertEqualsByteArrays ("PublicKey", an.GetPublicKey (), dsAssemblyName.GetPublicKey ());
		AssertEqualsByteArrays ("PublicToken", an.GetPublicKeyToken (), dsAssemblyName.GetPublicKeyToken ());
	}

#if !TARGET_JVM // Assemblyname.GetObjectData not implemented yet for TARGET_JVM
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void GetObjectData_Null ()
	{
		AssemblyName an = new AssemblyName ();
		an.GetObjectData (null, new StreamingContext (StreamingContextStates.All));
	}
#endif // TARGET_JVM

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
#endif
	public void Clone_Empty ()
	{
		an = new AssemblyName ();
		AssemblyName clone = (AssemblyName) an.Clone ();

		Assert.IsNull (clone.CodeBase, "CodeBase");
		Assert.IsNull (clone.CultureInfo, "CultureInfo");
		Assert.IsNull (clone.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (AssemblyNameFlags.None, clone.Flags, "Flags");
#if NET_2_0
		Assert.AreEqual (String.Empty, clone.FullName, "FullName");
#else
		Assert.IsNull (clone.FullName, "FullName");
#endif
		Assert.AreEqual (AssemblyHashAlgorithm.None, clone.HashAlgorithm, "HashAlgorithm");
		Assert.IsNull (clone.KeyPair, "KeyPair");
		Assert.IsNull (clone.Name, "Name");
		Assert.IsNull (clone.Version, "Version");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
			clone.VersionCompatibility, "VersionCompatibility");
	}

	[Test]
	public void Clone_Self ()
	{
		an = Assembly.GetExecutingAssembly ().GetName ();
		AssemblyName clone = (AssemblyName) an.Clone ();

		Assert.AreEqual (an.CodeBase, clone.CodeBase, "CodeBase");
		Assert.AreEqual (an.CultureInfo, clone.CultureInfo, "CultureInfo");
		Assert.AreEqual (an.EscapedCodeBase, clone.EscapedCodeBase, "EscapedCodeBase");
		Assert.AreEqual (an.Flags, clone.Flags, "Flags");
		Assert.AreEqual (an.FullName, clone.FullName, "FullName");
		Assert.AreEqual (an.HashAlgorithm, clone.HashAlgorithm, "HashAlgorithm");
		Assert.AreEqual (an.KeyPair, clone.KeyPair, "KeyPair");
		Assert.AreEqual (an.Name, clone.Name, "Name");
		Assert.AreEqual (an.Version, clone.Version, "Version");
		Assert.AreEqual (an.VersionCompatibility, clone.VersionCompatibility, "VersionCompatibility");
	}

	[Test]
	[ExpectedException (typeof (FileNotFoundException))]
	public void GetAssemblyName_AssemblyFile_DoesNotExist ()
	{
		AssemblyName.GetAssemblyName (Path.Combine (tempDir, "doesnotexist.dll"));
	}

	[Test]
	[Category ("NotWorking")]
	public void GetAssemblyName_AssemblyFile_LoadFailure ()
	{
		string file = Path.Combine (tempDir, "loadfailure.dll");
		using (FileStream fs = File.Open (file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None)) {
			try {
				AssemblyName.GetAssemblyName (file);
				Assert.Fail ("#1");
			} catch (FileLoadException ex) {
			}
		}
		File.Delete (file);
	}

	[Test]
	public void GetAssemblyName_AssemblyFile_BadImage ()
	{
		string file = Path.Combine (tempDir, "badimage.dll");
		using (StreamWriter sw = File.CreateText (file)) {
			sw.WriteLine ("somegarbage");
		}
		try {
			AssemblyName.GetAssemblyName (file);
			Assert.Fail ("#1");
		} catch (BadImageFormatException ex) {
		}
		File.Delete (file);
	}

	[Test]
	public void GetAssemblyName_CodeBase ()
	{
		Assembly execAssembly = Assembly.GetExecutingAssembly ();

		AssemblyName aname = AssemblyName.GetAssemblyName (execAssembly.Location);
		Assert.IsNotNull (aname.CodeBase, "#1");
		Assert.AreEqual (execAssembly.CodeBase, aname.CodeBase, "#2");
	}

	// helpers

	private string GetTokenString (byte[] value)
	{
		string tokenString = "";
		for (int i = 0; i < value.Length; i++) {
			tokenString += value[i].ToString ("x2");
		}
		return tokenString;
	}

#if NET_2_0
	[Test]
	[Category ("NotWorking")]
	public void Ctor1 ()
	{
		const string assemblyName = "TestAssembly";
		AssemblyName an = new AssemblyName (assemblyName);
		Assert.IsNotNull (an.Name, "Ctor1#1");
		Assert.AreEqual (an.Name, assemblyName, "Ctor1#2");
		Assert.IsNull (an.Version, "Ctor1#3");
		Assert.IsNull (an.CultureInfo, "Ctor1#4");
		Assert.IsNull (an.GetPublicKeyToken (), "Ctor1#5");
	}

	[Test]
	[Category("TargetJvmNotWorking")] // Not yet supported for TARGET_JVM.
	public void Ctor2 ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyVersion = "1.2.3.4";
		AssemblyName an = new AssemblyName (assemblyName + ", Version=" + assemblyVersion);
		Assert.IsNotNull (an.Name, "Ctor2#1");
		Assert.AreEqual (an.Name, assemblyName, "Ctor2#2");
		Assert.IsNotNull (an.Version, "Ctor2#3");
		Assert.AreEqual (an.Version, new Version (assemblyVersion), "Ctor2#4");
		Assert.IsNull (an.CultureInfo, "Ctor2#5");
		Assert.IsNull (an.GetPublicKeyToken (), "Ctor2#6");
	}

	[Test]
	[Category ("NotWorking")]
	public void Ctor3 ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyCulture = "en-US";
		AssemblyName an = new AssemblyName (assemblyName + ", Culture=" + assemblyCulture);
		Assert.IsNotNull (an.Name, "Ctor3#1");
		Assert.AreEqual (an.Name, assemblyName, "Ctor3#2");
		Assert.IsNotNull (an.CultureInfo, "Ctor3#3");
		Assert.AreEqual (an.CultureInfo, new CultureInfo (assemblyCulture), "Ctor3#4");
		Assert.IsNull (an.Version, "Ctor3#5");
		Assert.IsNull (an.GetPublicKeyToken (), "Ctor3#6");
	}

	[Test]
	[Category ("NotWorking")]
	public void Ctor4 ()
	{
		const string assemblyName = "TestAssembly";
		byte [] assemblyKeyToken;
		AssemblyName an = new AssemblyName (assemblyName + ", PublicKeyToken=" + GetTokenString (token));
		Assert.IsNotNull (an.Name, "Ctor4#1");
		Assert.AreEqual (an.Name, assemblyName, "Ctor4#2");
		Assert.IsNotNull (assemblyKeyToken = an.GetPublicKeyToken (), "Ctor4#3");
		Assert.AreEqual (assemblyKeyToken, token, "Ctor4#4");
		Assert.IsNull (an.Version, "Ctor4#5");
		Assert.IsNull (an.CultureInfo, "Ctor4#6");
	}

	[Test]
	[Category("TargetJvmNotWorking")] // Not yet supported for TARGET_JVM.
	public void Ctor5 ()
	{
		const string assemblyName = "TestAssembly";
		const string assemblyCulture = "neutral";
		const string assemblyVersion = "1.2.3.4";
		byte [] assemblyKeyToken;

		AssemblyName an = new AssemblyName (assemblyName + ", Version=" + assemblyVersion + 
				", Culture=" + assemblyCulture + ", PublicKeyToken=" + GetTokenString (token));
		Assert.IsNotNull (an.Name, "Ctor5#1");
		Assert.AreEqual (an.Name, assemblyName, "Ctor5#2");
		Assert.IsNotNull (an.CultureInfo, "Ctor5#3");
		Assert.AreEqual (an.CultureInfo, new CultureInfo (""), "Ctor5#4");
		Assert.IsNotNull (an.Version, "Ctor5#5");
		Assert.AreEqual (an.Version, new Version (assemblyVersion), "Ctor5#6");
		Assert.IsNotNull (assemblyKeyToken = an.GetPublicKeyToken (), "Ctor5#7");
		Assert.AreEqual (assemblyKeyToken, token, "Ctor5#8");
	}

	[Test]
	[Category ("NotWorking")]
	public void Ctor6 ()
	{
		const string assemblyName = "TestAssembly";
		AssemblyName an = null;
		
		// null argument
		try {
			an = new AssemblyName (null);
		} catch (ArgumentNullException) {
		}
		Assert.IsNull (an, "Ctor6#1");

		// empty string
		an = null;
		try {
			an = new AssemblyName ("");
		} catch (ArgumentException) {
		}
		Assert.IsNull (an, "Ctor6#2");

		// incomplete entry
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", Version=,Culture=neutral");
		} catch (FileLoadException) {
		}
		Assert.IsNull (an, "Ctor6#3");

		// bad format for version
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", Version=a.b");
		} catch (FileLoadException) {
		}
		Assert.IsNull (an, "Ctor6#4");

		// bad culture info
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", Culture=aa-AA");
		} catch (ArgumentException) {
		}
		Assert.IsNull (an, "Ctor6#5");

		// incorrect length for key token
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", PublicKeyToken=27576a8182a188");
		} catch (FileLoadException) {
		}
		Assert.IsNull (an, "Ctor6#6");

		// Incorrect length for key
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", PublicKey=0024000004800000940000000602000000240000525341310004000011000000e39d99616f48cf7d6d59f345e485e713e89b8b1265a31b1a393e9894ee3fbddaf382dcaf4083dc31ee7a40a2a25c69c6d019fba9f37ec17fd680e4f6fe3b5305f71ae9e494e3501d92508c2e98ca1e22991a217aa8ce259c9882ffdfff4fbc6fa5e6660a8ff951cd94ed011e5633651b64e8f4522519b6ec84921ee22e4840e");
		} catch (FileLoadException) {
		}
		Assert.IsNull (an, "Ctor6#7");

		// missing spec
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", =1.2.4.5");
		} catch (FileLoadException) {
		}
		Assert.IsNull (an, "Ctor6#8");

		// No '=' found
		an = null;
		try {
			an = new AssemblyName (assemblyName + ", OtherAttribute");
		} catch (FileLoadException) {
		}
		Assert.IsNull (an, "Ctor6#9");
	}

#endif
}

}
