//
// GuidTest.cs - NUnit Test Cases for the System.Guid struct
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{

public class GuidTest : TestCase
{
        public GuidTest (string name): base(name) {}

	public static ITest Suite
	{
		get {
			return new TestSuite (typeof(GuidTest));
		}
	}

	public void TestCtor1() {
		Guid g =  new Guid(new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f});
		bool exception;
		
		if (BitConverter.IsLittleEndian) {
			AssertEquals("A1", g.ToString(), "03020100-0504-0706-0809-0a0b0c0d0e0f");
		}
		else {
			AssertEquals("A1", g.ToString(), "00010203-0405-0607-0809-0a0b0c0d0e0f");
		}

		try {
			Guid g1 = new Guid ((byte[])null);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		AssertEquals("A2", exception, true);

		try {
			Guid g1 = new Guid(new byte[] {0x00, 0x01, 0x02});
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		AssertEquals("A3", exception, true);
	}

	public void TestCtor4() {
		Guid g1 = new Guid(0x00010203, (short) 0x0405, (short) 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid(unchecked((int) 0xffffffff), unchecked((short) 0xffff), unchecked((short) 0xffff), 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
		
		AssertEquals("A1", g1.ToString(), "00010203-0405-0607-0809-0a0b0c0d0e0f");
		AssertEquals("A2", g2.ToString(), "ffffffff-ffff-ffff-ffff-ffffffffffff");

	}

	public void TestCtor5() {
		Guid g1 = new Guid(0x00010203u, (ushort) 0x0405u, (ushort) 0x0607u, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid(0xffffffffu, (ushort) 0xffffu, (ushort) 0xffffu, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
		
		AssertEquals("A1", g1.ToString(), "00010203-0405-0607-0809-0a0b0c0d0e0f");
		AssertEquals("A2", g2.ToString(), "ffffffff-ffff-ffff-ffff-ffffffffffff");

	}

	public void TestEmpty() {
		AssertEquals("A1", Guid.Empty.ToString(), "00000000-0000-0000-0000-000000000000");
	}

	public void TestEquals() {
		Guid g1 = new Guid(0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid(0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g3 = new Guid(0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);
		string s = "Thus is not a Guid!";

		AssertEquals("A1", g1.Equals(g1), true);
		AssertEquals("A2", g1.Equals(g2), true);
		AssertEquals("A3", g1.Equals(g3), false);
		AssertEquals("A4", g1.Equals(null), false);
		AssertEquals("A5", g1.Equals(s), false);
	}

	public void TestToString() {
		Guid g = new Guid(0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		bool exception;
		
		AssertEquals("A1", g.ToString(), "00010203-0405-0607-0809-0a0b0c0d0e0f");
		AssertEquals("A2", g.ToString("N"), "000102030405060708090a0b0c0d0e0f");
		AssertEquals("A3", g.ToString("D"), "00010203-0405-0607-0809-0a0b0c0d0e0f");
		AssertEquals("A4", g.ToString("B"), "{00010203-0405-0607-0809-0a0b0c0d0e0f}");
		AssertEquals("A5", g.ToString("P"), "(00010203-0405-0607-0809-0a0b0c0d0e0f)");
		AssertEquals("A6", g.ToString(""), "000102030405060708090a0b0c0d0e0f");
		AssertEquals("A7", g.ToString(null), "000102030405060708090a0b0c0d0e0f");

		try {
			g.ToString("X");
			exception = false;
		}
		catch ( FormatException ) {
			exception = true;
		}
		AssertEquals("A8", exception, true);

		try {
			g.ToString("This is invalid");
			exception = false;
		}
		catch ( FormatException ) {
			exception = true;
		}
		AssertEquals("A9", exception, true);

		AssertEquals("A10", g.ToString("B", null), "{00010203-0405-0607-0809-0a0b0c0d0e0f}");

		
	}

}

}
