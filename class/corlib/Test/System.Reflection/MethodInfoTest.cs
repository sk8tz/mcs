//
// System.Reflection.MethodInfo Test Cases
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
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

using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class MethodInfoTest : Assertion
	{
		//		[DllImport ("libfoo", EntryPoint="foo", CharSet=CharSet.Unicode, ExactSpelling=false, PreserveSig=true, SetLastError=true, BestFitMapping=true, ThrowOnUnmappableChar=true)]
		// bug #73161
		[DllImport ("libfoo", EntryPoint="foo", CharSet=CharSet.Unicode, ExactSpelling=false, PreserveSig=true, SetLastError=true)]
		public static extern void dllImportMethod ();

		[MethodImplAttribute(MethodImplOptions.PreserveSig)]
		public void preserveSigMethod () {
		}

		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void synchronizedMethod () {
		}

#if NET_2_0
		[Category ("NotWorking")]
		[Test]
		public void PseudoCustomAttributes ()
		{
			Type t = typeof (MethodInfoTest);

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod ("dllImportMethod").GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			AssertEquals (CallingConvention.Winapi, attr.CallingConvention);
			AssertEquals ("foo", attr.EntryPoint);
			AssertEquals ("libfoo", attr.Value);
			AssertEquals (CharSet.Unicode, attr.CharSet);
			AssertEquals (false, attr.ExactSpelling);
			AssertEquals (true, attr.PreserveSig);
			AssertEquals (true, attr.SetLastError);
			AssertEquals (true, attr.BestFitMapping);
			AssertEquals (true, attr.ThrowOnUnmappableChar);

			PreserveSigAttribute attr2 = (PreserveSigAttribute)((t.GetMethod ("preserveSigMethod").GetCustomAttributes (true)) [0]);

			// This doesn't work under MS.NET
			/*
			  MethodImplAttribute attr3 = (MethodImplAttribute)((t.GetMethod ("synchronizedMethod").GetCustomAttributes (true)) [0]);
			*/
		}
#endif

		[Test]
		public void ByRefInvoke ()
		{
			MethodInfo met = typeof(MethodInfoTest).GetMethod ("ByRefTest");
			object[] parms = new object[] {1};
			met.Invoke (null, parms);
			AssertEquals (2, parms [0]);
		}

		public static void ByRefTest (ref int a1)
		{
			if (a1 == 1)
				a1 = 2;
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetMethodBody_Abstract () {
			typeof (ICloneable).GetMethod ("Clone").GetMethodBody ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetMethodBody_Runtime () {
			typeof (AsyncCallback).GetMethod ("Invoke").GetMethodBody ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetMethodBody_Pinvoke () {
			typeof (MethodInfoTest).GetMethod ("dllImportMethod").GetMethodBody ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetMethodBody_Icall () {
			foreach (MethodInfo mi in typeof (object).GetMethods (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
				if ((mi.GetMethodImplementationFlags () & MethodImplAttributes.InternalCall) != 0)
					mi.GetMethodBody ();
		}

		public static void locals_method () {
			byte[] b = new byte [10];

			unsafe {
				/* This generates a pinned local */
				fixed (byte *p = &b [0]) {
				}
			}
		}

		[Test]
		public void GetMethodBody () {
			MethodBody mb = typeof (MethodInfoTest).GetMethod ("locals_method").GetMethodBody ();

			Assert (mb.InitLocals);
			Assert (mb.LocalSignatureMetadataToken > 0);

			IList<LocalVariableInfo> locals = mb.LocalVariables;

			// This might break with different compilers etc.
			AssertEquals (2, locals.Count);

			Assert ((locals [0].LocalType == typeof (byte[])) || (locals [1].LocalType == typeof (byte[])));
			if (locals [0].LocalType == typeof (byte[]))
				AssertEquals (false, locals [0].IsPinned);
			else
				AssertEquals (false, locals [1].IsPinned);
		}
#endif
	}
}

