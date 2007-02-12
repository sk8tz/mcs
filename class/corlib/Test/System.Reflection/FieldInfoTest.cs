//
// FieldInfoTest - NUnit Test Cases for the FieldInfo class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 64)]
public class Class1 {
	[FieldOffset (32)]
	public int i;
}

[StructLayout(LayoutKind.Sequential)]
public class Class2 {
	[MarshalAsAttribute(UnmanagedType.Bool)]
	public int f0;

	[MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)]
	public string[] f1;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=100)]
	public string f2;

	// This doesn't work under mono
	//[MarshalAs( UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Marshal1), MarshalCookie = "5")]
	//public object f3;
}

[TestFixture]
public class FieldInfoTest : Assertion
{
	[NonSerialized]
	public int i;

#if NET_2_0
	[Test]
	public void PseudoCustomAttributes () {
		Type t = typeof (FieldInfoTest);

		AssertEquals (1, t.GetField ("i").GetCustomAttributes (typeof (NonSerializedAttribute), true).Length);

		FieldOffsetAttribute field_attr = (FieldOffsetAttribute)(typeof (Class1).GetField ("i").GetCustomAttributes (true) [0]);
		AssertEquals (32, field_attr.Value);

		MarshalAsAttribute attr;

		attr = (MarshalAsAttribute)typeof (Class2).GetField ("f0").GetCustomAttributes (true) [0];
		AssertEquals (UnmanagedType.Bool, attr.Value);

		attr = (MarshalAsAttribute)typeof (Class2).GetField ("f1").GetCustomAttributes (true) [0];
		AssertEquals (UnmanagedType.LPArray, attr.Value);
		AssertEquals (UnmanagedType.LPStr, attr.ArraySubType);

		attr = (MarshalAsAttribute)typeof (Class2).GetField ("f2").GetCustomAttributes (true) [0];
		AssertEquals (UnmanagedType.ByValTStr, attr.Value);
		AssertEquals (100, attr.SizeConst);

		/*
		attr = (MarshalAsAttribute)typeof (Class2).GetField ("f3").GetCustomAttributes (true) [0];
		AssertEquals (UnmanagedType.CustomMarshaler, attr.Value);
		AssertEquals ("5", attr.MarshalCookie);
		AssertEquals (typeof (Marshal1), Type.GetType (attr.MarshalType));
		*/
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void GetValueOnRefOnlyAssembly ()
	{
		Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (FieldInfoTest).Assembly.FullName);
		Type t = assembly.GetType (typeof (RefOnlyFieldClass).FullName);
		FieldInfo f = t.GetField ("RefOnlyField", BindingFlags.Static | BindingFlags.NonPublic);

		f.GetValue (null);
	}
	
	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void SetValueOnRefOnlyAssembly ()
	{
		Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (FieldInfoTest).Assembly.FullName);
		Type t = assembly.GetType (typeof (RefOnlyFieldClass).FullName);
		FieldInfo f = t.GetField ("RefOnlyField", BindingFlags.Static | BindingFlags.NonPublic);

		f.SetValue (null, 8);
	}

	const int literal = 42;

	[Test]
	[ExpectedException (typeof (FieldAccessException))]
	public void SetValueOnLiteralField ()
	{
		FieldInfo f = typeof (FieldInfoTest).GetField ("literal", BindingFlags.Static | BindingFlags.NonPublic);
		f.SetValue (null, 0);
	}

	public int? nullable_field;

	public static int? static_nullable_field;

	[Test]
	public void NullableTests ()
	{
		FieldInfoTest t = new FieldInfoTest ();

		FieldInfo fi = typeof (FieldInfoTest).GetField ("nullable_field");

		fi.SetValue (t, 101);
		AssertEquals (101, fi.GetValue (t));
		fi.SetValue (t, null);
		AssertEquals (null, fi.GetValue (t));

		FieldInfo fi2 = typeof (FieldInfoTest).GetField ("static_nullable_field");

		fi2.SetValue (t, 101);
		AssertEquals (101, fi2.GetValue (t));
		fi2.SetValue (t, null);
		AssertEquals (null, fi2.GetValue (t));
	}
	
	[Test]
	public void NonPublicTests ()
	{		
		Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (FieldInfoTest).Assembly.FullName);
		
		Type t = assembly.GetType (typeof (NonPublicFieldClass).FullName);

		// try to get non-public field
		FieldInfo fi = t.GetField ("protectedField");
		AssertNull (fi);
		// get it for real
		fi = t.GetField ("protectedField", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertNotNull (fi);		
		// get via typebuilder		
		FieldInfo f = TypeBuilder.GetField (t, fi);
		AssertNotNull (f);	
	}
	
#endif
}		
#if NET_2_0
// Helper classes
class RefOnlyFieldClass 
{
	// Helper property
	static int RefOnlyField;
}

class NonPublicFieldClass
{
	protected int protectedField;
}
#endif
}
