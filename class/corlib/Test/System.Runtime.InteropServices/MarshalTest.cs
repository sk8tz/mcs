//
// System.Runtime.InteropServices.Marshal Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class MarshalTest : Assertion
	{
		[StructLayout (LayoutKind.Sequential)]
		class ClsSequential {
			public int field;
		}

		class ClsNoLayout {
			public int field;
		}

		[StructLayout (LayoutKind.Explicit)]
		class ClsExplicit {
			[FieldOffset (0)] public int field;
		}

		[StructLayout (LayoutKind.Sequential)]
		struct StrSequential {
			public int field;
		}

		struct StrNoLayout {
			public int field;
		}

		[StructLayout (LayoutKind.Explicit)]
		struct StrExplicit {
			[FieldOffset (0)] public int field;
		}

		[Test]
		public void ClassSequential ()
		{
			Marshal.SizeOf (typeof (ClsSequential));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ClassNoLayout ()
		{
			Marshal.SizeOf (typeof (ClsNoLayout));
		}

		[Test]
		public void ClassExplicit ()
		{
			Marshal.SizeOf (typeof (ClsExplicit));
		}

		[Test]
		public void StructSequential ()
		{
			Marshal.SizeOf (typeof (StrSequential));
		}

		[Test]
		public void StructNoLayout ()
		{
			Marshal.SizeOf (typeof (StrNoLayout));
		}

		[Test]
		public void StructExplicit ()
		{
			Marshal.SizeOf (typeof (StrExplicit));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArrayType ()
		{
			Marshal.SizeOf (typeof (string[]));
		}

		[Test]
		public void PtrToStringWithNull ()
		{
			AssertEquals (Marshal.PtrToStringAnsi (IntPtr.Zero), String.Empty);
			AssertEquals (Marshal.PtrToStringAnsi (IntPtr.Zero, 0), String.Empty);
			AssertEquals (Marshal.PtrToStringUni (IntPtr.Zero), String.Empty);
			AssertEquals (Marshal.PtrToStringUni (IntPtr.Zero, 0), String.Empty);
		}

		[Test]
		public unsafe void UnsafeAddrOfPinnedArrayElement () {
			short[] sarr = new short [5];
			sarr [2] = 3;

			IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement (sarr, 2);
			AssertEquals (3, *(short*)ptr.ToPointer ());
		}
	}
}

